# Handicap

Per-player handicap for the stock versus modes: give someone a head start in wins
or extra lives, to even out a game between players of different levels. A life bar
is drawn above the archers concerned.

A mod for **FortRise 5** (>= 5.3.3). The FortRise 4 version (`tf-mod-fortrise-handicap`) is no longer maintained: fixes and new features only land in this repository.

## Installation

1. Install FortRise 5 and start the game through `FortRise.exe`.
2. Copy `release/handicap` (or the shipped folder) into `<TowerFall>/FortRise/Mods/`.

Settings are under **Options > Mods > Handicap**.
Data and log files live in `<TowerFall>/FortRise/Saves/Handicap/` and `<TowerFall>/FortRise/Logs/`.

## Usage

Works in **Last Man Standing**, **Head Hunters** and **Team Deathmatch**.

> **Opening the popup**: on the versus screen, with the relevant mode selected,
> press **Y** (the "arrows" button on the controller) on the mode button. A hint is
> shown under the button. The popup locks the menu while it is open (no going back,
> no starting the match); **A** or **B** closes it.

The popup sets, **per player**, the head start in wins and the number of lives:

| Input | Effect |
|-------|--------|
| Up / Down | switch player |
| Alt (RB) | switch between the "wins" (V:) and "lives" fields |
| Left / Right | adjust the value |
| A or B | close |

The `Y: HANDICAP/LIFE` hint only appears in the three supported modes, and so does
the life bar: it will not show up in a mode added by another mod.

## Settings

| Setting | Purpose |
|---------|---------|
| Handicap: Immunity on respawn (seconds) | immunity window after respawning |

## Build / deployment

| Script | Purpose |
|--------|---------|
| `script/release.bat` | build, then assemble into `release/` |
| `script/deploy.bat` | copy `release/` into the TowerFall `Mods` folder |
| `script/release_deploy.bat` | both, one after the other |

Paths (game folder, module name) are set in `script/config.bat`.
