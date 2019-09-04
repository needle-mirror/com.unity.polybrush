# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

### Known Issues
- Lightmapping is currently not compatible with use of "Additional Vertex Streams". Workaround is to disable "Use Additional Vertex Streams" in preferences.

## [1.0.1] - 2019-09-05
### Bug fixes
- Fixed errors when entering playmode.
- Fixed jagged geometry when smoothing with `Brush normal`.
- Fixed broken normals in some situations.
- Fixed compilation errors.
- Fixed minor UI issues.

## [1.0.1-preview.3] - 2019-08-21
### Bug fixes
- Fixed errors when entering playmode.

## [1.0.1-preview.2] - 2019-08-01
### Bug fixes
- Fixed jagged geometry when smoothing with `Brush normal`.
- Fixed broken normals in some situations.

## [1.0.1-preview.1] - 2019-07-26
### Bug fixes
- Fixed compilation errors.
- Fixed minor UI issues.

## [1.0.0] - 2019-07-08
### Changes
- Added support for Unity 2018.4.

- Using Polybrush on an object will always add the PolybrushMesh component.

- PolybrushMesh component now allows you choose how mesh edits are applied (Override Mesh or Additional Vertex Streams).

- Texture Blend Mode: shader setup is now done by clicking the "Configure" button in Texture Blend Mode, and saved into the shader meta file.

- Removed obsolete example files, replaced with basic "Vertex Colored" and "Texture Blend" example shaders, built specifically for Non-SRP, LWRP, and HDRP.

### Bug fixes
- Fixed crash occuring when users convert from Additional Vertex Streams to Baked mode (and vice-versa).
- Fixed crash occurring when users undo changes and not using Additional Vertex Streams.
- Fixed loss of data when not using Additional Vertex Streams and Prefab mode.
- Fixed Prefab mode not showing data from Polybrush when it opens.
- Fixed erase action (hold `CTRL`) not removing data for only the selected texture in Texture Blend mode.
- Fixed brush not showing the visual preview when hovering over meshes until first stroke.
- Fixed brush not showing the proper visual preview for data removal when holding `CTRL`.
- Fixed brush direction gizmo not matching the movement vector.
- Fixed exception appearing sometimes while working with mirrored brushes.
- Fixed `Reset` action on ColorPalette not properly resetting the data.
- Fixed console warnings from Overlay and VertexBillboard shaders.
- Fixed minor UI issues.

## [1.0.0-preview.17] - 2019-05-13

### Changes
- Set "Use Additional Vertex Streams" default value to false.

## [1.0.0-preview.16] - 2019-04-30

### Changes
- Add support for Unity 2018.3.

### Bug Fixes
- Fix an issue occurring when there is a vertex count mismatch between Additional Vertex Streams and the mesh referenced in the MeshFilter component.

### Changes
- Polybrush will automatically upgrade Z_AdditionalVertexStreams to PolybrushMesh components.
- Add new section in documentations: Upgrading Polybrush.

## [1.0.0-preview.14] - 2019-04-04

### Changes
- Polybrush will automatically upgrade Z_AdditionalVertexStreams to PolybrushMesh components.
- Add new section in documentations: Upgrading Polybrush.

## [1.0.0-preview.13] - 2019-03-20

### Changes
- Update samples.

## [1.0.0-preview.12] - 2019-03-20

### Changes
- Update dependencies.

## [1.0.0-preview.11] - 2019-03-13

### Bug Fixes
- Minor fixes

## [1.0.0-preview.10] - 2019-03-12

### Bug Fixes
- Minor fixes

### Changes
- Make every modes use Ctrl key to invert actions.

## [1.0.0-preview.9] - 2019-03-08

### Bug Fixes
- Fix compatibility issues with SkinnedMeshRenderer component.
- Fix compatibility issues with ProBuilder 4.
- Scattering tool: fix exceptions thrown when removing objects from palettes.
- Scattering tool: fix issues with prefabs having more than one mesh.

## [1.0.0-preview.8] - 2019-02-26

### Bug Fixes
- Fix brush inner and outer rings not always reflecting user's Preferences.

