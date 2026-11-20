# SHOHIN CITY DRIVE

SHOHIN CITY DRIVE is being rebuilt as a native Android 3D driving game.

## Technology
- Unity 6.3 LTS (pinned editor: 6000.3.28f1)
- C#
- Universal Render Pipeline (URP) 17.3.0
- Unity WheelCollider vehicle physics
- Android ARM64 build target
- GitHub Actions + GameCI for automated APK builds

## Current repository state
The previous Babylon.js browser prototype and old Godot experiment have been removed from the active branch. The new Unity project lives in `UnityProject/`.

The first editor-generated scene is a small procedural city blockout with a temporary physics test chassis, WheelColliders, a chase camera, speed HUD, and touch controls. **The temporary chassis is not a finished realistic car model.** A properly licensed detailed vehicle asset, sound, refined suspension/tire tuning, and production city art are still required.

## Build requirements
Unity Personal is free if the account meets Unity's eligibility terms. GameCI requires a valid Unity Personal license and account credentials.

1. On any available Windows, macOS, or Linux computer, install Unity Hub and sign in to your Unity account.
2. In Unity Hub > Preferences > Licenses, add a free Personal license. Make sure the `.ulf` license file is actually created.
3. In GitHub, open this repository > Settings > Secrets and variables > Actions. Add these repository secrets:
   - `UNITY_LICENSE` — complete contents of the `.ulf` file.
   - `UNITY_EMAIL` — email address for the Unity account.
   - `UNITY_PASSWORD` — Unity account password.
4. Sign out of Unity Hub after copying the license file, as required by the current GameCI Personal-license instructions.
5. Open Actions and manually run **SHOHIN CITY DRIVE - Unity Android Build**. The APK is uploaded as a workflow artifact.

**Phone-only limitation:** the official current GameCI process requires a Unity Personal license file created through Unity Hub. It cannot reliably be generated solely inside the Android ChatGPT/GitHub apps. A computer can be borrowed briefly for the one-time license step; do not send the password or license contents in chat.

A successful workflow build is not the same as a tested game. The APK must still be installed and tested on a real Android device.

## Project notes
See [PROJECT_RESEARCH.md](PROJECT_RESEARCH.md) for technical decisions, performance targets, physics, asset licensing, and known limitations.
