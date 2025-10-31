<div style="background: #ffe2d7; padding: 16px; border-radius: 4px; margin: 16px 0;">
    The Polybrush package is deprecated and no longer supported in Unity 6.3 (6000.3) and later.
</div>

# Using Polybrush modes

Polybrush provides five brush modes that perform different tasks. They all use the same basic brush settings and mirroring options:

| **Button**                           | **Mode**                    | **Task to perform**                                          |
| ------------------------------------ | --------------------------- | ------------------------------------------------------------ |
| ![Sculpt button](images/icons/Sculpt.png)         | [Sculpt](modes_sculpt.md)   | Moves the vertices of your Mesh in several directions to manipulate its surface. |
| ![Smooth button](images/icons/Smooth.png)         | [Smooth](modes_smooth.md)   | Smooth the differences between vertex positions.             |
| ![Color button](images/icons/Palette.png)        | [Color](modes_color.md)     | Paint colors on the surface of your Mesh.                    |
| ![Texture button](images/icons/Bricks.png)         | [Texture](modes_texture.md) | Paint and blend Textures across your Mesh.                   |
| ![Scatter button](images/icons/FlowerAndGrass.png) | [Scatter](modes_prefab.md)  | Place or scatter Prefabs on the surface of your Mesh.        |

To activate a specific brush mode, click its button on the Mode toolbar:

![The Mode toolbar contains all the buttons listed in the table](images/Polybrush_ModeToolbar.png)

> **Tip:** To see which brush mode is active, look at the toolbar. The active button appears with a darker background.

To deactivate a brush mode, click the mode button that is currently active, or select any standard Unity transform tool (Move, Rotate, Scale).
