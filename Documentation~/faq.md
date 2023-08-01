# Troubleshooting

These are some of the errors you might encounter and how to fix them:

* [Polybrush does not appear](#missing-menu) in the **Tools** menu after installation.
* [Painting colors or Textures](#paint-fail) doesn't work.
* [Can't find](#find-shaders) example Shaders.
* [Errors when importing](#import-errors) example Shaders.
* [Mesh automatically resets](#resets) to a source Mesh after few seconds when baking lightmaps (or if the Lightmap's [Auto Generate](https://docs.unity3d.com/Manual/GlobalIllumination.html) property is enabled).



<a name="missing"></a>

## Polybrush does not appear in the Tools menu after installation

This might happen when an exception occurs in the Editor. Check the [Console](https://docs.unity3d.com/Manual/Console.html) to see whether you see the exception error.

<a name="paint-fail"></a>

## Painting colors or Textures doesn't work

If you see the brush when you hover over the Mesh, the tool is working correctly. If the Texture or color does not appear, it might be because of a problem with the Shader.

* If you are painting vertex colors, make sure you use [a compatible Shader](mode_color.md).
* If you are painting Textures, make sure you apply [a compatible Shader](modes_texture.md) and [configure it correctly](modes_texture.md#config) in the Polybrush panel.

<a name="find-shaders"></a>

## Can't find example Shaders

Unity does not import examples by default when you install Polybrush. You can install them from inside the Package Manager. For more information, see [Importing Polybrush Shaders](index.md#import-shaders).

<a name="import-errors"></a>

## Errors when importing example Shaders

Each set of Shaders is made for a specific rendering pipeline. Make sure to import the one that is appropriate for your Project. You can tell which rendering pipeline it is for by its name.

<a name="resets"></a>

## Mesh automatically resets

The Mesh automatically resets to a source Mesh after few seconds when baking lightmaps (or if the Lightmap's [Auto Generate](https://docs.unity3d.com/Manual/GlobalIllumination.html) property is enabled)

This is a known issue between [Additional Vertex Stream](index.md#batch-avs) and lightmaps. Baking lightmaps erases the Additional Vertex Stream from the [MeshRenderer](https://docs.unity3d.com/Manual/class-MeshRenderer.html). However, data is not lost because it is stored in the PolybrushMesh component. To work around this, set the [PolybrushMesh](component.md) component's **Apply As** property to **Overwrite Mesh**.