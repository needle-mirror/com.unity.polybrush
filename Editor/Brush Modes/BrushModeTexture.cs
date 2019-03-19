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
		[System.NonSerialized]
		SplatSet 	splat_cache = null,
					splat_target = null,
					splat_erase = null,
					splat_current = null;

        internal SplatWeight brushColor = null;

        SplatWeight m_MinWeights = null;

		[SerializeField]
        int m_SelectedAttributeIndex = -1;

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

        internal AttributeLayout[] meshAttributes
		{
			get
			{
				return m_MeshAttributesContainer != null ? m_MeshAttributesContainer.attributes : null;
			}
		}

		GUIContent[] m_ModeIcons = new GUIContent[]
		{
			new GUIContent("Brush", "Brush" ),
			new GUIContent("Fill", "Fill" ),
			new GUIContent("Flood", "Flood" )
		};


        // The message that will accompany Undo commands for this brush.  Undo/Redo is handled by PolyEditor.
        internal override string UndoMessage { get { return "Paint Brush"; } }
		protected override string ModeSettingsHeader { get { return "Texture Paint Settings"; } }
		protected override string DocsLink { get { return PrefUtility.documentationTextureBrushLink; } }

		internal override void OnEnable()
		{
			base.OnEnable();

			m_LikelySupportsTextureBlending = false;
			m_MeshAttributesContainer = null;
			brushColor = null;

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
                paintMode = (PaintMode)GUILayout.Toolbar((int)paintMode, m_ModeIcons, GUILayout.Width(130));
                GUILayout.FlexibleSpace();
            }

			GUILayout.Space(4);

			if(!m_LikelySupportsTextureBlending)
			{
				EditorGUILayout.HelpBox("It doesn't look like any of the materials on this object support texture blending!\n\nSee the readme for information on creating custom texture blend shaders.", MessageType.Warning);
			}

            // Selection dropdown for material (for submeshes)
            if (m_AvailableMaterialsAsString.Count() > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.LabelField("Material :", GUILayout.Width(60));
                currentMeshACIndex = EditorGUILayout.Popup(currentMeshACIndex, m_AvailableMaterialsAsString, "Popup");

                if (EditorGUI.EndChangeCheck())
                    m_MeshAttributesContainer = m_MeshAttributesContainers[currentMeshACIndex];

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4);

            if (meshAttributes != null)
			{
			    RefreshPreviewTextureCache();
                int prevSelectedAttributeIndex = m_SelectedAttributeIndex;
				m_SelectedAttributeIndex = SplatWeightEditor.OnInspectorGUI(m_SelectedAttributeIndex, ref brushColor, meshAttributes);
				if(prevSelectedAttributeIndex != m_SelectedAttributeIndex)
					SetBrushColorWithAttributeIndex(m_SelectedAttributeIndex);

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
			AttributeLayoutContainer detectedMeshAttributes;
            bool supports = false;
            var materials = Util.GetMaterials(go);
            m_MeshAttributesContainers.Clear();
            Material mat;
            List<int> indexes = new List<int>();
            for(int i = 0; i < materials.Count; i++)
			{
                mat = materials[i];
				if(PolyShaderUtil.GetMeshAttributes(mat, out detectedMeshAttributes))
				{
					m_MeshAttributesContainers.Add(detectedMeshAttributes);
                    indexes.Add(i);
                    supports = true;
				}
			}
            if (supports)
            {
                m_MeshAttributesContainer = m_MeshAttributesContainers.First();
                foreach(int i in indexes)
                    ArrayUtility.Add<string>(ref m_AvailableMaterialsAsString, materials[i].name);
            }
			return supports;
		}

		internal void SetBrushColorWithAttributeIndex(int index)
		{
			if(	brushColor == null ||
				meshAttributes == null ||
				index < 0 ||
				index >= meshAttributes.Length)
				return;

			m_SelectedAttributeIndex = index;

			if(meshAttributes[index].mask > -1)
			{
				for(int i = 0; i < meshAttributes.Length; i++)
				{
					if(meshAttributes[i].mask == meshAttributes[index].mask)
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

            bool refresh = (m_CacheTarget != null && !m_CacheTarget.Equals(target.gameObjectAttached)) || m_CacheTarget == null;
            if(m_CacheTarget != null && m_CacheTarget.Equals(target.gameObjectAttached))
                refresh = m_CacheMaterials != target.gameObjectAttached.GetMaterials();

            if (refresh)
            {
                m_CacheTarget = target;
                m_CacheMaterials = target.gameObjectAttached.GetMaterials();
                m_MeshAttributesContainer = null;
                currentMeshACIndex = 0;
                ArrayUtility.Clear(ref m_AvailableMaterialsAsString);
                m_LikelySupportsTextureBlending = CheckForTextureBlendSupport(target.gameObjectAttached);
            }

            if (m_LikelySupportsTextureBlending && (brushColor == null || !brushColor.MatchesAttributes(meshAttributes)))
            {
                brushColor = new SplatWeight(SplatWeight.GetChannelMap(meshAttributes));
                SetBrushColorWithAttributeIndex(Mathf.Clamp(m_SelectedAttributeIndex, 0, meshAttributes.Length - 1));
            }
            RebuildCaches(target.editMesh, settings.strength);
            RebuildColorTargets(brushColor, settings.strength);
        }

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		internal override void OnBrushMove(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushMove(target, settings);

			if(!Util.IsValid(target) || !m_LikelySupportsTextureBlending)
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

			if(m_SelectedAttributeIndex < 0 || m_SelectedAttributeIndex >= meshAttributes.Length)
				SetBrushColorWithAttributeIndex(0);

			int mask = meshAttributes[m_SelectedAttributeIndex].mask;

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

        void RebuildColorTargets(SplatWeight blend, float strength)
        {
            if (blend == null || splat_cache == null || splat_target == null)
                return;

            m_MinWeights = splat_target.GetMinWeights();

            splat_target.LerpWeights(splat_cache, blend, strength);
            splat_erase.LerpWeights(splat_cache, m_MinWeights, strength);
        }

        void RebuildCaches(PolyMesh m, float strength)
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
            splat_current = new SplatSet(splat_cache);
            splat_target = new SplatSet(m_VertexCount, meshAttributes);
            splat_erase = new SplatSet(m_VertexCount, meshAttributes);
        }

    }
}
