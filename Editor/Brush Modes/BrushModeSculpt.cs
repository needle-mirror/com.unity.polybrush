using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Base class for brush modes that move vertices around.  Implements an overlay preview.
    /// </summary>
    internal abstract class BrushModeSculpt : BrushModeMesh
	{
        class EditableObjectData
        {
            public List<Vector3> BrushNormalOnBeginApply = new List<Vector3>();
            public Vector3[] CachedNormals;

            public HashSet<int> NonManifoldIndices;
            public EditableObject CacheTarget;
            public List<Material> CacheMaterials;

            public OverlayRenderer TempComponent;
        }

        internal static class Styles
        {
            /// <summary>
            /// List built based on PolyDirection enum.
            /// Make sure to properly convert values between this list and PolyDirection.
            /// </summary>
            internal static string[] s_BrushDirectionList = new string[]
            {
                "Brush Normal",
                "Vertex Normal",
                "Global Y Axis",
                "Global X Axis",
                "Global Z Axis"
            };

            internal static GUIContent gcDirection = new GUIContent("Direction", "How vertices are moved when the brush is applied.  You can explicitly set an axis, or use the vertex normal.");
            internal static GUIContent gcIgnoreOpenEdges = new GUIContent("Ignore Open Edges", "When on, edges that are not connected on both sides will be ignored by brush strokes.");
            internal static GUIContent gcBrushNormalIsSticky = new GUIContent("Brush Normal is Sticky", "If enabled, vertices will be moved only on the direction of the brush normal at the time of first application.");
        }

        [UserSetting]
        internal static Pref<float> s_VertexBillboardSize = new Pref<float>("Brush.VertexBillboardSize", 2f, SettingsScope.Project);

        [UserSettingBlock("General Settings")]
        static void HandleBrushPreferences(string searchContext)
        {
            s_VertexBillboardSize.value = SettingsGUILayout.SettingsSlider(new GUIContent("Vertex Render Size", "The size at which selected vertices will be rendered."), s_VertexBillboardSize, 0f, 10f, searchContext);
        }

        Dictionary<EditableObject,EditableObjectData> m_EditableObjectsData = new Dictionary<EditableObject, EditableObjectData>();

        protected bool m_LikelyToSupportVertexSculpt = true;

		internal override string UndoMessage { get { return "Sculpt Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Sculpt Settings"; } }

        internal override void OnEnable()
		{
			base.OnEnable();

            foreach (GameObject go in Selection.gameObjects)
            {
                m_LikelyToSupportVertexSculpt = CheckForVertexScluptSupport(go);

                if (m_LikelyToSupportVertexSculpt)
                    break;
            }
        }

        protected List<Vector3> BrushNormalsOnBeginApply(EditableObject target)
        {
            return m_EditableObjectsData[target].BrushNormalOnBeginApply;
        }

        protected bool ContainsIndexInNonManifoldIndices(EditableObject target, int index)
        {
            return m_EditableObjectsData[target].NonManifoldIndices.Contains(index);
        }

        /// <summary>
        /// Check if the sculpt is supported for the current selection. No support for skin meshes
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        bool CheckForVertexScluptSupport(GameObject go)
        {
            return !go.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

            EditableObjectData data;
            if(!m_EditableObjectsData.TryGetValue(target, out data))
            {
                data = new EditableObjectData();
                m_EditableObjectsData.Add(target, data);
            }

            data.NonManifoldIndices = PolyMeshUtility.GetNonManifoldIndices(target.editMesh);

            RefreshVertexSculptSupport(target);
        }

        // Called when the mouse exits hovering an editable object.
        internal override void OnBrushExit(EditableObject target)
        {
            base.OnBrushExit(target);

            if(m_EditableObjectsData.ContainsKey(target))
            {
                DestroyImmediate(m_EditableObjectsData[target].TempComponent);
                m_EditableObjectsData.Remove(target);
            }
        }

        /// <summary>
        /// Will ensure that the check is not done everytime (inspired from BrushModeTexture script)
        /// </summary>
        /// <param name="target"></param>
        void RefreshVertexSculptSupport(EditableObject target)
        {
            EditableObjectData data;
            if(m_EditableObjectsData.TryGetValue(target, out data))
            {
                bool refresh = (data.CacheTarget != null && !data.CacheTarget.Equals(target.gameObjectAttached)) || data.CacheTarget == null;
                if (data.CacheTarget != null && data.CacheTarget.Equals(target.gameObjectAttached))
                    refresh = data.CacheMaterials != target.gameObjectAttached.GetMaterials();

                if (refresh)
                {
                    data.CacheTarget = target;
                    data.CacheMaterials = target.gameObjectAttached.GetMaterials();
                    m_LikelyToSupportVertexSculpt = CheckForVertexScluptSupport(target.gameObjectAttached);
                }
            }
        }

        internal override void DrawGUI(BrushSettings settings)
		{
			base.DrawGUI(settings);

            if (!m_LikelyToSupportVertexSculpt)
                EditorGUILayout.HelpBox("Sculpting on skin meshes is not supported.", MessageType.Warning);
		}

        /// <summary>
        /// Cache the brush normals and the mesh normals
        /// </summary>
        /// <param name="target">Target object to cache the brush normals and mesh normals from</param>
		protected void CacheBrushNormals(BrushTarget target)
        {
            EditableObjectData data;
            if(m_EditableObjectsData.TryGetValue(target.editableObject, out data))
            {
                data.BrushNormalOnBeginApply.Clear();

                for(int i = 0; i < target.raycastHits.Count; i++)
                    data.BrushNormalOnBeginApply.Add(target.raycastHits[i].normal);

                PolyMesh mesh = target.editableObject.editMesh;

                data.CachedNormals = new Vector3[mesh.vertexCount];

                if(mesh.normals != null && mesh.normals.Length == mesh.vertexCount)
                {
                    System.Array.Copy(mesh.normals, 0, data.CachedNormals, 0, mesh.vertexCount);
                    target.editableObject.modifiedChannels |= MeshChannel.Normal;
                }
            }
        }

		internal override void OnBrushBeginApply(BrushTarget target, BrushSettings settings)
		{
			CacheBrushNormals(target);
			base.OnBrushBeginApply(target, settings);
		}

        internal override void OnBrushFinishApply(BrushTarget target, BrushSettings settings)
        {
            base.OnBrushFinishApply(target, settings);
            if(target!= null && m_EditableObjectsData.ContainsKey(target.editableObject))
                m_EditableObjectsData[target.editableObject].BrushNormalOnBeginApply.Clear();
        }

        protected override void CreateTempComponent(EditableObject target)
		{
            if(target == null)
                return;

            RefreshVertexSculptSupport(target);
            if (!m_LikelyToSupportVertexSculpt)
                return;

            OverlayRenderer ren = target.gameObjectAttached.AddComponent<OverlayRenderer>();
            ren.hideFlags = HideFlags.DontSave
                | HideFlags.NotEditable
                | HideFlags.HideInInspector
                | HideFlags.HideInHierarchy;
			ren.SetMesh(target.editMesh);

            ren.fullColor = s_FullStrengthColor;
            ren.gradient = s_BrushGradientColor;
            ren.vertexBillboardSize = s_VertexBillboardSize;

            EditableObjectData data;
            if(!m_EditableObjectsData.TryGetValue(target, out data))
            {
                data = new EditableObjectData();
                m_EditableObjectsData.Add(target, data);
            }
            data.TempComponent = ren;
        }

		protected override void UpdateTempComponent(BrushTarget target, BrushSettings settings)
        {
            if(!Util.IsValid(target))
                return;

            EditableObjectData data;
            if(m_EditableObjectsData.TryGetValue(target.editableObject, out data))
            {
                if(data.TempComponent != null)
                	((OverlayRenderer)data.TempComponent).SetWeights(target.GetAllWeights(), settings.strength);
            }
		}

		protected void UpdateWireframe(BrushTarget target, BrushSettings settings)
        {
            if(!Util.IsValid(target))
                return;

            if(m_EditableObjectsData.TryGetValue(target.editableObject, out EditableObjectData data))
            {
                if(data.TempComponent != null)
                	data.TempComponent.OnVerticesMoved(target.editableObject.editMesh);

                //Might be costly to do that on every wireframe update
                if(ProBuilderBridge.ProBuilderExists() && target.editableObject.isProBuilderObject)
                    ProBuilderBridge.RefreshEditor(false);
            }
		}
	}
}
