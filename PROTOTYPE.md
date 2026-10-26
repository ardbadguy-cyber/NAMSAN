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
