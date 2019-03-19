using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Brush mode for moving vertices in a direction.
    /// </summary>
    internal class BrushModeSmooth : BrushModeSculpt
	{
		const float SMOOTH_STRENGTH_MODIFIER = .1f;

        [UserSetting]
        internal static Pref<PolyDirection> s_SmoothDirection = new Pref<PolyDirection>("Brush.Direction", PolyDirection.VertexNormal, SettingsScope.Project);
        /// <summary>
        /// If true vertices on the edge of a mesh will not be affected by brush strokes. It is up to inheriting
        /// classes to implement this preference (use `nonManifoldIndices` HashSet to check if a vertex index is
        /// non-manifold).
        /// </summary>
        [UserSetting]
        internal static Pref<bool> s_IgnoreOpenEdges = new Pref<bool>("SmoothBrush.IgnoreOpenEdges", true, SettingsScope.Project);
        [UserSetting]
        internal static Pref<bool> s_UseFirstNormalVector = new Pref<bool>("SmoothBrush.UseFirstNormalVector", false, SettingsScope.Project);

		Vector3[] vertices = null;
		Dictionary<int, List<int>> neighborLookup = new Dictionary<int, List<int>>();
		List<List<int>> commonVertices = null;
		int commonVertexCount;

		internal override string UndoMessage { get { return "Smooth Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Smooth Settings"; } }
		protected override string DocsLink { get { return PrefUtility.documentationSmoothBrushLink; } }

        internal override void DrawGUI(BrushSettings settings)
		{
			base.DrawGUI(settings);

            EditorGUI.BeginChangeCheck();

            s_IgnoreOpenEdges.value = PolyGUILayout.Toggle(BrushModeSculpt.Styles.gcIgnoreOpenEdges, s_IgnoreOpenEdges);
            if (s_SmoothDirection == PolyDirection.BrushNormal)
                s_UseFirstNormalVector.value = PolyGUILayout.Toggle(BrushModeSculpt.Styles.gcBrushNormalIsSticky, s_UseFirstNormalVector);
            s_SmoothDirection.value = (PolyDirection)PolyGUILayout.PopupFieldWithTitle(BrushModeSculpt.Styles.gcDirection,
                (int)s_SmoothDirection.value, BrushModeSculpt.Styles.s_BrushDirectionList);

            if (EditorGUI.EndChangeCheck())
                PolybrushSettings.Save();
        }

		internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

            if (!likelyToSupportVertexSculpt)
                return;

            vertices = target.editMesh.vertices;
			neighborLookup = PolyMeshUtility.GetAdjacentVertices(target.editMesh);
			commonVertices = PolyMeshUtility.GetCommonVertices(target.editMesh);
			commonVertexCount = commonVertices.Count;
		}

		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
		{
            if (!likelyToSupportVertexSculpt)
                return;

            int rayCount = target.raycastHits.Count;

			Vector3[] normals = (s_SmoothDirection == PolyDirection.BrushNormal) ? target.editableObject.editMesh.normals : null;

			Vector3 v, t, avg, dirVec = s_SmoothDirection.value.ToVector3();
			Plane plane = new Plane(Vector3.up, Vector3.zero);
			PolyMesh mesh = target.editableObject.editMesh;
			int vertexCount = mesh.vertexCount;

			// don't use target.GetAllWeights because brush normal needs
			// to know which ray to use for normal
			for(int ri = 0; ri < rayCount; ri++)
			{
				PolyRaycastHit hit = target.raycastHits[ri];

				if(hit.weights == null || hit.weights.Length < vertexCount)
					continue;

                for (int i = 0; i < commonVertexCount; i++)
                {
					int index = commonVertices[i][0];

                    if (hit.weights[index] < .0001f || (s_IgnoreOpenEdges && nonManifoldIndices.Contains(index)))
                        continue;

					v = vertices[index];

                    if (s_SmoothDirection == PolyDirection.VertexNormal)
                    {
						avg = PolyMath.Average(vertices, neighborLookup[index]);
					}
					else
					{
						avg = PolyMath.WeightedAverage(vertices, neighborLookup[index], hit.weights);

                        if (s_SmoothDirection == PolyDirection.BrushNormal)
                        {
                            if (s_UseFirstNormalVector)
                                dirVec = brushNormalOnBeginApply[ri];
							else
								dirVec = PolyMath.WeightedAverage(normals, neighborLookup[index], hit.weights).normalized;
						}

						plane.SetNormalAndPosition(dirVec, avg);
						avg = v - dirVec * plane.GetDistanceToPoint(v);
					}

					t = Vector3.Lerp(v, avg, hit.weights[index]);
					List<int> indices = commonVertices[i];

					Vector3 pos = v + (t-v) * settings.strength * SMOOTH_STRENGTH_MODIFIER;

					for(int n = 0; n < indices.Count; n++)
						vertices[indices[n]] = pos;
				}
			}

			mesh.vertices = vertices;

			if(tempComponent != null)
				tempComponent.OnVerticesMoved(mesh);

			base.OnBrushApply(target, settings);
		}
	}
}
