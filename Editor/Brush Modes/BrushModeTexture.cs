// #define POLYBRUSH_DEBUG

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Vertex texture painter brush mode.
    /// Similar to BrushModePaint, except it packs blend information into both the color32 and UV3/4 channels.
    /// </summary>
    internal class BrushModeTexture : BrushModeMesh
    {
        static class Styles
        {
            internal static readonly GUIContent[] k_ModeIcons = new GUIContent[]
            {
                new GUIContent("Brush", "Brush" ),
                new GUIContent("Fill", "Fill" ),
                new GUIContent("Flood", "Flood" )
            };

            internal static readonly string[] k_MeshChannel =
            {
                "Color",
                "Tangent",
                "UV1",
                "UV2",
                "UV3",
                "UV4"
            };

            internal static MeshChannel ToMeshChannel(int value)
            {
                switch (value)
                {
                    case 0:
                        return MeshChannel.Color;
                    case 1:
                        return MeshChannel.Tangent;
                    case 2:
                        return MeshChannel.UV0;
                    case 3:
                        return MeshChannel.UV2;
                    case 4:
                        return MeshChannel.UV3;
                    case 5:
                        return MeshChannel.UV4;
                }
                return MeshChannel.Null;
            }

            internal static int ToInt(MeshChannel channel)
            {
                switch (channel)
                {
                    case MeshChannel.Color:
                        return 0;
                    case MeshChannel.Tangent:
                        return 1;
                    case MeshChannel.UV0:
                        return 2;
                    case MeshChannel.UV2:
                        return 3;
                    case MeshChannel.UV3:
                        return 4;
                    case MeshChannel.UV4:
                        return 5;
                }
                return 0;
            }
        }

        enum PanelView
        {
            Paint,
            Configuration,
        }

        class EditableObjectData
        {
            public int VertexCount;
            public EditableObject CacheTarget;
            public List<Material> CacheMaterials;
            public SplatSet SplatCache,
                SplatTarget,
                SplatErase,
                SplatCurrent;
            public Dictionary<PolyEdge, List<int>> TriangleLookup;
        }

        internal SplatWeight brushColor = null;

        SplatWeight m_MinWeights = null;

        [SerializeField]
        int m_SelectedAttributeIndex = 0;
        int selectedAttributeIndex
        {
            get { return m_SelectedAttributeIndex; }
            set
            {
                m_SelectedAttributeIndex = Mathf.Clamp(value, 0, meshAttributes.Length-1);
            }
        }

        bool m_LikelySupportsTextureBlending = true;

        AttributeLayoutContainer m_MeshAttributesContainer = null;
        List<AttributeLayoutContainer> m_MeshAttributesContainers = new List<AttributeLayoutContainer>();

        string[] m_AvailableMaterialsAsString = { };
        EditableObject m_MainCacheTarget = null;
        List<Material> m_MainCacheMaterials = new List<Material>();

        Dictionary<EditableObject,EditableObjectData> m_EditableObjectsData = new Dictionary<EditableObject, EditableObjectData>();

        // temp vars
        PolyEdge[] m_FillModeEdges = new PolyEdge[3];
        List<int> m_FillModeAdjacentTriangles = null;

        internal int m_CurrentMeshACIndex = 0;
        internal PaintMode m_PaintMode = PaintMode.Brush;

        PanelView m_CurrentPanelView = PanelView.Paint;

        internal AttributeLayout[] meshAttributes
		{
			get
			{
				return m_MeshAttributesContainer != null ? m_MeshAttributesContainer.attributes : null;
			}
		}

        // The message that will accompany Undo commands for this brush.  Undo/Redo is handled by PolyEditor.
        internal override string UndoMessage { get { return "Paint Brush"; } }
		protected override string ModeSettingsHeader { get { return "Texture Paint Settings"; } }
		protected override string DocsLink { get { return PrefUtility.documentationTextureBrushLink; } }

		internal override void OnEnable()
		{
			base.OnEnable();

            m_CurrentPanelView = PanelView.Paint;

			m_LikelySupportsTextureBlending = false;
			m_MeshAttributesContainer = null;
			brushColor = null;

            RebuildMaterialCaches();

			if (meshAttributes != null)
                OnMaterialSelected();

			foreach(GameObject go in Selection.gameObjects)
				m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(go);
		}

        internal override bool SetDefaultSettings()
        {
            m_PaintMode = PaintMode.Brush;
            return true;
        }

        // Inspector GUI shown in the Editor window.  Base class shows BrushSettings by default
        internal override void DrawGUI(BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                m_PaintMode = (PaintMode)GUILayout.Toolbar((int)m_PaintMode, Styles.k_ModeIcons, GUILayout.Width(150));
                GUILayout.FlexibleSpace();
            }

			GUILayout.Space(4);

            // Selection dropdown for material (for submeshes)
            if (m_AvailableMaterialsAsString.Count() > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.LabelField("Material", GUILayout.Width(60));
                if (m_CurrentPanelView == PanelView.Configuration)
                    GUI.enabled = false;

                m_CurrentMeshACIndex = EditorGUILayout.Popup(m_CurrentMeshACIndex, m_AvailableMaterialsAsString, "Popup");

                if (m_CurrentPanelView == PanelView.Configuration)
                    GUI.enabled = true;

                // Buttons to switch between Paint and Configuration views
                if (m_CurrentPanelView == PanelView.Paint && GUILayout.Button("Configure", GUILayout.Width(70)))
                    OpenConfiguration();
                else if (m_CurrentPanelView == PanelView.Configuration)
                {
                    if (GUILayout.Button("Revert", GUILayout.Width(70)))
                        CloseConfiguration(false);
                    if (GUILayout.Button("Save", GUILayout.Width(70)))
                        CloseConfiguration(true);

                }

                if (EditorGUI.EndChangeCheck())
                {
                    m_MeshAttributesContainer = m_MeshAttributesContainers[m_CurrentMeshACIndex];
                    OnMaterialSelected();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (m_CurrentPanelView == PanelView.Paint)
                DrawGUIPaintView();
            else if (m_CurrentPanelView == PanelView.Configuration)
            {
                Material selectedMat = m_MainCacheMaterials[m_CurrentMeshACIndex];

                string[] names = selectedMat.GetTexturePropertyNames();

                using (new GUILayout.VerticalScope())
                {
                    for (int i = 0; i < names.Length; ++i)
                    {
                        string n = names[i];
                        if (selectedMat.HasProperty(n))
                            DrawConfigurationPanel(GetPropertyInfo(n), n, selectedMat);
                    }
                }
            }
        }

        struct MaterialPropertyInfo
        {
            public string PropertyName;
            public bool LinkedToAttributesLayout;
            public bool IsVisible;
        }

        List<MaterialPropertyInfo> materialPropertiesCache = new List<MaterialPropertyInfo>();

        AttributeLayoutContainer m_LoadedAttributes = null;

        private MaterialPropertyInfo GetPropertyInfo(string name)
        {
            MaterialPropertyInfo res = default(MaterialPropertyInfo);
            foreach (MaterialPropertyInfo p in materialPropertiesCache)
            {
                if (p.PropertyName == name)
                {
                    res = p;
                    break;
                }
            }
            return res;
        }

        private void UpdatePropertyInfo(MaterialPropertyInfo pUpdate)
        {
            int index = materialPropertiesCache.FindIndex(0, p => p.PropertyName == pUpdate.PropertyName);
            if (index >= 0)
                materialPropertiesCache[index] = pUpdate;
        }

        private void OnMaterialSelected()
        {
            Material selectedMat = m_MainCacheMaterials[m_CurrentMeshACIndex];
            string[] names = selectedMat.GetTexturePropertyNames();

            materialPropertiesCache.Clear();

            foreach (string n in names)
            {
                if (selectedMat.HasProperty(n))
                {
                    materialPropertiesCache.Add(new MaterialPropertyInfo()
                    {
                        PropertyName = n,
                        LinkedToAttributesLayout = false,
                        IsVisible = true
                    });
                }
            }
        }

        private void OpenConfiguration()
        {
            m_CurrentPanelView = PanelView.Configuration;

            if (m_MainCacheMaterials == null || m_CurrentMeshACIndex < 0 || m_CurrentMeshACIndex >= m_MainCacheMaterials.Count)
                return;
            Material mat = m_MainCacheMaterials[m_CurrentMeshACIndex];
            Shader shader = mat.shader;

            if (ShaderMetaDataUtility.IsValidShader(shader))
            {
#pragma warning disable 0618
                // Data conversion between Polybrush Beta and Polybrush 1.0.
                string path = ShaderMetaDataUtility.FindPolybrushMetaDataForShader(shader);
                if (!string.IsNullOrEmpty(path))
                    ShaderMetaDataUtility.ConvertMetaDataToNewFormat(shader);
#pragma warning restore 0618

                m_LoadedAttributes = ShaderMetaDataUtility.LoadShaderMetaData(shader);
            }
        }

        private void CloseConfiguration(bool saveOnDisk)
        {
            if (saveOnDisk)
            {
                Material mat = m_MainCacheMaterials[m_CurrentMeshACIndex];
                Shader shader = mat.shader;

                ShaderMetaDataUtility.SaveShaderMetaData(shader, m_LoadedAttributes);
                foreach(GameObject go in Selection.gameObjects)
                    m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(go);
            }

            m_LoadedAttributes = null;
            m_CurrentPanelView = PanelView.Paint;
        }

        private void DrawGUIPaintView()
        {
            if (meshAttributes != null)
            {
                RefreshPreviewTextureCache();
                int prevSelectedAttributeIndex = selectedAttributeIndex;
                selectedAttributeIndex =
                    SplatWeightEditor.OnInspectorGUI(selectedAttributeIndex, ref brushColor, meshAttributes);
                if (prevSelectedAttributeIndex != selectedAttributeIndex)
                    SetBrushColorWithAttributeIndex(selectedAttributeIndex);

#if POLYBRUSH_DEBUG
				GUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();

				if(GUILayout.Button("MetaData", EditorStyles.miniButton))
				{
					Debug.Log(meshAttributes.ToString("\n"));

					string str = EditorUtility.FindPolybrushMetaDataForShader(meshAttributesContainer.shader);

					if(!string.IsNullOrEmpty(str))
					{
						TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(str);

						if(asset != null)
							EditorGUIUtility.PingObject(asset);
						else
							Debug.LogWarning("No MetaData found for Shader \"" + meshAttributesContainer.shader.name + "\"");
					}
					else
					{
						Debug.LogWarning("No MetaData found for Shader \"" + meshAttributesContainer.shader.name + "\"");
					}
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(4);

				if(GUILayout.Button("rebuild  targets"))
					RebuildColorTargets(brushColor, brushSettings.strength);


				GUILayout.Label(brushColor != null ? brushColor.ToString() : "brush color: null\n");
#endif
            }
            else
            {
                if (!m_LikelySupportsTextureBlending)
                {
                    EditorGUILayout.HelpBox("It doesn't look like any of the materials on this object support texture blending!\n\nSee the readme for information on creating custom texture blend shaders.", MessageType.Warning);
                }

            }
        }

        private void DrawConfigurationPanel(MaterialPropertyInfo guiInfo, string propertyName, Material mat)
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                using (new GUILayout.VerticalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    using (new GUILayout.HorizontalScope())
                    {
                        if (!m_LoadedAttributes.HasAttributes(propertyName))
                        {
                            GUILayout.Label(propertyName, GUILayout.Width(100));
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Create attributes",GUILayout.Width(120), GUILayout.Height(16)))
                            {
                                AttributeLayout newAttr = new AttributeLayout();
                                newAttr.propertyTarget = propertyName;
                                m_LoadedAttributes.AddAttribute(newAttr);
                            }
                        }
                        else
                        {
                            guiInfo.IsVisible = EditorGUILayout.Foldout(guiInfo.IsVisible, propertyName, true);
                            if (GUILayout.Button("Erase attributes", GUILayout.ExpandWidth(true), GUILayout.MinWidth(60), GUILayout.MaxWidth(120), GUILayout.Height(16)))
                                m_LoadedAttributes.RemoveAttribute(propertyName);
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdatePropertyInfo(guiInfo);
                    }

                    if (m_LoadedAttributes.HasAttributes(propertyName) && guiInfo.IsVisible)
                    {
                        AttributeLayout attr = m_LoadedAttributes.GetAttributes(propertyName);

                        EditorGUILayout.Space();

                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope(GUILayout.Width(70), GUILayout.ExpandWidth(false)))
                            {
                                Texture tex = mat.GetTexture(propertyName);
                                EditorGUI.DrawPreviewTexture(
                                    EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64)),
                                    (tex != null) ? tex : Texture2D.blackTexture);
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Channel");
                                GUILayout.Label("Index");
                                GUILayout.Label("Range");
                                GUILayout.Label("Group");
                                GUILayout.Label("Is Base Texture");
                            }
                            GUILayout.FlexibleSpace();

                            using (new GUILayout.VerticalScope())
                            {
                                // Channel selection
                                attr.channel =
                                    Styles.ToMeshChannel(EditorGUILayout.Popup(Styles.ToInt(attr.channel), Styles.k_MeshChannel, GUILayout.Width(140)));

                                // Index selection
                                attr.index = (ComponentIndex)GUILayout.Toolbar((int)attr.index, ComponentIndexUtility.ComponentIndexPopupDescriptions, GUILayout.Width(140));

                                // Value range
                                attr.range = EditorGUILayout.Vector2Field("", attr.range, GUILayout.Width(140));

                                // Group selection
                                attr.mask = EditorGUILayout.Popup(attr.mask, AttributeLayout.DefaultMaskDescriptions, GUILayout.Width(140));

                                attr.isBaseTexture = EditorGUILayout.Toggle(attr.isBaseTexture);
                            }
                        }
                    }
                }
            }
        }

        internal override void OnBrushSettingsChanged(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushSettingsChanged(target, settings);
			RebuildColorTargets(target?.editableObject, brushColor, settings.strength);
            //RebuildColorTargets(brushColor, settings.strength);
		}

        /// <summary>
        /// Test a gameObject and it's mesh renderers for compatible shaders, and if one is found
        /// load it's attribute data into meshAttributes.
        /// </summary>
        /// <param name="go">The GameObject being checked for texture blend support</param>
        /// <returns></returns>
        internal bool CheckForTextureBlendSupport(GameObject go)
		{
            bool supports = false;
            var materials = Util.GetMaterials(go);
            m_MeshAttributesContainers.Clear();
            Material mat;
            List<int> indexes = new List<int>();
            for(int i = 0; i < materials.Count; i++)
            {
                mat = materials[i];
                if (ShaderMetaDataUtility.IsValidShader(mat.shader))
                {
                    AttributeLayoutContainer detectedMeshAttributes = ShaderMetaDataUtility.LoadShaderMetaData(mat.shader);
                    {
                        if (detectedMeshAttributes != null)
                        {
                            m_MeshAttributesContainers.Add(detectedMeshAttributes);
                            indexes.Add(i);
                            m_MainCacheMaterials.Add(mat);
                            supports = true;
                        }
                    }
                }
            }

            if (supports)
            {
                m_MeshAttributesContainer = m_MeshAttributesContainers.First();
                foreach(int i in indexes)
                    ArrayUtility.Add<string>(ref m_AvailableMaterialsAsString, materials[i].name);
            }

            if (meshAttributes == null)
                supports = false;

            return supports;
		}

		internal void SetBrushColorWithAttributeIndex(int index)
		{
			if(	brushColor == null ||
				meshAttributes == null)
				return;

			if(meshAttributes[index].mask > -1)
			{
				for(int i = 0; i < meshAttributes.Length; i++)
				{
                    brushColor.SetAttributeValue(meshAttributes[i], meshAttributes[i].min);
				}
			}

			brushColor.SetAttributeValue(meshAttributes[index], meshAttributes[index].max);
		}

		// Called when the mouse begins hovering an editable object.
		internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
        {
			base.OnBrushEnter(target, settings);

			if(target.editMesh == null)
				return;

            bool refresh = (m_MainCacheTarget != null && !m_MainCacheTarget.Equals(target)) || m_MainCacheTarget == null;

            if (m_MainCacheTarget != null && m_MainCacheTarget.Equals(target))
            {
                var targetMaterials = target.gameObjectAttached.GetMaterials();
                refresh = !targetMaterials.SequenceEqual(m_MainCacheMaterials);
            }

            if (refresh)
            {
                SetActiveObject(target);
                RebuildMaterialCaches();
                PolybrushEditor.instance.Repaint();
            }

            if (m_LikelySupportsTextureBlending && (brushColor == null || !brushColor.MatchesAttributes(meshAttributes)))
            {
                brushColor = new SplatWeight(SplatWeight.GetChannelMap(meshAttributes));
                SetBrushColorWithAttributeIndex(selectedAttributeIndex);
            }
            RebuildColorTargets(target, brushColor, settings.strength);
        }

        void RebuildMaterialCaches()
        {
            ArrayUtility.Clear(ref m_AvailableMaterialsAsString);
            m_CurrentMeshACIndex = 0;
            m_MainCacheMaterials.Clear();
            if (m_MainCacheTarget == null)
                return;
            m_MeshAttributesContainer = null;
            m_CurrentMeshACIndex = 0;
            m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(m_MainCacheTarget.gameObjectAttached);
        }

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		internal override void OnBrushMove(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushMove(target, settings);

			if(!Util.IsValid(target) || !m_LikelySupportsTextureBlending || meshAttributes.Length == 0)
				return;

            if(!m_EditableObjectsData.ContainsKey(target.editableObject))
                return;

            var data = m_EditableObjectsData[target.editableObject];

            if(!data.CacheMaterials.Contains(m_MainCacheMaterials[m_CurrentMeshACIndex]))
                return;

			bool invert = settings.isUserHoldingControl;
			float[] weights;

			if(m_PaintMode == PaintMode.Brush)
			{
				weights = target.GetAllWeights();
			}
			else if(m_PaintMode == PaintMode.Flood)
			{
				weights = new float[data.VertexCount];

				for(int i = 0; i < data.VertexCount; i++)
					weights[i] = 1f;
			}
			else
			{
				weights = new float[data.VertexCount];
				int[] indices = target.editableObject.editMesh.GetTriangles();
				int index = 0;
				float weightTarget = 1f;

				foreach(PolyRaycastHit hit in target.raycastHits)
				{
					if(hit.triangle > -1)
					{
						index = hit.triangle * 3;

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

									weights[indices[index  ]] = weightTarget;
									weights[indices[index+1]] = weightTarget;
									weights[indices[index+2]] = weightTarget;
								}
							}
						}
					}
				}
			}

			int mask = meshAttributes[selectedAttributeIndex].mask;

            if(data.SplatCurrent == null)
                RebuildCaches(data);

			data.SplatCurrent.LerpWeights(data.SplatCache, invert ? data.SplatErase : data.SplatTarget, mask, weights);
			data.SplatCurrent.Apply(target.editableObject.editMesh);
			target.editableObject.ApplyMeshAttributes();
		}

		// Called when the mouse exits hovering an editable object.
		internal override void OnBrushExit(EditableObject target)
        {
			base.OnBrushExit(target);

            if(!m_EditableObjectsData.ContainsKey(target))
                return;

            var data = m_EditableObjectsData[target];
			if(data.SplatCache != null)
			{
				data.SplatCache.Apply(target.editMesh);
				target.ApplyMeshAttributes();
				target.graphicsMesh.UploadMeshData(false);
			}

            if(m_MainCacheTarget != null && data.CacheTarget.Equals(m_MainCacheTarget))
                m_MainCacheTarget = null;

            m_EditableObjectsData.Remove(target);
		}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
		{
			if(!m_LikelySupportsTextureBlending)
				return;

            var data = m_EditableObjectsData[target.editableObject];
			data.SplatCurrent.CopyTo(data.SplatCache);
			data.SplatCache.Apply(target.editableObject.editMesh);

            MeshChannel channelsChanged =
                MeshChannel.Color | MeshChannel.Tangent | MeshChannel.UV0 | MeshChannel.UV2 | MeshChannel.UV3 | MeshChannel.UV4;
            target.editableObject.modifiedChannels |= channelsChanged;
            base.OnBrushApply(target, settings);
		}

		// set mesh splat_current back to their original state before registering for undo
		internal override void RegisterUndo(BrushTarget brushTarget)
		{
            if(!m_EditableObjectsData.ContainsKey(brushTarget.editableObject))
                return;

            var data = m_EditableObjectsData[brushTarget.editableObject];
			if(data.SplatCache != null)
			{
				data.SplatCache.Apply(brushTarget.editableObject.editMesh);
				brushTarget.editableObject.ApplyMeshAttributes();
			}

			base.RegisterUndo(brushTarget);
		}

        internal override void UndoRedoPerformed(List<GameObject> modified)
        {
            base.UndoRedoPerformed(modified);
            foreach(var data in m_EditableObjectsData)
                RebuildCaches(data.Value);
        }

		internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
		{
			PolyMesh mesh = target.editableObject.editMesh;

			if(Util.IsValid(target) && m_PaintMode == PaintMode.Fill)
			{
				Vector3[] vertices = mesh.vertices;
				int[] indices = mesh.GetTriangles();

                using(new Handles.DrawingScope(target.transform.localToWorldMatrix))
                {
				    int index = 0;

                    var data = m_EditableObjectsData[target.editableObject];
				    foreach(PolyRaycastHit hit in target.raycastHits)
				    {
					    if(hit.triangle > -1)
					    {
						    Handles.color = Color.green;

						    index = hit.triangle * 3;

						    Handles.DrawLine(vertices[indices[index+0]] + hit.normal * .1f, vertices[indices[index+1]] + hit.normal * .1f);
						    Handles.DrawLine(vertices[indices[index+1]] + hit.normal * .1f, vertices[indices[index+2]] + hit.normal * .1f);
						    Handles.DrawLine(vertices[indices[index+2]] + hit.normal * .1f, vertices[indices[index+0]] + hit.normal * .1f);

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

									    Handles.DrawLine(vertices[indices[index+0]] + hit.normal * .1f, vertices[indices[index+1]] + hit.normal * .1f);
									    Handles.DrawLine(vertices[indices[index+1]] + hit.normal * .1f, vertices[indices[index+2]] + hit.normal * .1f);
									    Handles.DrawLine(vertices[indices[index+2]] + hit.normal * .1f, vertices[indices[index+0]] + hit.normal * .1f);
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

	    internal void RefreshPreviewTextureCache()
	    {
	        if (meshAttributes != null
                && m_MainCacheMaterials != null)
	        {
	            for (int i = 0; i < meshAttributes.Length; ++i)
	            {
	                AttributeLayout attributes = meshAttributes[i];
	                attributes.previewTexture = (Texture2D)m_MainCacheMaterials[m_CurrentMeshACIndex].GetTexture(attributes.propertyTarget);
	            }
	        }
	    }

        private AttributeLayout GetBaseTexture()
        {
            AttributeLayout selectedAttribute = meshAttributes[selectedAttributeIndex];
            foreach (var attr in meshAttributes)
            {
                if (attr.mask != selectedAttribute.mask)
                {
                    continue;
                }

                if (attr.isBaseTexture)
                {
                    return attr;
                }
            }
            return null;
        }

        void RebuildColorTargets(EditableObject target, SplatWeight blend, float strength)
        {
            if(target == null)
                return;

            var data = m_EditableObjectsData[target];

            if (blend == null || data.SplatCache == null || data.SplatTarget == null)
                return;

            m_MinWeights = data.SplatTarget.GetMinWeights();

            data.SplatTarget.LerpWeights(data.SplatCache, blend, strength);

            // get index of texture that is being painted
            var attrib = meshAttributes[selectedAttributeIndex];

            var baseTexture = attrib.isBaseTexture? attrib : GetBaseTexture();
            var baseTexIndex = baseTexture != null ? (int)baseTexture.index : -1;

            data.SplatErase.LerpWeightOnSingleChannel(data.SplatCache, m_MinWeights, strength, attrib.channel, (int)attrib.index, baseTexIndex);
        }

        void SetupBaseTextures(SplatSet set)
        {
            // map base texture index to mask index
            // map mask index to indices of other textures in mask
            Dictionary<MeshChannel, List<int>> channelsToBaseTex = new Dictionary<MeshChannel, List<int>>();
            Dictionary<int, int> baseTexToMask = new Dictionary<int, int>();
            Dictionary<int, List<int>> maskToIndices = new Dictionary<int, List<int>>();

            foreach(var attr in meshAttributes)
            {
                if (attr.isBaseTexture)
                {
                    if (channelsToBaseTex.TryGetValue(attr.channel, out List<int> baseTexIndices))
                    {
                        baseTexIndices.Add((int)attr.index);
                        channelsToBaseTex[attr.channel] = baseTexIndices;
                    }
                    else
                        channelsToBaseTex.Add(attr.channel, new List<int>() { (int)attr.index });

                    baseTexToMask.Add((int)attr.index, attr.mask);
                }
                else
                {
                    if (maskToIndices.TryGetValue(attr.mask, out List<int> indices))
                    {
                        indices.Add((int)attr.index);
                        maskToIndices[attr.mask] = indices;
                    }
                    else
                        maskToIndices.Add(attr.mask, new List<int>() { (int)attr.index });
                }
            }
            if(baseTexToMask.Count > 0)
                set.SetChannelBaseTextureWeights(channelsToBaseTex, baseTexToMask, maskToIndices);
        }

        void SetActiveObject(EditableObject activeObject)
        {
            m_MainCacheTarget = activeObject;

            EditableObjectData data;

            if(!m_EditableObjectsData.TryGetValue(activeObject, out data))
            {
                data = new EditableObjectData();
                m_EditableObjectsData.Add(activeObject, data);
            }

            data.CacheTarget = activeObject;
            data.CacheMaterials = activeObject.gameObjectAttached.GetMaterials();
            RebuildCaches(data);
        }

        void RebuildCaches(EditableObjectData data)
        {
            PolyMesh mesh = data.CacheTarget.editMesh;
            data.VertexCount = mesh.vertexCount;
            data.TriangleLookup = PolyMeshUtility.GetAdjacentTriangles(mesh);

            if (meshAttributes == null)
            {
                // clear caches
                data.SplatCache = null;
                data.SplatCurrent = null;
                data.SplatTarget = null;
                data.SplatErase = null;
                return;
            }

            data.SplatCache = new SplatSet(mesh, meshAttributes);
            SetupBaseTextures(data.SplatCache);
            data.SplatCurrent = new SplatSet(data.SplatCache);
            data.SplatTarget = new SplatSet(data.VertexCount, meshAttributes);
            data.SplatErase = new SplatSet(data.VertexCount, meshAttributes);
        }
    }
}
