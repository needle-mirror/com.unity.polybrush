# About Polybrush

Mesh painting, sculpting, and geo-scattering tool for Unity.

[Intro to Polybrush Video](https://youtu.be/JQyntL-Z5bM)

[Full Documentation](https://www.procore3d.com/docs/polybrush)

> IMPORTANT: Polybrush is for working with meshes, not terrain. Use the Unity Terrain tool for modeling terrains.

# Installing Polybrush

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html). 

# Using Polybrush

## 1) Open the Polybrush Window

From the Unity menu bar, choose `Tools > Polybrush > Polybrush Window`. This opens the [**Polybrush Window**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface/), which you will use for 99% of **Polybrush** functionality.

![interface exmaple](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/PolybrushPanel_Off.png)

## 2) Choose a Mesh to Edit

**Polybrush** will automatically work on any mesh, but for now we'll use a Unity primitive mesh.

To create a Plane primitive, in the Unity menu bar choose `Game Object > 3D Object > Plane`

![starter plane](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/GettingStarted_Plane.png)

> You can also use [**ProBuilder**](http://www.procore3d.com/probuilder) meshes for an even more powerful workflow!

## 3) Sculpt Your Mesh

Let's make this mesh look more interesting!

1. Activate [**Sculpt Mode**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/sculpt/) by clicking it's icon (![sculpt](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/icons/Sculpt.png)) in the [**Mode Toolbar**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface/#mode-toolbar).
1. Hover your mouse over the Plane, and `Left Click` to "pull", `Shift Left Click` to "push" geometry.
1. To smooth out hard edges, activate [**Smoothing Mode**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/smooth) (![smoothing](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/icons/Smooth.png)) and brush the mesh with `Left Click`.

![sculpted plane](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/ModeExamples_Sculpt.png)

## 4) Paint Pretty Colors

Danger, Danger! The default Unity material won't show [**Vertex Coloring**](modes/color), so let's create a new material that will:

1. In your Project Panel, right-click and choose `Create > Material`, then give it a really cool name
1. With the new material selected, look at the top of the Inspector Panel- see that drop-down labeled "Shader"?  Click it, and choose `Polybrush > Standard Vertex Color`
1. Apply this material to the mesh you'd like to paint

> Hey custom shader people! You can use **any** shader, as long as it supports vertex colors

Now we're ready to paint vertex colors:

1. Activate [**Color Painting Mode**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/color) by clicking it's icon (![color-painting](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/icons/Palette.png)) in the [**Mode Toolbar**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface/#mode-toolbar).
1. Click to select a color from the [**Color Palette**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/color/#color-palette) buttons at the bottom of the [**Polybrush Panel**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface).
1. Hover your mouse over the Plane, and `Left Click` to paint, `Shift Left Click` to erase.

![colored plane](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/ModeExamples_Color.png)

## 5) Blend Fancy Textures

[**Texture Blending**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/texture) requires a special shader- you can create your own, or use ours:

1. In your Project Panel, right-click and choose `Create > Material`.
1. With the new material selected, look at the top of the Inspector Panel- see that drop-down labeled "Shader"? Click it, and choose `Polybrush > Standard Texture Blend`.
1. Apply this material to the mesh you'd like to paint.

> Prefer your custom shader? You can use **any** shader with Polybrush - see [Writing Texture Blend Shaders](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/shaders) for more information.

We can now paint and blend textures on the mesh:

1. Activate [**Texture Blending Mode**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/texture by clicking it's icon ( ![texture-blending](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/icons/Bricks.png) ) in the [**Mode Toolbar**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface/#mode-toolbar).
1. Click on a texture in the bottom of the [**Polybrush Panel**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/panel-overview) to select it.
1. Hover your mouse over the Plane, and `Left Click` to paint, `Shift Left Click` to erase the selected texture.

![textured plane](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/ModeExamples_Texture.png)

## 5) Place Detail Meshes

1. Activate [**Mesh Placement Mode**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/modes/place/) by clicking it's icon (![place](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/icons/FlowerAndGrass.png)) in the [**Mode Toolbar**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface/#mode-toolbar).
1. Click on a Prefab in the bottom of the [**Polybrush Panel**](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/interface) to select it.
1. Hover your mouse over the Plane, and `Left Click` to paint, `Shift Left Click` to erase the selected texture.

![place example](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/images/ModeExamples_Place.png)


# Technical details
## Requirements

This version of Polybrush is compatible with the following versions of the Unity Editor:

* 2018.1 and later (recommended)

## Package contents

**Polybrush** includes a few different blend materials to get you started:

| Name                                | Description                                                                                                                             |
| -                                   | -                                                                                                                                       |
| **Standard Texture Blend**          | A PBR enabled material with support for blending up to 12 different textures.                                                           |
| **Standard Texture Blend Bump**     | A PBR enabled material with support for blending up to 4 different textures with normal maps.                                           |
| **TriPlanar Texture Blend**         | A PBR enabled material with support for blending up to 4 textures and automatically projects UV coordinates.                            |
| **TriPlanar Texture Blend Legacy**  | A Blinn-Phong lighting pipeline (legacy) material with support for blending up to 4 textures and automatically projects UV coordinates. |
| **Unlit Texture Blend**             | A simple unlit material with support for blending up to 6 textures.                                                                     |

## Document revision history

|Date|Reason|
|---|---|
|July 12, 2018 | Initial Version |