## [1.0.0-preview.7] - 2019-02-08

### Bug Fixes
- Scattering tool: prefab preview compatibility with PolybrushMesh.
- Scattering tool: fix distance between objects being to great when Avoid Overlap is enabled.

## [1.0.0-preview.6] - 2019-02-04

### Bug Fixes
- Fix Polybrush not able to paint on mesh with multiple submeshes.
- Fix scattering tool not removing prefabs based on the active loadout.
- Fix "Save To Asset" button in MeshFilter component not doing anything.
- Small UI fixes.

## [1.0.0-preview.5] - 2019-01-28

### Bug Fixes
- Fix scattering tool not removing prefabs in some cases.
- Fix "Fill" mode with the Texture Blend tool.
- Fix "Flood" mode with Vertex Color tool.
- Small UI fixes.

## [1.0.0-preview.4] - 2019-01-16

### Changes
- Update Polybrush for Unity 2019.1 compatibility.
- General settings are now in Unity Preferences window.
- Update Polybrush skin and improve Polybrush window reactivity.
- Samples are now available through the Package Manager in their own section (Unity 2019.1+).

### Bug Fixes
- Fix PolybrushMesh component staying on GameObject even when we don't apply changes.
- Fix Undo actions not totally reverting back GameObjects state.
- Fix broken shader graphs for LWRP and HDRP.
- Fix Polybrush losing control when mouse cursor goes out of scene view while applying a brush.

## [1.0.0-preview.3] - 2018-10-12

### Features

- Port Polybrush from Asset Store to Package Manager.
- New prefab scattering brush mode settings.
- Update Polybrush for Unity 2018.1, 2018.2, and 2018.3 compatibility.
- New example shaders for HDRP and LWRP.

### Bug Fixes

- Update ShaderForge examples to Unity 2018.1 and up.
- Fix Polybrush modified meshes losing values when converted to a prefab.
- Fix `Undo` not updating `Additional Vertex Stream` mesh modifications.
- Fix the detection of texture blend support when switching materials on an object.

### Changes

- `Escape` key now exits the brush editing tool.
- Replace `Save As` with `Save As New brush` in the save menu of Brush Settings.
- Remove legacy code for custom made shaders.
- `AdditionalVertexStream` modified meshes are now stored in a component, fixing issues with lost data.
- Dramatically improve unit testing coverage.
- `z_` prefix removed from class names.

## [0.9.13-preview.0] - 2017-08-04

### Bug Fixes

- Fix compile error due to `z_ZoomOverride.cs` incorrectly being included in builds.

### Changes

- New example textures.

## [0.9.12-preview.0] - 2017-06-19

### Bug Fixes

- Fix compile errors when building to Standalone.
- Fix issue where subdividing in ProBuilder could cause Polybrush to endlessly throw errors.

## [0.9.11-preview.0] - 2017-04-25

### Features

- Add Flood paint brush in Vertex Color and Texture painting modes.
- Add utility for baking additional vertex streams back to mesh.
- Set better defaults for Push/Pull and Prefab modes.

## Bug Fixes

- sRGB should be marked false on icons in 5.5 and up.
- Fix deprecated handle warnings in Unity 5.6.
- Updated header backing color to match GUI Color Scheme.
- Fix bug where additional vertex streams would modify static meshes at runtime, causing them to either disappear or be moved.

## [0.9.10-preview.0] - 2016-12-09

### Features

- Prefab Brush Mode added (still very early in development).
- Significant performance improvements when working with high vertex count meshes.
- Add a height based blend shader.
- Add texture blend modulate shader.
- Add option to hide vertex dots.

## Bug Fixes

- Fix bug where Polybrush would always instance a new mesh when "Meshes Are Assets" was enabled in ProBuilder.
- Increase text contrast in Polybrush skins.
- Try to crash less when mesh has no normals.
- Fix bug where loading a new scene would prevent brush from updating.
- Fix mismatched text color in command toolbar.
- Fix bug where hovering multiple selected objects would repeatedly instance new brush targets.
- Fix additional space in Color field labels when content is null or empty.
- Fix bug where hovering multiple meshes would sometimes apply the previous mesh vertex positions to the current.
- Fix brush settings anchor setting clipping the top header.
- Fix scene not repainting when applying brush in Unity 5.5.

