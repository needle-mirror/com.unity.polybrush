# Getting Started With Polybrush

First time using Polybrush?  Start here for an overview of the essentials.

> If you've encountered a bug or other technical issue, please post on the official [**Support Forum**](https://forum.unity.com/forums/world-building.146/). We respond very quickly, and the community benefits as well.

---

### 1) Open the Polybrush Window

From the Unity menu bar, choose `Tools > Polybrush > Polybrush Window`. This opens the [**Polybrush Window**](/interface/), which you will use for 99% of **Polybrush** functionality.

<div style="text-align:center">
<img src="../images/PolybrushPanel_Off.png" alt="Polybrush Window">
</div>

---

### 2) Choose a Mesh to Edit

**Polybrush** will automatically work on any mesh, but for now we'll use a Unity primitive mesh.

To create a Plane primitive, in the Unity menu bar choose `Game Object > 3D Object > Plane`

<div style="text-align:center">
<img src="../images/GettingStarted_Plane.png" alt="Plane">
</div>

> You can also use [**ProBuilder**](http://www.procore3d.com/probuilder) meshes for an even more powerful workflow!

---

### 3) Sculpt Your Mesh

Let's make this mesh look more interesting!

1. Activate [**Sculpt Mode**](/modes/sculpt/) by clicking it's icon (![sculpt](images/icons/Sculpt.png)) in the [**Mode Toolbar**](/interface/#mode-toolbar).
1. Hover your mouse over the Plane, and `Left Click` to "pull", `Shift Left Click` to "push" geometry.
1. To smooth out hard edges, activate [**Smoothing Mode**](modes/smooth) (![smoothing](images/icons/Smooth.png)) and brush the mesh with `Left Click`.

<div style="text-align:center">
<img src="../images/ModeExamples_Sculpt.png" alt="Sculpting">
</div>

---

### 4) Paint Pretty Colors

Danger, Danger! The default Unity material won't show [**Vertex Coloring**](modes/color), so let's create a new material that will:

1. In your Project Panel, right-click and choose `Create > Material`, then give it a really cool name
1. With the new material selected, look at the top of the Inspector Panel- see that drop-down labeled "Shader"?  Click it, and choose `Polybrush > Standard Vertex Color`
1. Apply this material to the mesh you'd like to paint

> Hey custom shader people! You can use **any** shader, as long as it supports vertex colors

Now we're ready to paint vertex colors:

1. Activate [**Color Painting Mode**](modes/color) by clicking it's icon (![color-painting](images/icons/Palette.png)) in the [**Mode Toolbar**](/interface/#mode-toolbar).
1. Click to select a color from the [**Color Palette**](modes/color/#color-palette) buttons at the bottom of the [**Polybrush Panel**](interface).
1. Hover your mouse over the Plane, and `Left Click` to paint, `Shift Left Click` to erase.

<div style="text-align:center">
<img src="../images/ModeExamples_Color.png" alt="Colors">
</div>

---

### 5) Blend Fancy Textures

[**Texture Blending**](modes/texture) requires a special shader- you can create your own, or use ours:

1. In your Project Panel, right-click and choose `Create > Material`.
1. With the new material selected, look at the top of the Inspector Panel- see that drop-down labeled "Shader"? Click it, and choose `Polybrush > Standard Texture Blend`.
1. Apply this material to the mesh you'd like to paint.

> Prefer your custom shader? You can use **any** shader with Polybrush - see [Writing Texture Blend Shaders](/shaders) for more information.

We can now paint and blend textures on the mesh:

1. Activate [**Texture Blending Mode**](modes/texture) by clicking it's icon (![texture-blending](images/icons/Bricks.png)) in the [**Mode Toolbar**](/interface/#mode-toolbar).
1. Click on a texture in the bottom of the [**Polybrush Panel**](panel-overview) to select it.
1. Hover your mouse over the Plane, and `Left Click` to paint, `Shift Left Click` to erase the selected texture.

<div style="text-align:center">
<img src="../images/ModeExamples_Texture.png" alt="Texture">
</div>

---

### 5) Place Detail Meshes

1. Activate [**Mesh Placement Mode**](modes/place) by clicking it's icon (![place](images/icons/FlowerAndGrass.png)) in the [**Mode Toolbar**](/interface/#mode-toolbar).
1. Click on a Prefab in the bottom of the [**Polybrush Panel**](interface) to select it.
1. Hover your mouse over the Plane, and `Left Click` to paint, `Shift Left Click` to erase the selected texture.

<div style="text-align:center">
<img src="../images/ModeExamples_Place.png" alt="Texture">
</div>

---

**Congratulations**, you've learned the essentials of Polybrush!