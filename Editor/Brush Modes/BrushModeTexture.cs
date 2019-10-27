// #define POLYBRUSH_DEBUG

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;
using System;

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

        [System.NonSerialized]
        SplatSet splat_cache = null,
                    splat_target = null,
                    splat_erase = null,
                    splat_current = null;

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

        [SerializeField]
        int m_VertexCount = 0;

        bool m_LikelySupportsTextureBlending = true;

        AttributeLayoutContainer m_MeshAttributesContainer = null;
        List<AttributeLayoutContainer> m_MeshAttributesContainers = new List<AttributeLayoutContainer>();

        string[] m_AvailableMaterialsAsString = { };
        EditableObject m_CacheTarget = null;
        List<Material> m_CacheMaterials = null;

        Dictionary<PolyEdge, List<int>> m_TriangleLookup = null;

        // temp vars
        PolyEdge[] m_FillModeEdges = new PolyEdge[3];
        List<int> m_FillModeAdjacentTriangles = null;

        internal int currentMeshACIndex = 0;
        internal PaintMode paintMode = PaintMode.Brush;
        
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
			
			if (meshAttributes != null)    
                OnMaterialSelected();
			
			foreach(GameObject go in Selection.gameObjects)
			{
				m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(go);

				if(m_LikelySupportsTextureBlending)
					break;
			}
		}

        internal override bool SetDefaultSettings()
        {
            paintMode = PaintMode.Brush;
            return true;
        }

        // Inspector GUI shown in the Editor window.  Base class shows BrushSettings by default
        internal override void DrawGUI(BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                paintMode = (PaintMode)GUILayout.Toolbar((int)paintMode, Styles.k_ModeIcons, GUILayout.Width(150));
                GUILayout.FlexibleSpace();
            }

			GUILayout.Space(4);

            // Selection dropdown for material (for submeshes)
            if (m_AvailableMaterialsAsString.Count() > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.LabelField("Material :", GUILayout.Width(60));
                if (m_CurrentPanelView == PanelView.Configuration)
                    GUI.enabled = false;

                currentMeshACIndex = EditorGUILayout.Popup(currentMeshACIndex, m_AvailableMaterialsAsString, "Popup");

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
                    m_MeshAttributesContainer = m_MeshAttributesContainers[currentMeshACIndex];
                    OnMaterialSelected();
                }

                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();

            if (m_CurrentPanelView == PanelView.Paint)
                DrawGUIPaintView();
            else if (m_CurrentPanelView == PanelView.Configuration)
            {
                Material selectedMat = m_CacheMaterials[currentMeshACIndex];

                string[] names = selectedMat.GetTexturePropertyNames();
            
                using (new GUILayout.VerticalScope())
                {
                    for (int i = 0; i < names.Length; ++i)
                    {
                        string n = names[i];
                        if (selectedMat.HasProperty(n))
                        {
                            DrawConfigurationPanel(GetPropertyInfo(n), n, selectedMat);
                        }
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
            if (currentMeshACIndex < 0 || currentMeshACIndex > (m_CacheMaterials.Count - 1))
                throw new IndexOutOfRangeException("currentMeshACIndex");

            Material selectedMat = m_CacheMaterials[currentMeshACIndex];
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

            Material mat = m_CacheMaterials[currentMeshACIndex];
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
                Material mat = m_CacheMaterials[currentMeshACIndex];
                Shader shader = mat.shader;

                ShaderMetaDataUtility.SaveShaderMetaData(shader, m_LoadedAttributes);
                foreach(GameObject go in Selection.gameObjects)
                {
                    m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(go);

                    if(m_LikelySupportsTextureBlending)
                        break;
                }

                if (m_CacheTarget != null)
                {
                    RebuildCaches(m_CacheTarget.editMesh);
                }
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
			RebuildColorTargets(brushColor, settings.strength);
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

        /// <summary>
        /// Helper function for comparing whether two lists are equivalent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>true if lists are equivalent, false otherwise</returns>
        private bool AreListsEqual<T>(List<T> a, List<T> b)
        {
            if(a.Count != b.Count)
            {
                return false;
            }

            for(int i = 0, n = a.Count; i < n; i++)
            {
                if(!a[i].Equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
		// Called when the mouse begins hovering an editable object.
		internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

			if(target.editMesh == null)
				return;

            bool refresh = (m_CacheTarget != null && !m_CacheTarget.Equals(target)) || m_CacheTarget == null;
            if (m_CacheTarget != null && m_CacheTarget.Equals(target.gameObjectAttached))
            {
                var targetMaterials = target.gameObjectAttached.GetMaterials();
                refresh = !AreListsEqual(targetMaterials, m_CacheMaterials);
            }

            if (refresh)
            {
                m_CacheTarget = target;
                m_CacheMaterials = target.gameObjectAttached.GetMaterials();
                m_MeshAttributesContainer = null;
                currentMeshACIndex = 0;
                ArrayUtility.Clear(ref m_AvailableMaterialsAsString);
                m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(target.gameObjectAttached);
                RebuildCaches(target.editMesh);
            }

            if (m_LikelySupportsTextureBlending && (brushColor == null || !brushColor.MatchesAttributes(meshAttributes)))
            {
                brushColor = new SplatWeight(SplatWeight.GetChannelMap(meshAttributes));
                SetBrushColorWithAttributeIndex(selectedAttributeIndex);
            }
            RebuildColorTargets(brushColor, settings.strength);
        }

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		internal override void OnBrushMove(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushMove(target, settings);

			if(!Util.IsValid(target) || !m_LikelySupportsTextureBlending || meshAttributes.Length == 0)
				return;

			bool invert = settings.isUserHoldingControl;
			float[] weights;

			if(paintMode == PaintMode.Brush)
			{
				weights = target.GetAllWeights();
			}
			else if(paintMode == PaintMode.Flood)
			{
				weights = new float[m_VertexCount];

				for(int i = 0; i < m_VertexCount; i++)
					weights[i] = 1f;
			}
			else
			{
				weights = new float[m_VertexCount];
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
							if(m_TriangleLookup.TryGetValue(m_FillModeEdges[i], out m_FillModeAdjacentTriangles))
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

			splat_current.LerpWeights(splat_cache, invert ? splat_erase : splat_target, mask, weights);
			splat_current.Apply(target.editableObject.editMesh);
			target.editableObject.ApplyMeshAttributes();
		}

		// Called when the mouse exits hovering an editable object.
		internal override void OnBrushExit(EditableObject target)
		{
			base.OnBrushExit(target);

			if(splat_cache != null)
			{
				splat_cache.Apply(target.editMesh);
				target.ApplyMeshAttributes();
				target.graphicsMesh.UploadMeshData(false);
			}

			//likelySupportsTextureBlending = true;
		}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
		{
			if(!m_LikelySupportsTextureBlending)
				return; 

			splat_current.CopyTo(splat_cache);
			splat_cache.Apply(target.editableObject.editMesh);

            MeshChannel channelsChanged =
                MeshChannel.Color | MeshChannel.Tangent | MeshChannel.UV0 | MeshChannel.UV2 | MeshChannel.UV3 | MeshChannel.UV4;
            target.editableObject.modifiedChannels |= channelsChanged;
            base.OnBrushApply(target, settings);
		}

		// set mesh splat_current back to their original state before registering for undo
		internal override void RegisterUndo(BrushTarget brushTarget)
		{
			if(splat_cache != null)
			{
				splat_cache.Apply(brushTarget.editableObject.editMesh);
				brushTarget.editableObject.ApplyMeshAttributes();
			}

			base.RegisterUndo(brushTarget);
		}

		internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
		{
			PolyMesh mesh = target.editableObject.editMesh;

			if(Util.IsValid(target) && paintMode == PaintMode.Fill)
			{
				Vector3[] vertices = mesh.vertices;
				int[] indices = mesh.GetTriangles();

				PolyHandles.PushMatrix();
				PolyHandles.PushHandleColor();

				Handles.matrix = target.transform.localToWorldMatrix;

				int index = 0;

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
							if(m_TriangleLookup.TryGetValue(m_FillModeEdges[i], out m_FillModeAdjacentTriangles))
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

				PolyHandles.PopHandleColor();
				PolyHandles.PopMatrix();
			}
			else
			{
				base.DrawGizmos(target, settings);
			}

		}

	    internal void RefreshPreviewTextureCache()
	    {
	        if (m_CacheTarget != null)
	        {
	            for (int i = 0; i < meshAttributes.Length; ++i)
	            {
	                AttributeLayout attributes = meshAttributes[i];
	                attributes.previewTexture = (Texture2D)m_CacheMaterials[currentMeshACIndex].GetTexture(attributes.propertyTarget);
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

        void RebuildColorTargets(SplatWeight blend, float strength)
        {
            if (blend == null || splat_cache == null || splat_target == null)
                return;

            m_MinWeights = splat_target.GetMinWeights();

            splat_target.LerpWeights(splat_cache, blend, strength);
            
            // get index of texture that is being painted
            var attrib = meshAttributes[selectedAttributeIndex];
            int index = blend.GetAttributeIndex(attrib);
                
            var baseTexture = attrib.isBaseTexture? attrib : GetBaseTexture();
            int baseTexIndex = -1;
            if (baseTexture != null)
            {
                baseTexIndex = blend.GetAttributeIndex(baseTexture);
            }

            splat_erase.LerpWeightOnSingleChannel(splat_cache, m_MinWeights, strength, index, baseTexIndex);
        }

        void SetupBaseTextures(SplatSet set)
        {
            // map base texture index to mask index
            // map mask index to indices of other textures in mask
            Dictionary<int, int> baseTexToMask = new Dictionary<int, int>();
            Dictionary<int, List<int>> maskToIndices = new Dictionary<int, List<int>>();
            
            foreach(var attr in meshAttributes)
            {
                if (attr.isBaseTexture)
                {
                    baseTexToMask.Add((int)attr.index, attr.mask);
                }
                else
                {
                    List<int> indices;
                    if (maskToIndices.TryGetValue(attr.mask, out indices))
                    {
                        indices.Add((int)attr.index);
                        maskToIndices[attr.mask] = indices;
                    }
                    else
                    {
                        maskToIndices.Add(attr.mask, new List<int>() { (int)attr.index });
                    }
                }
            }
            set.SetChannelBaseTextureWeights(MeshChannel.Color, baseTexToMask, maskToIndices);
        }

        void RebuildCaches(PolyMesh m)
        {
            m_VertexCount = m.vertexCount;
            m_TriangleLookup = PolyMeshUtility.GetAdjacentTriangles(m);

            if (meshAttributes == null)
            {
                // clear caches
                splat_cache = null;
                splat_current = null;
                splat_target = null;
                splat_erase = null;
                return;
            }

            splat_cache = new SplatSet(m, meshAttributes);
            SetupBaseTextures(splat_cache);
            splat_current = new SplatSet(splat_cache);
            splat_target = new SplatSet(m_VertexCount, meshAttributes);
            splat_erase = new SplatSet(m_VertexCount, meshAttributes);
        }

    }
}
