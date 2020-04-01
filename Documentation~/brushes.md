# Brush Settings

These settings determine how a brush affects Meshes.

![Brush Settings](images/PolybrushPanel_BrushSettings_WithLetters.png)

 

![](images/icons/LetterA.png) Choose one of the saved brush presets from the brush preset drop-down menu. 

![](images/icons/LetterB.png) Select the **Save** button to save the current brush settings as a new Preset, or overwrite an existing Preset.

![](images/icons/LetterC.png) Set the maximum and minimum radius for the current brush with the **Brush Radius Min / Max** settings. Select the arrow icon (![](images/icons/FoldOut_Closed.png)) to toggle between expanding and collapsing these settings:

| **Property**          | **Description**                                              |
| --------------------- | ------------------------------------------------------------ |
| **Min**               | Set the minimum value the **Outer Radius** and **Inner Radius** sliders allow. |
| **Max**               | Set the maximum value the **Outer Radius** and **Inner Radius** sliders allow. |
| **Unclamped Falloff** | Enable this property to extend the brush falloff past the limit of the outer radius. |

![](images/icons/LetterD.png) Set the outer limit of the brush, where it's effect becomes 0%. <a name="outer"></a>The **Outer Radius** is the full radius of the brush, that the values in the **Brush Radius Min / Max** section limit. This appears as a light-colored outer ring. 

The shortcut for this setting is Ctrl + rotate wheel (Command + rotate wheel on macOS).

![](images/icons/LetterE.png) Set the zone of 100% effect for the brush. <a name="inner"></a>Everything inside the **Inner Radius** gets the full brush effect (for example, to create hard vs. soft brushes). This appears as a bright blue inner ring.

The shortcut for this setting is Shift + rotate wheel.

![](images/icons/LetterF.png) Set the **Strength** value to control the brush's maximum effect: <a name="strength"></a>

* When painting colors or textures, this setting controls Opacity. 
* When sculpting geometry, this setting corresponds to a percentage of the [Sculpt Power](modes_sculpt.md#power). 

The shortcut for this setting is Ctrl + Shift + rotate wheel (Command + Shift + rotate wheel on macOS).

![](images/icons/LetterG.png) Set the **Falloff Curve** to control exactly how the brush fades from full (**Inner Radius**) to zero (**Outer Radius**). <a name="falloff"></a>To modify the curve, select the curve image. The Unity **Curve** editor opens:

![The Curve editor](images/FalloffCurve.png) 

For information on how to use this window, see the documentation on [Editing Curves](https://docs.unity3d.com/Documentation/Manual/EditingCurves.html).