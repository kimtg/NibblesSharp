# QBasic Nibbles in C# (.NET 10)

An authentic, faithful modern recreation of the classic 1990 MS-DOS Microsoft QBasic game **Nibbles** (`NIBBLES.BAS` by Rick Raddatz) written in C#.

Runs directly in your terminal with zero flickering, authentic PC-Speaker sound effects, subpixel text rendering, and all 9 original levels!

---

## Features

- **Authentic QBasic Visuals & Text Mode Simulation**:
  - Uses the original CP437/Unicode half-block rendering method (`▀`, `▄`, `█`) to achieve **80x50 virtual resolution** in standard 80x25 terminal text mode.
  - Classic QBasic color scheme: Navy Blue playing arena, Yellow Sammy, Light Magenta Jake, Light Red walls, and Bright White/Red dialogs.
  - Supports both **Color** and **Monochrome** monitor modes.
  - Title screen with the animated flashing **sparkle border**.
  - Classic 10-pass **interlaced dissolving death animation** when a snake crashes.

- **Authentic IBM PC Speaker Sound Engine**:
  - Built-in interpreter for the original QBasic `PLAY` macro strings.
  - Synthesizes 8-bit square wave audio matching the vintage Intel 8253 PIT PC Speaker buzzer.
  - Plays non-blockingly via Windows `winmm.dll` (with `Console.Beep` fallback).
  - Contains all 4 iconic musical cues and sound effects:
    1. **Intro Theme**: `MBT160O1L8CDEDCDL4ECC`
    2. **Round Start**: `T160O1>L20CDEDCDL10ECC`
    3. **Eat Number**: `MBO0L16>CCCE`
    4. **Death Crash**: `MBO0L32EFGEFDC`
  - Press `M` anytime to toggle mute.

- **1-Player and 2-Player (Simultaneous) Gameplay**:
  - **Player 1 (SAMMY)**: Arrow keys (`↑`, `↓`, `←`, `→`)
  - **Player 2 (JAKE)**: `W`, `S`, `A`, `D`

- **All 9 Classic Level Mazes + Endless Mode**:
  - Every wall coordinate, player spawn point, and initial direction is mathematically identical to the original Microsoft source code.
  - Level 1: Open arena
  - Level 2: Horizontal center barrier
  - Level 3: Dual vertical pillars
  - Level 4: Opposing corner barriers
  - Level 5: Enclosed central box with corner gaps
  - Level 6: 7 vertical bars with central corridor
  - Level 7: Dotted vertical dividing line
  - Level 8: Interleaved vertical comb
  - Level 9: Diagonal barriers
  - Level 10+: Staggered obstacle field

- **Autonomous AI Demo Mode (`--demo`)**:
  - Built-in BFS pathfinding and flood-fill survival AI that plays the game autonomously so you can watch and verify gameplay without touching the keyboard.

- **Comprehensive Unit Tests**:
  - 18 automated unit tests verifying PLAY string parser, sound frequencies, arena boundaries, coordinate conversions, level maps, and AI pathfinding.

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/) (or .NET 8 / 9)
- Any terminal supporting UTF-8 (Windows Terminal, PowerShell, CMD, VS Code Terminal, etc.)

---

## How to Run

### Interactive Game (Standard)
```bash
dotnet run
```
Follow the classic QBasic prompts to select:
1. Number of players (1 or 2)
2. Skill level (1 to 100, where 1=Novice, 90=Expert, 100=Twiddle Fingers)
3. Increase speed during play (Y or N)
4. Color or Monochrome monitor (C or M)

### Autonomous AI Demonstration Mode
Watch the computer play the game on its own:
```bash
dotnet run -- --demo
```

### Quick Start (Skip intro prompts)
```bash
dotnet run -- --quick --players 1 --skill 50
```

### Command-Line Options
```text
Usage:
  dotnet run [options]

Options:
  --demo            Run autonomous AI demonstration mode (press Esc or key to exit)
  --quick           Skip intro and setup screens, start directly with defaults
  --players <1|2>   Set number of players (1 or 2)
  --skill <1-100>   Set game speed/skill level (1=Novice, 90=Expert, 100=Twiddle Fingers)
  --color           Enable color mode (Default: Blue background, Yellow/Magenta snakes)
  --mono            Enable monochrome monitor mode
  --mute            Start with sound muted
  --no-accel        Disable game speed acceleration across levels
  -h, --help        Show this help message
```

---

## Game Controls

| Action | Player 1 (Sammy) | Player 2 (Jake) |
|---|---|---|
| **Move Up** | `↑` (Up Arrow) | `W` |
| **Move Down** | `↓` (Down Arrow) | `S` |
| **Move Left** | `←` (Left Arrow) | `A` |
| **Move Right** | `→` (Right Arrow) | `D` |
| **Pause / Resume** | `P` | `P` |
| **Toggle Mute** | `M` | `M` |
| **Exit Game** | `Esc` | `Esc` |

---

## Running the Unit Tests

```bash
dotnet test
```

Executes 18 automated tests covering all core game logic and sound systems.
