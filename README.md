

## Overview

This project is a port of the core gameplay part of the old version TinyMode's MatchThree sample project, which was based on Unity 2019. It was created for study purposes and to learn about Unity's ECS 1.0 pre and some other new features, such as UIToolkit.

Please note:

- The code in this project is not optimized and is intended for study purposes only. Some test code may still be present.
- This project was developed using Unity 2022.2.1f1 and Entities 1.0.0-pre.15.
- Main game scene path `Assets/_Project/scene/main.unity`.
- UIToolkit for the UI.
- Unity's default 2D sprite for rendering.
- Dotween for the tween system.
- DeAudio for the audio system.

Known issues:

- To make the build version work correctly, sprite atlases must be disabled. There seems to be an issue with sprite atlases and the ecs converted hybrid sprite renderer.
- In IL2CPP build, it appears that the hybrid gameobject does not update positions correctly, but it works fine in a mono build.