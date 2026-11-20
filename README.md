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

## Build
1. Run the **Request Unity Personal License File** workflow in GitHub Actions.
2. Download the `Unity-Activation-File` artifact and use it with your Unity account to obtain the Personal license file.
3. Add the license file contents as the repository Actions secret named `UNITY_LICENSE`.
4. Run **SHOHIN CITY DRIVE - Unity Android Build**. The APK is uploaded as a workflow artifact.

A successful workflow build is not the same as a tested game. The APK must still be installed and tested on a real Android device.

## Project notes
See [PROJECT_RESEARCH.md](PROJECT_RESEARCH.md) for the technical decisions, performance targets, physics plan, asset licensing rules, and known limitations.
