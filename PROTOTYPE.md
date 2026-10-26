# B401 playable blockout

Unity 6000.3.27f1 / URP / Input System (existing project dependencies).

1. Open the repository root in Unity Hub (not the older project copy).
2. Wait for script compilation. Select **NAMSAN > Create B401 Prototype**.
3. Save any existing scene when prompted. A uniquely named scene is created in Assets/Scenes; existing scenes are preserved.
4. Press Play and click the Game view. WASD moves, mouse looks, E interacts, F toggles the collected lighter, Escape releases the cursor.
5. Look at the small lighter on the desk and press E. Approach and look at the door, press E, then toggle F to reveal the placeholder silhouette at the corridor end. Approach it to make it disappear.

Room geometry is generated only while playing; stopping removes runtime objects. Run the menu again only to create another test scene. This is a functional blockout, not final art or the complete prologue. No damage, collection progression, sound, or endings are implemented. The lighter can be picked up unlit as the initial tutorial exception; later collectible rules are not implemented yet.

Manual validation required on Windows: compile without Console errors; verify collision and mouse capture; collect lighter; open door; toggle silhouette visibility; stop and replay to reset. Unity Editor is unavailable in the cloud workspace, so these checks have not been executed there.
