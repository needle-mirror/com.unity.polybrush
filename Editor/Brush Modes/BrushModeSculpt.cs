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

        protected bool likelyToSupportVertexSculpt = true;

		protected List<Vector3> brushNormalOnBeginApply = new List<Vector3>();
		protected Vector3[] cached_normals;

		internal override string UndoMessage { get { return "Sculpt Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Sculpt Settings"; } }

		protected HashSet<int> nonManifoldIndices = null;
        private EditableObject cache_target = null;
        private List<Material> cache_materials = null;

        internal override void OnEnable()
		{
			base.OnEnable();

            foreach (GameObject go in Selection.gameObjects)
            {
                likelyToSupportVertexSculpt = CheckForVertexScluptSupport(go);

                if (likelyToSupportVertexSculpt)
                    break;
            }
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
			nonManifoldIndices = PolyMeshUtility.GetNonManifoldIndices(target.editMesh);

            RefreshVertexSculptSupport(target);
        }

        /// <summary>
        /// Will ensure that the check is not done everytime (inspired from BrushModeTexture script)
        /// </summary>
        /// <param name="target"></param>
        void RefreshVertexSculptSupport(EditableObject target)
        {
            bool refresh = (cache_target != null && !cache_target.Equals(target.gameObjectAttached)) || cache_target == null;
            if (cache_target != null && cache_target.Equals(target.gameObjectAttached))
                refresh = cache_materials != target.gameObjectAttached.GetMaterials();

            if (refresh)
            {
                cache_target = target;
                cache_materials = target.gameObjectAttached.GetMaterials();
                likelyToSupportVertexSculpt = CheckForVertexScluptSupport(target.gameObjectAttached);
            }
        }

        internal override void DrawGUI(BrushSettings settings)
		{
			base.DrawGUI(settings);

            if (!likelyToSupportVertexSculpt)
            {
                EditorGUILayout.HelpBox("Sculpting on skin meshes is not supported.", MessageType.Warning);
            }
		}

        /// <summary>
        /// Cache the brush normals and the mesh normals
        /// </summary>
        /// <param name="target">Target object to cache the brush normals and mesh normals from</param>
		protected void CacheBrushNormals(BrushTarget target)
		{
			brushNormalOnBeginApply.Clear();

			for(int i = 0; i < target.raycastHits.Count; i++)
				brushNormalOnBeginApply.Add(target.raycastHits[i].normal);

			PolyMesh mesh = target.editableObject.editMesh;

			cached_normals = new Vector3[mesh.vertexCount];

            if (mesh.normals != null && mesh.normals.Length == mesh.vertexCount)
            {
                System.Array.Copy(mesh.normals, 0, cached_normals, 0, mesh.vertexCount);
                target.editableObject.modifiedChannels |= MeshChannel.Normal;
            }
		}

		internal override void OnBrushBeginApply(BrushTarget target, BrushSettings settings)
		{
			CacheBrushNormals(target);
			base.OnBrushBeginApply(target, settings);
		}

        internal override void OnBrushFinishApply(BrushTarget target, BrushSettings settings)
        {
            brushNormalOnBeginApply.Clear();
            base.OnBrushFinishApply(target, settings);
        }

        protected override void CreateTempComponent(EditableObject target)
		{
            RefreshVertexSculptSupport(target);
            if (!likelyToSupportVertexSculpt)
                return;

            OverlayRenderer ren = target.gameObjectAttached.AddComponent<OverlayRenderer>();
			ren.SetMesh(target.editMesh);

            ren.fullColor = s_FullStrengthColor;
            ren.gradient = s_BrushGradientColor;
            ren.vertexBillboardSize = s_VertexBillboardSize;

			tempComponent = ren;
		}

		protected override void UpdateTempComponent(BrushTarget target, BrushSettings settings)
		{
			if(tempComponent != null)
				((OverlayRenderer)tempComponent).SetWeights(target.GetAllWeights(), settings.strength);
		}
	}
}
