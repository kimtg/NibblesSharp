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

- **1-Player, 1-Player vs. AI, and 2-Player Gameplay**:
  - **Player 1 (SAMMY)**: Arrow keys (`↑`, `↓`, `←`, `→`)
  - **Player 2 (JAKE)**: `W`, `S`, `A`, `D` (or autonomous AI in `--vs-ai` mode)
  - **Responsive Input Buffering**: 2-step input queue for razor-sharp cornering without dropped turns.
  - **Persistent High Scores Leaderboard**: Top-10 scores recorded and displayed (`--scores`).

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

- **Autonomous AI & Vs-AI Mode (`--demo` / `--vs-ai`)**:
  - Built-in BFS pathfinding, cavity trap lookahead, and tail-following survival heuristics that navigate mazes smoothly.

- **Comprehensive Unit Tests**:
  - 35 automated unit tests verifying PLAY string parser, sound frequencies, arena boundaries, input buffering, high scores, coordinate conversions, level maps, and AI pathfinding.

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
1. Game Mode: (1) Solo, (2) 1 Player vs Computer AI, (3) 2 Players
2. Starting Level (1 to 10)
3. Skill level (1 to 100, where 1=Novice, 90=Expert, 100=Twiddle Fingers)
4. Increase speed during play (Y or N)
5. Color or Monochrome monitor (C or M)

### 1-Player vs. Computer AI
```bash
dotnet run -- --vs-ai
```

### Autonomous AI Demonstration Mode
Watch the computer play the game on its own:
```bash
dotnet run -- --demo
```

### View High Scores Leaderboard
```bash
dotnet run -- --scores
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
  --vs-ai           Play 1 Player against Computer AI (Jake)
  --quick           Skip intro and setup screens, start directly with defaults
  --players <1|2>   Set number of players (1 or 2)
  --level <1-10>    Set starting maze level (1 to 10)
  --skill <1-100>   Set game speed/skill level (1=Novice, 90=Expert, 100=Twiddle Fingers)
  --scores          Display high scores leaderboard
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
| **Pause / Resume** | `P` or `Space` | `P` or `Space` |
| **Toggle Mute** | `M` | `M` |
| **Exit Game** | `Esc` | `Esc` |

---

## Running the Unit Tests

```bash
dotnet test
```

Executes 35 automated tests covering all core game logic, input buffering, high scores, sound systems, and AI heuristics.
