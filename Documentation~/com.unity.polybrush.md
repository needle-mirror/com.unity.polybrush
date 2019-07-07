# About Polybrush

Polybrush is a mesh painting, sculpting, and geo-scattering tool for **Unity 2018.3 and later**.

> It is only compatible with Unity and ProBuilder 4 meshes. If you would like to work on Unity terrains, please use the dedicated tool instead.

Polybrush full documentation is available [here](https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages/).

[![Polybrush tutorial video](images/tutorial-video-thumb.png)](https://youtu.be/JQyntL-Z5bM "Polybrush Tutorial Video")

# Installation

From version 1.0 and onwards, Polybrush will only be available from the Package Manager.

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

# Upgrading Polybrush

If you have been using a version of Polybrush prior 1.0, please thoroughly follow these instructions:

   1. Close Unity.
   2. Find the `/ProCore/Polybrush/` folder. It should be located at `<project_assets_folder>/ProCore/Polybrush/`.
   3. Delete the folder.
   4. Open Unity.
   5. Install last version of Polybrush from Package Manager (see [Installing Polybrush](#installing))

As `Z_AdditionalVertexStreams` is now deprecated, Polybrush 1.0 will automatically replace them by the new component `PolybrushMesh` the first time you will be hovering an object with one of Polybrush's tools enabled.

### Batch update Z_AdditionalVertexStreams

**Note:** please skip this section if you haven't been using Additional Vertex Streams with previous versions of Polybrush.

In Polybrush 1.0, a menu item is available in `Tools > Polybrush > Upgrade Z_AdditionalVertexStreams`.

When used, it will go through every scene currently loaded in the Editor and look for `Z_AdditionalVertexStreams` components (even on inactive gameobjects). When one is found, it will be replaced by its new equivalent `PolybrushMesh` component. The internal data is converted during the process so you don't loose anything. Expect your scenes and objects to be marked as dirty, so don't forget to Save after this process.

### Texture Blend mode: update shaders configuration

As of Polybrush 1.0, shader setup (connecting channels with textures) is done directly within the Texture Blending panel of Polybrush. The setup info is stored in the shader's meta file.

We provide a simple way to convert the existing `.pbs.json` files to the new format:

   1. Select your shaders in the Project View.
   2. Go to `Tools > Polybrush > Update Shader Meta`.

This update process above will move the data from the `.pbs.json` into the shader `.meta file.` and delete the old `.pbs.json` file.

# Integrations
### ProBuilder 4

Polybrush 1.0 is fully compatible with ProBuilder 4. To use it, you only need to import ProBuilder 4 via the Package Manager. Interacting with Unity meshes and ProBuilder objects will work identically.

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
                                                           |

# Document revision history

|Date|Reason|
|---|---|
|March 27, 2019| Add new sections: [Installation](#installation), [Upgrading Polybrush](#upgrading-polybrush), [Integrations](#integrations).</br>Update [About](#about). |
|July 12, 2018| Initial Version |
