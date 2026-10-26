# B401 playable blockout

Unity 6000.3.27f1 / URP / Input System (existing project dependencies).

1. Open the repository root in Unity Hub (not the older project copy).
2. Wait for script compilation. Select **NAMSAN > Create B401 Prototype**.
3. Save any existing scene when prompted. A uniquely named scene is created in Assets/Scenes; existing scenes are preserved.
4. Press Play and click the Game view. WASD moves, mouse looks, E interacts, F toggles the collected lighter, Escape releases the cursor.
5. Look at the small lighter on the desk and press E. Approach and look at the door, press E, then toggle F to reveal the placeholder silhouette at the corridor end. Approach it to make it disappear.

Room geometry is generated only while playing; stopping removes runtime objects. Run the menu again only to create another test scene. This is a functional blockout, not final art or the complete prologue. No damage, collection progression, sound, or endings are implemented. The lighter can be picked up unlit as the initial tutorial exception; later collectible rules are not implemented yet.

Manual validation required on Windows: compile without Console errors; verify collision and mouse capture; collect lighter; open door; toggle silhouette visibility; stop and replay to reset. Unity Editor is unavailable in the cloud workspace, so these checks have not been executed there.

# B1 floor layout

Select **NAMSAN > Create B1 Floor** outside Play mode. This saves an editor-visible blockout to a new unique `Assets/Scenes/B1_Layout.unity` path. Existing scenes are not overwritten; shared prototype materials are created once in `Assets/Namsan/Generated`. Allow Unity to finish compiling before using the menu.

Plan viewed from above (+Z at the top):

```
left                                    right
B110     B109     B108     B107     B106
stairs -------- central corridor ------- end
B105     B104     B103     B102     B101
```

Each room is approximately 6 x 6m, corridor width 3m, ceiling height 3m. These are provisional dimensions inferred from the diagram, not historical measurements. B1 only: the left stair access is a closed landing; no descending flight or B2 is generated. Ten rooms have functional sliding placeholder doors, number signs, tables, chairs, and ceiling lighting. No characters, story triggers, lighter, or finished art in this scene.

Press Play, click Game view. Player starts inside B101 facing the corridor door. WASD/mouse move and look; E toggles doors within 2.6m while looking at them; Escape releases the cursor. Walk through all rooms and the left landing. Stop Play to reset runtime door positions. Scene geometry remains editable outside Play mode.

Validation pending in Windows Unity: verify compilation, B101 exit, all ten doors from both sides, wall collisions, sign readability on both sides, left landing boundary, and reset after stopping/restarting Play. The cloud has no Unity Editor, so runtime and rendering results are not yet verified.

# Full B1 atmosphere (updated)

Select **NAMSAN > Create B1 Atmosphere**, then Play. The older **Create B101 Atmosphere** menu is retained as an alias. This creates a separate, uniquely named `B1_Atmosphere` scene using the B1 layout. Existing scenes and the basic floor menu remain available. The builder saves generated procedural texture PNGs and shared materials in `Assets/Namsan/Generated/Atmosphere`. Repeated generation reuses those assets, preserving any user edits; to make a separate variation, duplicate materials first. Scene objects are editable outside Play mode.

The art pass now covers all ten rooms and the entire central corridor. It adds weathered plaster and green lower paint, metal trim, floor joints, riveted hinged iron doors with real barred viewing apertures and conventional lever handles on both faces, a room-number plaque, exposed pipes, vent, filing cabinet, desk files, a simplified rotary-style telephone, ashtray, clock, and warmer tube fixtures. These are procedural prototype shapes and textures, not final photorealistic assets or verified historical reconstruction. No external asset packs or network downloads are required. Only the principal B101 ceiling light gains soft shadows; other lights remain unshadowed.

The left landing sign orientation is corrected in newly generated scenes (existing saved scenes are unchanged). B2 and story events remain excluded. Controls are unchanged. Small props are deliberately noncolliding; the cabinet has collision. Door details are parented to the hinge root and move together. E smoothly opens/closes each leaf inward by 100 degrees. Panel and bar colliders leave the viewing aperture clear; interactions resolve from child colliders to the door root. Handles are decorative and move with the door; E operates the door without a separate locking mechanic. The original basic floor menu still generates the older sliding blockout doors for comparison.

