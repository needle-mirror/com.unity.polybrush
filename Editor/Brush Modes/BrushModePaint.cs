using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Vertex painter brush mode.
    /// </summary>
    internal class BrushModePaint : BrushModeMesh
	{
        [System.Serializable]
        struct VertexColorsPaintInfo
        {
            /// <summary>
            /// Copy of colors array loaded from active mesh.
            /// </summary>
            public Color[] OriginalColors;

            /// <summary>
            /// Colors array used when applying brush.
            /// </summary>
            public Color[] TargetColors;

            /// <summary>
            /// Colors array used when erasing color.
            /// Only used in "brush" mode. "Flood" and "Fill" modes will use white color.
            /// </summary>
            public Color[] EraseColors;

            /// <summary>
            /// Current active colors applied on active mesh.
            /// </summary>
            public Color[] Colors;

            /// <summary>
            /// Buffer of colors array loaded from active mesh.
            /// </summary>
            public ComputeBuffer OriginalColorsBuffer;

            /// <summary>
            /// Buffer array used when applying brush.
            /// </summary>
            public ComputeBuffer TargetColorsBuffer;

            /// <summary>
            /// Buffer array used when erasing color.
            /// Only used in "brush" mode. "Flood" and "Fill" modes will use white color.
            /// </summary>
            public ComputeBuffer EraseColorsBuffer;

            /// <summary>
            /// Buffer of current active colors applied on active mesh.
            /// </summary>
            public ComputeBuffer ColorsBuffer;

            /// <summary>
            /// Buffer of current weights applied on active mesh.
            /// </summary>
            public ComputeBuffer WeightsBuffer;

            /// <summary>
            /// Refresh this instance with new informations based on a given mesh vertex colors.
            /// </summary>
            /// <param name="baseColors">Vertex colors array from a given mesh. It'll be used to initialize every fields of this struct.</param>
            public void Build(Color[] baseColors)
            {
                var colorLength = baseColors.Length;
                OriginalColors = baseColors;
                Colors = new Color[colorLength];
                TargetColors = new Color[colorLength];
                EraseColors = new Color[colorLength];

                if(SystemInfo.supportsComputeShaders)
                {
                    DisposeBuffers();

                    OriginalColorsBuffer = new ComputeBuffer(colorLength, 4 * sizeof(float));
                    EraseColorsBuffer = new ComputeBuffer(colorLength, 4 * sizeof(float));
                    TargetColorsBuffer = new ComputeBuffer(colorLength, 4 * sizeof(float));
                    ColorsBuffer = new ComputeBuffer(colorLength, 4 * sizeof(float));
                    WeightsBuffer = new ComputeBuffer(colorLength, sizeof(float));

                    OriginalColorsBuffer.SetData(OriginalColors);
                }
            }

            /// <summary>
            /// Refresh brush fields TargetColors and EraseColors based on a given color.
            /// </summary>
            /// <param name="color">New colors to apply in TargetColors and EraseColors fields.</param>
            /// <param name="strength">Brush strength to apply on <paramref name="color"/>.</param>
            /// <param name="mask">Selected channels on which we will apply <paramref name="color"/>.</param>
            public void RebuildColorTargets(Color color, float strength, ColorMask mask)
            {
                if (OriginalColors == null || TargetColors == null || OriginalColors.Length != TargetColors.Length)
                    return;

                for (int i = 0; i < OriginalColors.Length; i++)
                {
                    TargetColors[i] = Util.Lerp(OriginalColors[i], color, mask, strength);
                    EraseColors[i] = Util.Lerp(OriginalColors[i], s_WhiteColor, mask, strength);
                }


                if(SystemInfo.supportsComputeShaders)
                {
                    TargetColorsBuffer.SetData(TargetColors);
                    EraseColorsBuffer.SetData(EraseColors);
                }
            }

            public void ApplyColors()
            {
                System.Array.Copy(Colors, OriginalColors, Colors.Length);

                if(SystemInfo.supportsComputeShaders)
                    OriginalColorsBuffer.SetData(OriginalColors);
            }

            public void DisposeBuffers()
            {
                if(OriginalColorsBuffer != null)
                    OriginalColorsBuffer.Dispose();
                if(EraseColorsBuffer != null)
                    EraseColorsBuffer.Dispose();
                if(TargetColorsBuffer != null)
                    TargetColorsBuffer.Dispose();
                if(ColorsBuffer != null)
                    ColorsBuffer.Dispose();
                if(WeightsBuffer != null)
                    WeightsBuffer.Dispose();
            }
        }

        class EditableObjectData
        {
            public VertexColorsPaintInfo MeshVertexColors;
            public bool LikelySupportsVertexColors;

            // used for fill mode
            public Dictionary<PolyEdge, List<int>> TriangleLookup;
        }

		// how many applications it should take to reach the full strength
		const float k_StrengthModifier = 1f/8f;
		static readonly Color s_WhiteColor = new Color32(255, 255, 255, 255);

		[SerializeField]
        internal PaintMode paintMode = PaintMode.Brush;

        Dictionary<EditableObject, EditableObjectData> m_EditableObjectsData = new Dictionary<EditableObject, EditableObjectData>();

        [SerializeField]
        Color32 m_BrushColor = Color.green;

        // The current color palette.
        [SerializeField]
        ColorPalette m_ColorPalette = null;

        internal ColorMask mask = new ColorMask(true, true, true, true);

		ColorPalette[] m_AvailablePalettes = null;
		string[] m_AvailablePalettesAsString = null;
		int m_CurrentPaletteIndex = -1;

        //Compute Shader variables
        const string k_ShaderPath = "/Content/ComputeShader/ColorLerpCS.compute";
        ComputeShader m_ColorLerpShader;
        ComputeShader colorLerpShader
        {
            get
            {
                if(m_ColorLerpShader == null)
                    m_ColorLerpShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(PolyEditorUtility.RootFolder + k_ShaderPath);

                if(m_ColorLerpShader == null)
                    Debug.LogWarning("Compute Shader not found at "+PolyEditorUtility.RootFolder + k_ShaderPath);

                return m_ColorLerpShader;
            }
        }

		// temp vars
		PolyEdge[] m_FillModeEdges = new PolyEdge[3];
		List<int> m_FillModeAdjacentTriangles = null;

		internal GUIContent[] modeIcons = new GUIContent[]
		{
			new GUIContent("Brush", "Brush" ),
			new GUIContent("Fill", "Fill" ),
			new GUIContent("Flood", "Flood" )
		};

		internal ColorPalette colorPalette
		{
			get
			{
				if(m_ColorPalette == null)
					colorPalette = PolyEditorUtility.GetFirstOrNew<ColorPalette>();
				return m_ColorPalette;
			}
			set
			{
				m_ColorPalette = value;
			}
		}

        internal override bool SetDefaultSettings()
        {
            RefreshAvailablePalettes();
            ColorPalette defaultPalette = m_AvailablePalettes.FirstOrDefault(x => x.name.Contains("Default"));
            if (defaultPalette == null)
            {
                return false;
            }

            SetColorPalette(defaultPalette);

            //other settings
            paintMode = PaintMode.Brush;
            SetBrushColor(Color.red, 1f);
            mask = new ColorMask(true, true, true, true);

            return true;
        }

        // An Editor for the colorPalette.
        ColorPaletteEditor m_ColorPaletteEditor = null;

		ColorPaletteEditor colorPaletteEditor
		{
			get
			{
				if(m_ColorPaletteEditor == null || m_ColorPaletteEditor.target != colorPalette)
				{
					m_ColorPaletteEditor = (ColorPaletteEditor) Editor.CreateEditor(colorPalette);
					m_ColorPaletteEditor.hideFlags = HideFlags.HideAndDontSave;
				}

				return m_ColorPaletteEditor;
			}
		}

        /// <summary>
        /// The message that will accompany Undo commands for this brush.  Undo/Redo is handled by PolyEditor.
        /// </summary>
        internal override string UndoMessage { get { return "Paint Brush"; } }
		protected override string ModeSettingsHeader { get { return "Color Paint Settings"; } }
		protected override string DocsLink { get { return PrefUtility.documentationColorBrushLink; } }

		internal override void OnEnable()
		{
			base.OnEnable();

			RefreshAvailablePalettes();
            m_BrushColor = colorPalette.current;
		}

		internal override void OnDisable()
		{
			base.OnDisable();
			if(m_ColorPaletteEditor != null)
				Object.DestroyImmediate(m_ColorPaletteEditor);
		}

        /// <summary>
        /// Inspector GUI shown in the Editor window.  Base class shows BrushSettings by default
        /// </summary>
        /// <param name="brushSettings">Current brush settings</param>
        internal override void DrawGUI(BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);

            using (new GUILayout.HorizontalScope())
            {
                if (colorPalette == null)
                    RefreshAvailablePalettes();

                EditorGUI.BeginChangeCheck();
                m_CurrentPaletteIndex = EditorGUILayout.Popup(m_CurrentPaletteIndex, m_AvailablePalettesAsString);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_CurrentPaletteIndex >= m_AvailablePalettes.Length)
                        SetColorPalette(ColorPaletteEditor.AddNew());
                    else
                        SetColorPalette(m_AvailablePalettes[m_CurrentPaletteIndex]);
                }

                paintMode = (PaintMode)GUILayout.Toolbar((int)paintMode, modeIcons);
            }

            bool likelySupportsVertexColors = m_EditableObjectsData.Count == 0;
            foreach(var kvp in m_EditableObjectsData)
                likelySupportsVertexColors |= kvp.Value.LikelySupportsVertexColors;

            if(!likelySupportsVertexColors)
				EditorGUILayout.HelpBox("It doesn't look like any of the materials on this object support vertex colors!", MessageType.Warning);

			colorPaletteEditor.onSelectIndex = (color) => { SetBrushColor(color, brushSettings.strength); };
			colorPaletteEditor.onSaveAs = SetColorPalette;

			mask = PolyGUILayout.ColorMaskField("Color Mask", mask);

			colorPaletteEditor.OnInspectorGUI();
		}

		internal void SetBrushColor(Color color, float strength)
		{
			m_BrushColor = color;
            foreach(var kvp in m_EditableObjectsData)
                kvp.Value.MeshVertexColors.RebuildColorTargets(color, strength, mask);
		}

		internal void SetColorPalette(ColorPalette palette)
		{
			colorPalette = palette;
            m_BrushColor = colorPalette.current;

            RefreshAvailablePalettes();
        }

		internal override void OnBrushSettingsChanged(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushSettingsChanged(target, settings);

            foreach(var kvp in m_EditableObjectsData)
                kvp.Value.MeshVertexColors.RebuildColorTargets(m_BrushColor, settings.strength, mask);
		}

		// Called when the mouse begins hovering an editable object.
        internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
        {
            base.OnBrushEnter(target, settings);

            if(target.graphicsMesh == null)
                return;

            EditableObjectData data;
            if(!m_EditableObjectsData.TryGetValue(target, out data))
            {
                data = new EditableObjectData();
                m_EditableObjectsData.Add(target, data);
            }

            RebuildCaches(target, settings);

            data.TriangleLookup = PolyMeshUtility.GetAdjacentTriangles(target.editMesh);

			MeshRenderer mr = target.gameObjectAttached.GetComponent<MeshRenderer>();

            if(mr != null && mr.sharedMaterials != null)
                data.LikelySupportsVertexColors = mr.sharedMaterials.Any(x =>
                    x != null && x.shader != null && PolyShaderUtil.SupportsVertexColors(x.shader));
            else
                data.LikelySupportsVertexColors = false;
        }

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		internal override void OnBrushMove(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushMove(target, settings);

			if(!Util.IsValid(target) || !m_EditableObjectsData.ContainsKey(target.editableObject))
				return;

			bool invert = settings.isUserHoldingControl;

			PolyMesh mesh = target.editableObject.editMesh;
			int vertexCount = mesh.vertexCount;
			float[] weights = target.GetAllWeights();

            var data = m_EditableObjectsData[target.editableObject];
            var vertexColorInfo = data.MeshVertexColors;

			switch(paintMode)
			{
				case PaintMode.Flood:
					for(int i = 0; i < vertexCount; i++)
                        vertexColorInfo.Colors[i] = invert? s_WhiteColor : vertexColorInfo.TargetColors[i];
					break;

				case PaintMode.Fill:
                    System.Array.Copy(vertexColorInfo.OriginalColors, vertexColorInfo.Colors, vertexCount);
					int[] indices = target.editableObject.editMesh.GetTriangles();
					int index = 0;

                    foreach(PolyRaycastHit hit in target.raycastHits)
					{
						if(hit.triangle > -1)
						{
							index = hit.triangle * 3;

                            vertexColorInfo.Colors[indices[index + 0]] = invert ? s_WhiteColor : vertexColorInfo.TargetColors[indices[index + 0]];
                            vertexColorInfo.Colors[indices[index + 1]] = invert ? s_WhiteColor : vertexColorInfo.TargetColors[indices[index + 1]];
                            vertexColorInfo.Colors[indices[index + 2]] = invert ? s_WhiteColor : vertexColorInfo.TargetColors[indices[index + 2]];

							m_FillModeEdges[0].x = indices[index+0];
							m_FillModeEdges[0].y = indices[index+1];

							m_FillModeEdges[1].x = indices[index+1];
							m_FillModeEdges[1].y = indices[index+2];

							m_FillModeEdges[2].x = indices[index+2];
							m_FillModeEdges[2].y = indices[index+0];

							for(int i = 0; i < 3; i++)
							{
								if(data.TriangleLookup.TryGetValue(m_FillModeEdges[i], out m_FillModeAdjacentTriangles))
								{
									for(int n = 0; n < m_FillModeAdjacentTriangles.Count; n++)
									{
										index = m_FillModeAdjacentTriangles[n] * 3;

                                        vertexColorInfo.Colors[indices[index + 0]] = invert ? s_WhiteColor : vertexColorInfo.TargetColors[indices[index + 0]];
                                        vertexColorInfo.Colors[indices[index + 1]] = invert ? s_WhiteColor : vertexColorInfo.TargetColors[indices[index + 1]];
                                        vertexColorInfo.Colors[indices[index + 2]] = invert ? s_WhiteColor : vertexColorInfo.TargetColors[indices[index + 2]];
									}
								}
							}
						}
					}

					break;

                default:
                {
                    if(SystemInfo.supportsComputeShaders && colorLerpShader != null)
                    {
                        int kernelIndex = colorLerpShader.FindKernel("ColorLerpKernel");

                        ComputeBuffer originalColorsBuffer = data.MeshVertexColors.OriginalColorsBuffer;
                        ComputeBuffer lerpColorsBuffer = data.MeshVertexColors.TargetColorsBuffer;
                        if(invert)
                            lerpColorsBuffer = data.MeshVertexColors.EraseColorsBuffer;

                        ComputeBuffer weightsBuffer = data.MeshVertexColors.WeightsBuffer;
                        weightsBuffer.SetData(weights);

                        ComputeBuffer resultColorsBuffer = data.MeshVertexColors.ColorsBuffer;
                        resultColorsBuffer.SetData(vertexColorInfo.Colors);

                        colorLerpShader.SetBuffer(kernelIndex, "originalColorBuffer", originalColorsBuffer);
                        colorLerpShader.SetBuffer(kernelIndex, "lerpColorBuffer", lerpColorsBuffer);
                        colorLerpShader.SetBuffer(kernelIndex, "weightBuffer", weightsBuffer);
                        colorLerpShader.SetBuffer(kernelIndex, "resultColors", resultColorsBuffer);

                        Vector4 maskVect = new Vector4(mask.r ? 1 : 0,mask.g ? 1 : 0, mask.b ? 1 : 0, mask.a ? 1 : 0);
                        colorLerpShader.SetVector("mask",maskVect);

                        uint threadGroupSize;
                        colorLerpShader.GetKernelThreadGroupSizes(kernelIndex, out threadGroupSize, out _, out _);
                        colorLerpShader.Dispatch(kernelIndex, Mathf.CeilToInt(( resultColorsBuffer.count / threadGroupSize ) + 1), 1, 1);

                        resultColorsBuffer.GetData(vertexColorInfo.Colors);
                    }
                    else
                    {
                        for(int i = 0; i < vertexCount; i++)
                        {
                            vertexColorInfo.Colors[i] = Util.Lerp(vertexColorInfo.OriginalColors[i],
                                invert ? vertexColorInfo.EraseColors[i] : vertexColorInfo.TargetColors[i],
                                mask,
                                weights[i]);
                        }
                    }

                    break;
                }
			}

			target.editableObject.editMesh.colors = vertexColorInfo.Colors;
			target.editableObject.ApplyMeshAttributes(MeshChannel.Color);
		}

        // Called when the mouse exits hovering an editable object.
		internal override void OnBrushExit(EditableObject target)
		{
			base.OnBrushExit(target);

            var data = m_EditableObjectsData[target];

            if(target.editMesh != null)
            {
                target.editMesh.colors = data.MeshVertexColors.OriginalColors;
                target.ApplyMeshAttributes(MeshChannel.Color);
            }

            data.LikelySupportsVertexColors = true;

            if(SystemInfo.supportsComputeShaders)
                data.MeshVertexColors.DisposeBuffers();

            m_EditableObjectsData.Remove(target);
        }

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
        {
            var vertexColorInfo = m_EditableObjectsData[target.editableObject].MeshVertexColors;
            vertexColorInfo.ApplyColors();
			target.editableObject.editMesh.colors = vertexColorInfo.OriginalColors;
            target.editableObject.modifiedChannels |= MeshChannel.Color;

            base.OnBrushApply(target, settings);
		}

        /// <summary>
        /// set mesh colors back to their original state before registering for undo
        /// </summary>
        /// <param name="brushTarget">Target object of the brush</param>
        internal override void RegisterUndo(BrushTarget brushTarget)
		{
			brushTarget.editableObject.editMesh.colors = m_EditableObjectsData[brushTarget.editableObject].MeshVertexColors.OriginalColors;
			brushTarget.editableObject.ApplyMeshAttributes(MeshChannel.Color);

			base.RegisterUndo(brushTarget);
		}

		internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
		{
			if(Util.IsValid(target) && paintMode == PaintMode.Fill)
			{
				Vector3[] vertices = target.editableObject.editMesh.vertices;
				int[] indices = target.editableObject.editMesh.GetTriangles();

				int index = 0;

                using(new Handles.DrawingScope(target.transform.localToWorldMatrix))
                {
                    var data = m_EditableObjectsData[target.editableObject];
                    foreach (PolyRaycastHit hit in target.raycastHits)
                    {
                        if (hit.triangle > -1)
                        {
                            Handles.color = data.MeshVertexColors.TargetColors[indices[index]];

                            index = hit.triangle * 3;

                            Handles.DrawLine(vertices[indices[index + 0]] + hit.normal * .1f, vertices[indices[index + 1]] + hit.normal * .1f);
                            Handles.DrawLine(vertices[indices[index + 1]] + hit.normal * .1f, vertices[indices[index + 2]] + hit.normal * .1f);
                            Handles.DrawLine(vertices[indices[index + 2]] + hit.normal * .1f, vertices[indices[index + 0]] + hit.normal * .1f);

                            m_FillModeEdges[0].x = indices[index + 0];
                            m_FillModeEdges[0].y = indices[index + 1];

                            m_FillModeEdges[1].x = indices[index + 1];
                            m_FillModeEdges[1].y = indices[index + 2];

                            m_FillModeEdges[2].x = indices[index + 2];
                            m_FillModeEdges[2].y = indices[index + 0];

                            for (int i = 0; i < 3; i++)
                            {
                                if (data.TriangleLookup.TryGetValue(m_FillModeEdges[i], out m_FillModeAdjacentTriangles))
                                {
                                    for (int n = 0; n < m_FillModeAdjacentTriangles.Count; n++)
                                    {
                                        index = m_FillModeAdjacentTriangles[n] * 3;

                                        Handles.DrawLine(vertices[indices[index + 0]] + hit.normal * .1f, vertices[indices[index + 1]] + hit.normal * .1f);
                                        Handles.DrawLine(vertices[indices[index + 1]] + hit.normal * .1f, vertices[indices[index + 2]] + hit.normal * .1f);
                                        Handles.DrawLine(vertices[indices[index + 2]] + hit.normal * .1f, vertices[indices[index + 0]] + hit.normal * .1f);
                                    }
                                }
                            }
                        }
                    }
                }
			}
			else
			{
				base.DrawGizmos(target, settings);
			}
		}

        void RefreshAvailablePalettes()
        {
            m_AvailablePalettes = PolyEditorUtility.GetAll<ColorPalette>().ToArray();

            if (m_AvailablePalettes.Length < 1)
                colorPalette = PolyEditorUtility.GetFirstOrNew<ColorPalette>();

            m_AvailablePalettesAsString = m_AvailablePalettes.Select(x => x.name).ToArray();
            ArrayUtility.Add<string>(ref m_AvailablePalettesAsString, string.Empty);
            ArrayUtility.Add<string>(ref m_AvailablePalettesAsString, "Add Palette...");
            m_CurrentPaletteIndex = System.Array.IndexOf(m_AvailablePalettes, colorPalette);
        }

        void RebuildCaches(EditableObject target, BrushSettings settings)
        {
            PolyMesh m = target.editMesh;
            int vertexCount = m.vertexCount;
            Color[] newBaseColors = null;

            if(m.colors != null && m.colors.Length == vertexCount)
                newBaseColors = Util.Duplicate(m.colors);
            else
                newBaseColors = Util.Fill<Color>(x => { return Color.white; }, vertexCount);

            EditableObjectData data;
            if(m_EditableObjectsData.TryGetValue(target, out data))
            {
                data.MeshVertexColors.Build(newBaseColors);
                data.MeshVertexColors.RebuildColorTargets(m_BrushColor, settings.strength, mask);
            }
        }
    }
}
