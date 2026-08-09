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

The popup sets the head start in wins and the number of lives **per player**, plus
the immunity window shared by everyone:

| Input | Effect |
|-------|--------|
| Up / Down | switch player |
| Alt (RB) | cycle through the fields: VICTORY, LIFE, IMMUNITY |
| Left / Right | adjust the value |
| A or B | close |

Immunity is a single value, so it sits on its own line under the player list
(`IMMUNITY (ALL)`) rather than being repeated on every row.

The `Y: HANDICAP/LIFE` hint only appears in the three supported modes, and so does
the life bar: it will not show up in a mode added by another mod.

### Life display

A bar of segments is drawn above each archer. Past **8 lives** the bar would be
unreadable and spill well beyond the archer, so it is replaced by a plain counter.

The arrow counter is hidden during the immunity window that follows a respawn.

## Settings

| Setting | Purpose |
|---------|---------|
| Handicap: Victories for all players | head start in wins, applied to every player |
| Handicap: Lives for all players | number of lives, applied to every player |
| Handicap: Immunity on respawn (seconds) | immunity window after respawning |

### Settings and popup are linked

They act on the same values, but the popup is per player while a setting is global:

- changing a **setting** applies the value to **every** player;
- changing a value in the **popup** only updates the setting when **all active
  players share it** — a single global value cannot represent four different ones,
  and overwriting it would show something misleading.

Immunity has no such issue: one value on both sides, always in sync.

Every change is written to disk immediately. FortRise only saves settings when
leaving the game's Options menu, so a value changed in the popup — or right before
quitting — used to be lost.

## Build / deployment

| Script | Purpose |
|--------|---------|
| `script/release.bat` | build, then assemble into `release/` |
| `script/deploy.bat` | copy `release/` into the TowerFall `Mods` folder |
| `script/release_deploy.bat` | both, one after the other |

Paths (game folder, module name) are set in `script/config.bat`.
