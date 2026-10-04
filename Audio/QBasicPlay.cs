namespace NibblesSharp.Audio;

/// <summary>
/// Represents a musical tone with frequency (in Hertz) and duration (in milliseconds).
/// Frequency 0 represents a rest (pause).
/// </summary>
public record Tone(int Frequency, int DurationMs);

/// <summary>
/// Parser and generator for classic Microsoft QBasic PLAY command macro strings.
/// Supports tempo (T), octave (O, >, <), length (L), notes (A-G, #, +, -), and pauses (P).
/// </summary>
public static class QBasicPlay
{
    /// <summary>
    /// Parses a QBasic PLAY string into a list of playable Tones.
    /// </summary>
    public static List<Tone> Parse(string playScript)
    {
        var tones = new List<Tone>();
        if (string.IsNullOrWhiteSpace(playScript))
            return tones;

        int tempo = 120;       // Quarter notes per minute (32..255)
        int octave = 4;        // Octave 0..6
        int defaultLength = 4; // Note length: 1=whole, 4=quarter, 8=eighth, etc.

        int i = 0;
        string s = playScript.Trim();

        while (i < s.Length)
        {
            char c = char.ToUpperInvariant(s[i]);

            // Whitespace
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // Octave change: O<number>
            if (c == 'O')
            {
                i++;
                int oct = ReadInt(s, ref i);
                if (oct >= 0 && oct <= 6)
                    octave = oct;
                continue;
            }

            // Octave up: >
            if (c == '>')
            {
                if (octave < 6) octave++;
                i++;
                continue;
            }

            // Octave down: <
            if (c == '<')
            {
                if (octave > 0) octave--;
                i++;
                continue;
            }

            // Tempo: T<number>
            if (c == 'T')
            {
                i++;
                int t = ReadInt(s, ref i);
                if (t >= 32 && t <= 255)
                    tempo = t;
                continue;
            }

            // Default length: L<number>
            if (c == 'L')
            {
                i++;
                int l = ReadInt(s, ref i);
                if (l >= 1 && l <= 64)
                    defaultLength = l;
                continue;
            }

            // Music style: MB (Background), MF (Foreground), MN (Normal), ML (Legato), MS (Staccato)
            if (c == 'M')
            {
                i++;
                if (i < s.Length && (char.ToUpperInvariant(s[i]) == 'B' || 
                                     char.ToUpperInvariant(s[i]) == 'F' || 
                                     char.ToUpperInvariant(s[i]) == 'N' || 
                                     char.ToUpperInvariant(s[i]) == 'L' || 
                                     char.ToUpperInvariant(s[i]) == 'S'))
                {
                    i++;
                }
                continue;
            }

            // Pause: P<length>
            if (c == 'P')
            {
                i++;
                int pauseLen = ReadInt(s, ref i);
                if (pauseLen <= 0) pauseLen = defaultLength;

                int durMs = CalculateDuration(tempo, pauseLen);
                tones.Add(new Tone(0, durMs));
                continue;
            }

            // Musical note: A to G
            if (c >= 'A' && c <= 'G')
            {
                i++;
                int semitone = NoteLetterToSemitone(c);

                // Sharp or Flat suffix
                if (i < s.Length && (s[i] == '#' || s[i] == '+'))
                {
                    semitone++;
                    i++;
                }
                else if (i < s.Length && s[i] == '-')
                {
                    semitone--;
                    i++;
                }

                // Explicit length for this note (e.g. C8, E4)
                int noteLen = ReadInt(s, ref i);
                if (noteLen <= 0) noteLen = defaultLength;

                // Dotted note: extends length by 50%
                bool dotted = false;
                if (i < s.Length && s[i] == '.')
                {
                    dotted = true;
                    i++;
                }

                int durMs = CalculateDuration(tempo, noteLen);
                if (dotted)
                {
                    durMs = (int)(durMs * 1.5);
                }

                int freq = CalculateFrequency(octave, semitone);
                tones.Add(new Tone(freq, durMs));
                continue;
            }

            // Any unhandled character, skip
            i++;
        }

        return tones;
    }

    private static int ReadInt(string s, ref int i)
    {
        int start = i;
        while (i < s.Length && char.IsAsciiDigit(s[i]))
        {
            i++;
        }
        if (i > start && int.TryParse(s[start..i], out int val))
            return val;
        return 0;
    }

    private static int NoteLetterToSemitone(char note)
    {
        // Semitone offsets within octave (C = 0, C# = 1, D = 2 ... B = 11)
        return note switch
        {
            'C' => 0,
            'D' => 2,
            'E' => 4,
            'F' => 5,
            'G' => 7,
            'A' => 9,
            'B' => 11,
            _ => 0
        };
    }

    private static int CalculateDuration(int tempo, int length)
    {
        // In QBasic: Tempo is quarter notes per minute (60 * 4 = 240 seconds per whole note).
        // Duration in ms = (240000 / tempo) / length
        if (length <= 0) length = 4;
        return (int)((240000.0 / tempo) / length);
    }

    private static int CalculateFrequency(int octave, int semitone)
    {
        // Equal-tempered formula: Note index 1..84 where 46 is A in octave 3 (440 Hz)
        int noteIndex = octave * 12 + semitone + 1;
        double freq = 440.0 * Math.Pow(2.0, (noteIndex - 46.0) / 12.0);
        return Math.Clamp((int)Math.Round(freq), 37, 32767);
    }
}