### Changes

- Store some brush settings per-mode.
- Preferences now stored local to project instead of in Unity Editor preferences.
- Use hinted smooth glyph rasterization instead of OS default for Roboto font.

## [0.9.9-preview.2] - 2016-11-14

### Features

- Redesigned interface.
- Add support for "Additional Vertex Streams" workflow.
- Unity 5.5 beta compatibility.
- Add ability to save brush setting modifications to preset.
- Enable multiple axes of mirroring.
- Show color palette as a set of swatches instead of a reorderable list.
- Improve performance when painting larger meshes.
- Handle wireframe and outline disabling/re-enabling correclty in Unity 5.5.
- Clean up various 5.5 incompatibilities.
- Add a question mark icon to header labels with link to documentation page.
- Improve performance by caching mesh values instead of polling UnityEngine.Mesh.
- Improve memory pooling in some performance-critical functions.
- Manual redesigned to better match ProBuilder.
- Mode toolbar now toggles Polybrush on/off when clicking active mode.

## Bug Fixes

- Fix cases where pb_Object optimize could be skipped on undo.
- Fix import settings for icons.
- Don't leak brush editor objects if the brush target has changed.
- Fix serialization warnings on opening editor in Unity 5.4.
- Mark z_Editor brushSettings as HideAndDontSave so that loading new scenes doesn't discard the instanced brush settings, resulting in NullReference errors in the Brush Editor.
- Destroy BrushEditor when z_Editor is done with it so that unity doesn't try to serialize and deserialize the brush editor, resulting in null reference errors.
- Fix texture brush inspector always showing a scroll bar on retina display.
- Fix bug where the texture brush would show a black swatch on the mesh after a script reload.
- Remove shift-q shortcut since it's inconsistent and when it does work interferes with capital Qs.
- Fix bug where OnBrushExit sometimes wouldn't refresh the mesh textures.
- Layer texture blend shaders instead of summing.

### Changes

- "Raise" mode is now "Push Pull."
- Remove vertex billboards from mesh sculpting overlay in favor of just the wireframe.

## [0.9.8-preview.0] - 2016-08-17

### Features

- Add option to make brush normal direction sticky to first application.

## Bug Fixes

- Fix Polybrush errors when working with pb 2.6
- Don't specify `isFallback` in CustomEditor implementations since they are not actually fallback editors.
- Don't rely on loaded assembly names matching the expected ProBuilderEditor assembly name when reflecting types and methods.  Fixes null reference when assembly names are changed somewhere in the pipeline.
- Don't bother testing that shader references match when looking for shader metadata since instance ids are so fickle in the first place

## [0.9.7-preview.0] - 2016-06-01

### Features

- Significantly improve perfomance in all modes for high vertex count meshes.
- Add "Texture Blend with Vertex Color" shader.
- Add two new default meshes, a smooth and hard icosphere.
- **Texture Mode** backend rewritten entirely to allow for far more complex interactions with shader properties.  See "Writing Texture Shaders" in documentation.
- Add color mask settings to vertex color painter.

## Bug Fixes

