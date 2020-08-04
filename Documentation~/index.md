# About Polybrush

Polybrush is a Mesh painting, sculpting, and geo-scattering tool for **Unity 2018.3 and later**.

> **Note:** Polybrush is not compatible with Terrains. If you would like to work on Terrains, use the [dedicated Terrain tool](https://docs.unity3d.com/Manual/terrain-UsingTerrains.html) instead.

Polybrush provides five different [brush modes](modes.md) that define how a brush interacts with a Mesh. Some move vertices around, such as for [sculpting](modes_sculpt.md) (![](images/icons/Sculpt.png)) and [smoothing](modes_smooth.md) (![](images/icons/Smooth.png)) vertices. Others apply [colors](modes_color.md) (![](images/icons/Palette.png)) and [Textures](modes_texture.md) (![](images/icons/Bricks.png)) to vertices, and [scatter Prefabs](modes_prefab.md) (![](images/icons/FlowerAndGrass.png)) on the surface of your Mesh.



<a name="interface"></a>

## Working with Polybrush

The **Polybrush** window provides access to most of the Polybrush tools.  

To access the **Polybrush** window:

* From the Unity Editor menu, select **Tools** > **Polybrush** > **Polybrush Window**.

![The Polybrush Interface](images/PolybrushPanel_Off_WithLetters.png)

![](images/icons/LetterA.png) The **Mode toolbar** sets and displays the active [Brush Mode](modes.md). The [Polybrush window](index.md#interface) opens with no specific brush mode selected: only the [Brush Settings](brushes.md) and [Brush Mirroring](brush_mirror.md) sections appear until you click one of the buttons on the [Mode toolbar](modes.md).

![](images/icons/LetterB.png) **Brush Settings** contains properties you can use to customize the [Radius](brushes.md#radius), [Falloff](brushes.md#falloff), [Strength/Opacity](brushes.md#strength), and [Falloff Curve](brushes.md#falloff-curve) of your brush tool. You can also use the [Brush Preset selector](brushes.md#brush-preset-selector) to save and load brushes that you frequently use.

![](images/icons/LetterC.png) **Brush Mirroring** lets you choose which [Brush Mirroring](brush_mirror.md) method to use with the current brush.

![](images/icons/LetterD.png) **Mode Settings** only appear when you activate a [Brush Mode](modes.md). These provide additional settings specific to the brush mode that is currently active. For example, in this image, the **Color Paint Settings** section appears because the Color brush mode is active.

> **Tip:** To de-activate Polybrush, you can:
>
> * Click the active Brush Mode button.
> * Activate any Unity transform tool (Pan, Move, Rotate, Scale).
> * Select the Esc key on your keyboard.

Besides the Polybrush window, you can also use the [Polybrush Mesh component](component.md), the [Menu options](menu.md), and the general [Polybrush preferences](prefs.md) to interact with and customize Polybrush.



<a name="installing"></a>

## Installation

From version 1.0 and onwards, Polybrush is only available from the Package Manager.

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Manual/upm-ui-install.html).



<a name="import-shaders"></a>

### Importing Polybrush Shaders

Polybrush provides sample Shaders which you can import into your Project. These Shaders are compatible with vertex colors and Texture blending.

To import these sample Shaders:

1. Open the Package Manager (**Window** > **Package Manager**) inside the Unity Editor and select the Polybrush package:

	![Shader samples in the Polybrush package](images/ImportShaderExamples.png)

2. Click the **Import in project** button next to the Shader that matches the render pipeline you are using.

	The Project view now displays the sample Shaders under the *Assets/Samples/Polybrush* folder of your Project.



## Upgrading Polybrush

If you have been using a version of Polybrush prior to version 1.0, follow these instructions to upgrade:

   1. Close the Unity Editor.
   2. Find the */ProCore/Polybrush/* folder under *&lt;project_assets_folder&gt;/ProCore/Polybrush/*.
   3. Delete the folder.
   4. Open the Editor.
   5. Install the latest version of Polybrush from the Package Manager (see [Installing Polybrush](#installing))



<a name="batch-avs"></a>

### Converting Additional Vertex Streams

Starting with Polybrush v1.0, Polybrush manages all data with the **PolybrushMesh** component, instead of managing data through the Additional Vertex Streams feature, as in previous versions of Polybrush.

The first time you hover over an object with one of Polybrush's tools enabled, Polybrush automatically replaces the deprecated **Z_AdditionalVertexStreams** components with new **PolybrushMesh** components.

You can also run the **Update Z_AdditionalVertexStreams** batch tool, which searches every Scene currently loaded in the Editor and switches the old component for the new one. It then automatically converts the internal data to avoid data loss.

To run the batch conversion:

1. From the Unity Editor menu, select **Tools** &gt; **Polybrush** &gt; **Upgrade Z_AdditionalVertexStreams**.
2. Save your Scenes and Projects.



<a name="shader-meta"></a>

### Updating Shader configurations

As of Polybrush v1.0, the [Texture Blending tool](modes_texture) connects Shader channels with Textures directly. Polybrush stores this setup information in the Shader's .meta file instead of .pbs.json files, like it did prior to v1.0.

To convert existing .pbs.json files to the new format:

1. Select your Shaders in the Project window.
2. From the Unity Editor menu, select **Tools** &gt; **Polybrush** &gt; **Update Shader Meta**.

The conversion process moves the data from the .pbs.json files into the Shader's .meta file and deletes the old .pbs.json file.

