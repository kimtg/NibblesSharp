using System.Runtime.InteropServices;

namespace NibblesSharp.Audio;

/// <summary>
/// Authentic PC Speaker sound engine for QBasic Nibbles.
/// Generates 8-bit square waves matching vintage IBM PC PIT 8253 audio
/// and plays asynchronously via Win32 PlaySound (or background Console.Beep fallback).
/// </summary>
public static class SoundEngine
{
    private static bool _isMuted = false;
    private static readonly object _lock = new();
    private static CancellationTokenSource? _currentCts;

    [DllImport("winmm.dll", EntryPoint = "PlaySoundW", SetLastError = true)]
    private static extern bool PlaySound(byte[]? pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_SYNC = 0x0000;
    private const uint SND_ASYNC = 0x0001;
    private const uint SND_NODEFAULT = 0x0002;
    private const uint SND_MEMORY = 0x0004;
    private const uint SND_PURGE = 0x0040;

    public static bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            if (_isMuted) Stop();
        }
    }

    public static void ToggleMute()
    {
        IsMuted = !IsMuted;
    }

    /// <summary>
    /// Plays a QBasic PLAY macro string asynchronously in the background.
    /// Does not block the game loop.
    /// </summary>
    public static void PlayAsync(string playScript)
    {
        if (_isMuted || string.IsNullOrWhiteSpace(playScript))
            return;

        Task.Run(() => PlayInternal(playScript, isAsync: true));
    }

    /// <summary>
    /// Plays a QBasic PLAY macro string synchronously.
    /// </summary>
    public static void PlaySync(string playScript)
    {
        if (_isMuted || string.IsNullOrWhiteSpace(playScript))
            return;

        PlayInternal(playScript, isAsync: false);
    }

    /// <summary>
    /// Stops any currently playing audio immediately.
    /// </summary>
    public static void Stop()
    {
        lock (_lock)
        {
            _currentCts?.Cancel();
            _currentCts = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    PlaySound(null, IntPtr.Zero, SND_PURGE);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }

    private static void PlayInternal(string playScript, bool isAsync)
    {
        CancellationTokenSource cts;
        lock (_lock)
        {
            _currentCts?.Cancel();
            _currentCts = new CancellationTokenSource();
            cts = _currentCts;
        }

        try
        {
            var tones = QBasicPlay.Parse(playScript);
            if (tones.Count == 0 || cts.Token.IsCancellationRequested)
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                byte[] wavData = SynthesizeWav(tones);
                if (wavData.Length > 0 && !cts.Token.IsCancellationRequested)
                {
                    uint flags = SND_MEMORY | SND_NODEFAULT | (isAsync ? SND_ASYNC : SND_SYNC);
                    PlaySound(wavData, IntPtr.Zero, flags);
                    return;
                }
            }

            // Fallback for non-Windows or if WAV failed: Console.Beep
            foreach (var tone in tones)
            {
                if (cts.Token.IsCancellationRequested || _isMuted) break;

                if (tone.Frequency > 37 && tone.Frequency < 32767)
                {
                    try
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            Console.Beep(tone.Frequency, Math.Max(10, (int)(tone.DurationMs * 0.85)));
                        }
                    }
                    catch
                    {
                        // Some systems don't support Console.Beep
                    }
                }
                else
                {
                    Thread.Sleep(tone.DurationMs);
                }
            }
        }
        catch
        {
            // Audio error should never crash the game
        }
    }

    /// <summary>
    /// Synthesizes a list of tones into an authentic 8-bit mono PC Speaker square-wave WAV file.
    /// </summary>
    private static byte[] SynthesizeWav(List<Tone> tones, int sampleRate = 22050)
    {
        using var pcmStream = new MemoryStream();

        foreach (var tone in tones)
        {
            if (tone.DurationMs <= 0) continue;

            // QBasic Music Normal (MN): 7/8 note duration tone, 1/8 note duration rest articulation gap
            int totalSamples = (int)(sampleRate * (tone.DurationMs / 1000.0));
            int toneSamples = tone.Frequency > 0 ? (int)(totalSamples * 0.875) : 0;
            int restSamples = totalSamples - toneSamples;

            if (toneSamples > 0 && tone.Frequency >= 37)
            {
                double period = sampleRate / (double)tone.Frequency;
                // Ramp-in/out smoothing to prevent DC pop while keeping authentic sharp square timbre
                int rampLength = Math.Min(30, toneSamples / 4);

                for (int i = 0; i < toneSamples; i++)
                {
                    // Square wave: +36 / -36 around baseline 128
                    double square = (Math.Sin(2.0 * Math.PI * i / period) >= 0) ? 1.0 : -1.0;

                    // Envelope
                    double amp = 36.0;
                    if (i < rampLength)
                        amp *= (double)i / rampLength;
                    else if (i > toneSamples - rampLength)
                        amp *= (double)(toneSamples - i) / rampLength;

                    byte sample = (byte)(128 + (int)(square * amp));
                    pcmStream.WriteByte(sample);
                }
            }
            else
            {
                // Rest / Pause
                for (int i = 0; i < toneSamples; i++)
                    pcmStream.WriteByte(128);
            }

            // Articulation silence gap
            for (int i = 0; i < restSamples; i++)
                pcmStream.WriteByte(128);
        }

        byte[] pcmData = pcmStream.ToArray();
        if (pcmData.Length == 0) return Array.Empty<byte>();

        // Build 44-byte WAV header
        using var wavStream = new MemoryStream();
        using var bw = new BinaryWriter(wavStream);

        bw.Write("RIFF"u8);
        bw.Write(36 + pcmData.Length);
        bw.Write("WAVE"u8);
        bw.Write("fmt "u8);
        bw.Write(16);             // Subchunk1Size (16 for PCM)
        bw.Write((short)1);       // AudioFormat (1 = PCM)
        bw.Write((short)1);       // NumChannels (1 = Mono)
        bw.Write(sampleRate);     // SampleRate
        bw.Write(sampleRate);     // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
        bw.Write((short)1);       // BlockAlign
        bw.Write((short)8);       // BitsPerSample (8 bit)
        bw.Write("data"u8);
        bw.Write(pcmData.Length);
        bw.Write(pcmData);

        return wavStream.ToArray();
    }
}
