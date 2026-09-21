# Tales of Ivory Moss Beta Release Checklist

## Before Building
- Unity opens with no compiler errors.
- `Build/Beta/Apply Release Player Settings` has been run.
- Main Menu is first in Build Settings.
- Campaign scenes, Wrath Mode, and boss scene are enabled.
- No unrelated workspace files are staged.

## Build
- Run `Build/Beta/Windows x64`.
- Output folder: `Builds/TalesOfIvoryMoss_Beta_0.1.0_Windows`.
- Zip the whole folder as `TalesOfIvoryMoss_Beta_0.1.0_Windows.zip`.

## Smoke Test
- Launch `TalesOfIvoryMoss.exe` outside Unity.
- Start New Game and enter the campaign.
- Enter Wrath Mode from the main menu.
- Save with the vendor, quit to menu, and load the slot.
- Die, then test Load Game, Main Menu, and Quit to Desktop.
- Enter the boss scene and confirm music/effects start.

## Friend Build Notes
- This beta is unsigned, so Windows SmartScreen may warn players.
- Share the zip, not only the exe.
- Ask testers to report the scene, action, and exact moment when something breaks.
