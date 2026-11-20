# SHOHIN CITY DRIVE — Game Design & Quality Plan

## Product direction
A mobile-first 3D city-driving game with a grounded, premium automotive presentation. It should feel like a driving game, not a toy or a collection of primitive blocks. Target device for first optimization: Android phone with 8 GB RAM, with scalable quality settings for weaker devices.

## Core driving loop
1. Start in a safe city street or driving hub.
2. Choose a car and camera view.
3. Drive through connected streets, intersections and marked routes.
4. Avoid traffic, street furniture and buildings; collisions must stop or deflect the car rather than allow it to pass through geometry.
5. Complete route challenges, earn credits and unlock cars or visual upgrades.

## World and track requirements
- A readable city road network with continuous asphalt, lane markings, curbs, sidewalks and clear intersections.
- A dedicated closed test circuit for vehicle tuning: straights, wide corners, hairpins, braking zones and runoff.
- Buildings must have collision volumes; decorative windows must not be treated as separate obstacles.
- Trees, poles, barriers, signs and traffic vehicles need collision handling.
- Roadside scenery should have visual depth, lighting and varied silhouettes without blocking the driving line.
- Distant city geometry should use lower detail; nearby geometry should retain sharper detail.
- Add a minimap/route indicator only after road layout is stable.

## Vehicle requirements
- Use a properly licensed, optimized 3D car model with separate wheels, glass, lights, mirrors and interior details.
- Front wheels visibly steer; all four wheels rotate with speed; suspension should react to bumps and weight transfer.
- Vehicle direction, steering, braking and reversing must be consistent with the camera and controls.
- Tune acceleration, top speed, braking distance, grip, understeer, oversteer and handbrake drift independently.
- Use stable collision shapes around the car. The vehicle must not sink below the road, clip through buildings or flip the camera.
- Include a chase camera, close chase camera and cockpit/hood view with smooth follow and sensible limits.

## Controls and HUD
- Large touch controls with pointer capture/release; no stuck throttle or steering after a finger leaves the button.
- Gas, brake/reverse, left/right steering and handbrake.
- Keyboard controls for desktop testing.
- Speed display in km/h, gear indicator, pause, restart and camera switch.
- HUD must avoid covering the road or the user's thumbs on small screens.

## Audio and feedback
- Engine sound that changes with RPM/load, tire/skid sound, collision feedback and optional ambient city sound.
- Audio must have a mute control and must not autoplay before user interaction.
- Visual feedback for collision, braking and handbrake without excessive screen shake.

## Performance and reliability
- Load the 3D engine with a visible loading state and a useful error/retry screen.
- Use a CDN fallback or a locally bundled engine if reliable offline play becomes a requirement.
- Cap device pixel ratio on mobile; reduce shadows, foliage and distant detail on low quality.
- Reuse materials and meshes; avoid creating new geometry every frame.
- Pause rendering when the page is hidden; resume safely when visible.
- Handle resize/orientation changes and avoid browser page scrolling while driving.
- Target stable 30 FPS on mid-range phones first; add a 60 FPS high-quality mode only after measurement.

## Quality assurance checklist
- Page opens on Android Chrome and the 3D engine initializes.
- Loading failures show an error message rather than a black screen.
- Car moves forward when gas is pressed; braking does not unexpectedly reverse the car.
- Left and right steering match the on-screen arrows.
- Wheels steer/rotate visibly and remain attached.
- Camera follows the car and never rotates the whole screen.
- Car collides with buildings, curbs, barriers and other vehicles.
- Car cannot pass through solid objects or fall through the road.
- Touch controls release correctly after pointerup, pointercancel, tab switching and interruptions.
- Pause/resume, restart and camera switching work repeatedly.
- Test at multiple viewport sizes and with low/high graphics settings.

## Current implementation snapshot
The active web build uses Babylon.js and a procedural city grid. It contains a green sports-car assembly, a follow camera with camera-distance modes, speed and gear HUD, touch/keyboard controls, basic acceleration/drag/braking/steering, handbrake lateral movement, and simple building collision checks. Recent fixes corrected steering direction, braking behavior, handbrake drift direction, camera distance limits and startup error feedback.

## Not yet complete — do not describe as finished
- A dedicated closed racing circuit and route/challenge system.
- AI traffic and pedestrian-free street activity.
- Full collision coverage for poles, trees, curbs, barriers and traffic.
- Suspension, tire grip curves, robust reverse gear and advanced vehicle dynamics.
- A licensed high-detail external car asset; the current vehicle is procedural geometry.
- Engine/tire/collision audio, damage model, minimap, garage, progression and settings.
- Real-device playtesting and measured FPS on Android hardware.

## Development order
1. Verify reliable startup and controls on the target phone.
2. Finish road geometry, boundaries and collision coverage.
3. Tune vehicle movement and wheel/camera behavior.
4. Add traffic and a dedicated test circuit.
5. Add audio, pause/settings, route objectives and progression.
6. Optimize graphics against measured Android performance.
7. Only then label the build a release candidate.