- Improve performance of shared edge triangle lookups, fixing lengthy lags when mousing over high vertex count meshes in paint and texture modes.
- When blending multiple brushes in texture mode use the max weight instead of summing.
- Add information about setting shader paths to Hidden in Shaderforge instructions.
- In vertex sculpting modes iterate per-common index instead of per-vertex, improving brush application performance and minimizing chances of splitting a common vertex by accident.
- Minor cleanup of enum types (make sure they're namespaced & remove unused includes).
- Fix bug where script reloads would null-ify texture mode brush color and not properly reset it.
- Fix errors in brush event logic that caused OnBrushEnter/Move/Exit to be called at incorrect times with invalid targets, resulting in crashes when editing prefabs with multiple meshes with different shaders
- Fix undo throwing errors when splat_cache is null.
- Clamp radius max value to min+.001 to avoid crashes when scrolling radius shortcut with equal min/max values.
- Fix uv3/4 not applying or applying to incorrect mesh channels in ProBuilder (requires update to ProBuilder 2.5.1).
- Fix null errors when sculpting pb_Object meshes caused by FinalizeAndResetHovering iterating the same object multiple times.

### Changes

- Set default brush settings radius max to 5.
- When blending multiple brush effects use lighten blending mode instead of additive.
- Shorten ShaderForge source file suffix to `_SfSr` (but keep compatibility for older `_SfTexBlendSrc`).

## [0.9.6-preview.0] - 2016-04-28

## Bug Fixes

- Fix lag when selecting or hovering an object with large vertex count.
- Increase the min threshold for vertex weight to be considered for movement in raise/lower mode.
- Fix out of bounds errors in overlay renderer when mesh vertex count is greater than ushort.max / 4.
- Fix typo that caused pb_Objects not to update vertex caches on z_EditableObject Apply calls.

## [0.9.5-preview.0] - 2016-04-20

## Bug Fixes

- 114ccc2 Show checkbox in z_Editor context menu for current floating state.
- f44d4d6 When applying mesh values to `pb_Object` also include vec4 uv3/4 attributes.
- c3d3f7a Fix ambiguous method error in z_EditableObject when modifying a ProBuilder mesh.

## [0.9.4-preview.0] - 2016-02-28

### Features

- Add new "Diffuse Vertex Color" and "TriPlanar Blend Legacy" shaders.

## Bug Fixes

- Fix Unity crash when selection contains non-mesh objects.

## [0.9.3-preview.0] - 2016-02-22

### Features
- Add option to ignore unselected GameObjects when a brush tool is in use.
- Improve detection of meshes in selected children.
- Add button to clear Polybrush preferences in Settings.

## Bug Fixes
- Fix mirrored brush applying only one brush in texture and vertex paint modes.

## [0.9.2-preview.0] - 2016-02-18

### Features

- Instead of a single readme, use a static site generator to build documentation.

## Bug Fixes

- Fix triplanar blend shader stretching on some poles.  Add more detailed information on using vertex color and texture blend shader materials
- When checking mouse picks for selection include children of selected gameObjects as valid targets (fixes issue where a selected model root would not register for brush.
- Fix null reference errors when brush mirroring is enabled.
- Don't throw null ref when weights cache in overlay doesn't match new set.
- Add docs for brush settings, interface, and general settings.
- Add misc and troubleshooting section to docs.
- Add warnings that Polybrush does not work on Unity terrain objects

### Changes

- Migrate project to Github.  For access to the latest development builds of Polybrush please email contact@procore3d.com with your invoice number and request Git access (this will be automated in the future).

## [0.9.1-preview.1]

## Bug Fixes

- Register children of current selection as valid mesh editables.  Fixes potential confusion when a model prefab is selected at it's root with children.

## [0.9.1-preview.0]

### Features

- Texture Blend Mode now supports any combination `UV0, UV2, UV3, UV4, Color, Tangent` mesh attributes, as set by the shader using the syntax `define Z_MESH_ATTRIBUTES UV0 ...`.
- Improve the `Unlit Texture Blend` example shader.
- Add option to automatically rebuild collision meshes.
- Split Direction *Normal* into *Brush Normal* and *Vertex Normal*.
- Improve the behavior of Smooth Mode.
- Remove relax option from Smooth Mode (what was once Normal w/ Relax on or off becomes Vertex Normal and Brush Normal, respectively).
- Add option to keep the brush focused on the first mesh hit when dragging.
- When dragging, always restrict brush application to the current selection.

## Bug Fixes

- Fix issue where Texture Paint Mode would sometimes apply values to the incorrect channel after switching between two different materials with the same shader.
- Make sure that the Texture Paint mode always has valid splat-weights to work with (usually causing "black face errors").
- Fix warnings when shaderforge isn't installed, and update version number.
- Fix instances where setting ProBuilder edit level would not correctly unset Polybrush tool (and vice-versa).

### Changes

- Standard Vertex Color and Standard Texture Blend shaders now default to metallic / roughness workflow.

## [0.9.0-preview.0]

Initial Release.