Windows Unity checks still required: script compilation; generate the new scene; inspect floors/walls and prop placement on both rows; open and close all ten doors from both sides; verify handles, rivets and bars move with each door and E remains usable; look through the barred apertures; inspect all number plaques from corridor; check stair sign from corridor; stop/restart Play; generate a second scene to check shared material reuse and unique naming. No Unity Editor is installed in this cloud, so no rendering or runtime pass is claimed.

# Existing B1 scenes: crouching and sprinting

Pull the updated `B1Explorer.cs`, stop Play, and allow Unity to compile. Open your existing B1 scene and press Play; **no scene regeneration is needed**. All previously generated B1 layout/atmosphere scenes already reference this script (the older B401-only prototype uses a different controller).

- C toggles crouch/stand. Capsule height changes from its saved standing height (normally 1.8m) to 1.1m, feet stay fixed, camera lowers smoothly, and speed drops to 1.4m/s. A solid overhead obstruction prevents standing; press C again after moving clear.
- Hold either Shift while moving to run at 5m/s (walk: 2.8m/s). Crouching takes priority over running.
- Running on the ground adds gentle vertical/lateral camera movement and roll. Stopping, walking, hitting a wall, crouching or releasing the cursor returns the camera smoothly to rest. Camera motion uses actual horizontal displacement, not just held keys.
- Select Player > B1 Explorer in the Inspector to tune speeds, crouch height and camera motion. Set Run Bob Amplitude to 0 to disable running camera motion, including roll.

Runtime checks pending on Windows: use an existing saved scene; verify C lowers both camera and collider without moving feet; walk/crouch/run through open doors; verify Shift speed and moving-only bob; verify bob settles on release and against a wall; place a temporary low solid ceiling to verify stand clearance and recovery after leaving it; verify E and Esc still work and stop/replay resets posture. Cloud verification is limited to code review and Git whitespace checks because Unity Editor is unavailable.

# B101 collectible lighter in existing scenes

Pull and let Unity compile, then play the same B1 scene. `B1Explorer` initializes a runtime `B1Lighter` component automatically; no scene regeneration or Inspector wiring is required. This does not alter saved scene objects. The original B401-only controller is separate.

A gold rectangular engraved lighter, inspired by a luxury hinged-cap design, appears at `(11.5, 0.835, -4.55)` on the standard B101 desk. This is procedural prototype geometry, not a licensed branded asset or exact product replica. If you have manually moved the desk, the fixed pickup position must be adjusted in code. Look down at it within 2.1m and press E to collect. It disappears from the table and appears at the lower-right of the camera. F opens the cap and ignites after opening; F again extinguishes and closes. Opening/closing plays a synthesized metallic click, not a recording of the branded lighter. A flickering warm point light illuminates nearby surfaces. No hand model, fuel system, save persistence, or ghost/item progression is included. Stopping and restarting Play resets the pickup.

All held geometry is noncolliding so it cannot block E interaction or the stand-clearance check. The desk pickup has one forgiving above-table collider. Existing C/Shift movement and camera motion remain in effect. Cursor release prevents pickup/toggle input while allowing the lid to finish animating.

Windows validation pending: in an already saved B1 scene, verify the lighter is visible on the desk; try E from outside reach and behind a wall; approach and collect exactly once; ensure F before collection does nothing; toggle repeatedly and check cap/flame/light/click; move/crouch/run and interact with doors while holding it; Esc releases input; stop/replay resets to table. Cloud checks: code inspection and `git diff --check` only; Unity runtime/render/audio checks are not executed here.

Running now extinguishes the lighter immediately (flame and light), leaving its lid open. It stays extinguished after stopping; press F to reignite, then F again to close. F is ignored while actually running. Holding Shift while stationary, crouching, or pushing directly against a wall does not count as running. Uses the same actual grounded movement condition as camera bob. Manual verification pending: lit -> run -> stop (stays out) -> F (relights); F while running cannot relight; Shift alone doesn't extinguish. Existing saved B1 scenes receive this script change without regeneration.
