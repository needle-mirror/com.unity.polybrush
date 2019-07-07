using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Brush mode for moving vertices in a direction.
    /// </summary>
    internal class BrushModeRaiseLower : BrushModeSculpt
	{
        internal new static class Styles
        {
            internal static GUIContent s_GCBrushEffect = new GUIContent("Sculpt Power", "Defines the baseline distance that vertices will be moved when a brush is applied at full strength.");
        }
        // Modifier to apply on top of strength.  Translates to brush applications per second roughly.
        const float k_StrengthModifier = .01f;

        [UserSetting]
        internal static Pref<float> s_RaiseLowerStrength = new Pref<float>("RaiseLowerBrush.Strength", 5f, SettingsScope.Project);
        [UserSetting]
        internal static Pref<PolyDirection> s_RaiseLowerDirection = new Pref<PolyDirection>("RaiseLowerBrush.Direction", PolyDirection.BrushNormal, SettingsScope.Project);
        /// <summary>
        /// If true vertices on the edge of a mesh will not be affected by brush strokes. It is up to inheriting
        /// classes to implement this preference (use `nonManifoldIndices` HashSet to check if a vertex index is
        /// non-manifold).
        /// </summary>
        [UserSetting]
        internal static Pref<bool> s_IgnoreOpenEdges = new Pref<bool>("RaiseLowerBrush.IgnoreOpenEdges", true, SettingsScope.Project);
        [UserSetting]
        internal static Pref<bool> s_UseFirstNormalVector = new Pref<bool>("RaiseLowerBrush.StickToFirstAppliedDirection", true, SettingsScope.Project);

		Vector3[] vertices = null;
		Dictionary<int, Vector3> normalLookup = null;
		int[][] commonVertices = null;
		int commonVertexCount;

		protected override string DocsLink { get { return PrefUtility.documentationSculptBrushLink; } }
        internal override string UndoMessage { get { return "Sculpt Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Sculpt Settings"; } }



		internal override void DrawGUI(BrushSettings settings)
		{
			base.DrawGUI(settings);

            EditorGUI.BeginChangeCheck();

            s_IgnoreOpenEdges.value = PolyGUILayout.Toggle(BrushModeSculpt.Styles.gcIgnoreOpenEdges, s_IgnoreOpenEdges);
            if (s_RaiseLowerDirection == PolyDirection.BrushNormal)
                s_UseFirstNormalVector.value = PolyGUILayout.Toggle(BrushModeSculpt.Styles.gcBrushNormalIsSticky, s_UseFirstNormalVector);

            s_RaiseLowerDirection.value = (PolyDirection)PolyGUILayout.PopupFieldWithTitle(BrushModeSculpt.Styles.gcDirection,
                (int)s_RaiseLowerDirection.value, BrushModeSculpt.Styles.s_BrushDirectionList);

            s_RaiseLowerStrength.value = PolyGUILayout.FloatField(Styles.s_GCBrushEffect, s_RaiseLowerStrength);
            
            if (EditorGUI.EndChangeCheck())
                PolybrushSettings.Save();
        }

		internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);

            if (!likelyToSupportVertexSculpt)
                return;

            vertices = target.editMesh.vertices;
			normalLookup = PolyMeshUtility.GetSmoothNormalLookup(target.editMesh);
			commonVertices = PolyMeshUtility.GetCommonVertices(target.editMesh);
			commonVertexCount = commonVertices.Length;
		}

		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
		{
            if (!likelyToSupportVertexSculpt)
                return;

            int rayCount = target.raycastHits.Count;

			if(rayCount < 1)
				return;

			Vector3 n = s_RaiseLowerDirection.value.ToVector3();

			float scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
            float sign = settings.isUserHoldingControl ? -1f : 1f;//Event.current != null ? (Event.current.control ? -1f : 1f) : 1f;

			float maxMoveDistance = settings.strength * k_StrengthModifier * sign * s_RaiseLowerStrength;
			int vertexCount = target.editableObject.vertexCount;

			PolyMesh mesh = target.editableObject.editMesh;

            // rayCount could be different from brushNormalOnBeginApply.Count with some shapes (example: sphere).
			for(int ri = 0; ri < rayCount && ri < brushNormalOnBeginApply.Count; ri++)
			{
				PolyRaycastHit hit = target.raycastHits[ri];

				if(hit.weights == null || hit.weights.Length < vertexCount)
					continue;

				if(s_RaiseLowerDirection == PolyDirection.BrushNormal )
				{
                    if (s_UseFirstNormalVector)
                        n = brushNormalOnBeginApply[ri];
					else
						n = target.raycastHits[ri].normal;

					scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
				}

				for(int i = 0; i < commonVertexCount; i++)
				{
					int index = commonVertices[i][0];

					if(hit.weights[index] < .0001f || (s_IgnoreOpenEdges && nonManifoldIndices.Contains(index)))
						continue;

					if(s_RaiseLowerDirection == PolyDirection.VertexNormal)
					{
						n = normalLookup[index];
						scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
					}

					Vector3 pos = vertices[index] + n * (hit.weights[index] * maxMoveDistance * scale);

					int[] indices = commonVertices[i];

					for(int it = 0; it < indices.Length; it++)
						vertices[indices[it]] = pos;
				}
			}

			mesh.vertices = vertices;
            target.editableObject.modifiedChannels |= MeshChannel.Position;

			// different than setting weights on temp component,
			// which is what BrushModeMesh.OnBrushApply does.
			if(tempComponent != null)
				tempComponent.OnVerticesMoved(mesh);

			base.OnBrushApply(target, settings);
		}

        /// <summary>
        /// Draw gizmos taking into account handling of normal by raiser lower brush mode.
        /// </summary>
        /// <param name="target">Current target Object</param>
        /// <param name="settings">Current brush settings</param>
        internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
        {
            UpdateBrushGizmosColor();
            int rayCount = target.raycastHits.Count;
            for (int ri = 0; ri < rayCount; ri++)
            {
                PolyRaycastHit hit = target.raycastHits[ri];

                Vector3 normal = hit.normal;
                switch (s_RaiseLowerDirection.value)
                {
                    case PolyDirection.BrushNormal:
                        {
                            if (s_UseFirstNormalVector && brushNormalOnBeginApply.Count > ri)
                                normal = brushNormalOnBeginApply[ri];
                        }
                        break;
                    case PolyDirection.Up:
                    case PolyDirection.Right:
                    case PolyDirection.Forward:
                        {
                            normal = DirectionUtil.ToVector3(s_RaiseLowerDirection);
                        }
                        break;
                    case PolyDirection.VertexNormal:
                        {
                            //For vertex normal mode we take the vertex with the highest weight to compute the normal
                            //if non has enough we take the hit normal.
                            float highestWeight = .0001f;
                            int highestIndex = -1;
                            for (int i = 0; i < commonVertexCount; i++)
                            {
                                int index = commonVertices[i][0];

                                if (hit.weights[index] < .0001f || (s_IgnoreOpenEdges && nonManifoldIndices.Contains(index)))
                                    continue;

                                if (hit.weights[index] > highestWeight)
                                {
                                    highestIndex = index;
                                }
                            }

                            if (highestIndex != -1)
                            {
                                normal = normalLookup[highestIndex];
                            }
                        }
                        break;
                };

                normal = settings.isUserHoldingControl ? normal * -1f : normal;
                PolyHandles.DrawBrush(hit.position, normal, settings, target.localToWorldMatrix, innerColor, outerColor);
            }
        }
    }
}
