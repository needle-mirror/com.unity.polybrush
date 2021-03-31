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
        class EditableObjectData
        {
            public Vector3[] Vertices;
            public Dictionary<int, Vector3> NormalLookup;
            public int[][] CommonVertices;
            public int CommonVertexCount;
        }

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

		protected override string DocsLink { get { return PrefUtility.documentationSculptBrushLink; } }
        internal override string UndoMessage { get { return "Sculpt Vertices"; } }
		protected override string ModeSettingsHeader { get { return "Sculpt Settings"; } }

        Dictionary<EditableObject,EditableObjectData> m_EditableObjectsData = new Dictionary<EditableObject, EditableObjectData>();

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

            if (!m_LikelyToSupportVertexSculpt)
                return;

            EditableObjectData data;
            if(!m_EditableObjectsData.TryGetValue(target, out data))
            {
                data = new EditableObjectData();
                m_EditableObjectsData.Add(target, data);
            }
            data.Vertices = target.editMesh.vertices;
			data.NormalLookup = PolyMeshUtility.GetSmoothNormalLookup(target.editMesh);
			data.CommonVertices = PolyMeshUtility.GetCommonVertices(target.editMesh);
			data.CommonVertexCount = data.CommonVertices.Length;
        }

        // Called when the mouse exits hovering an editable object.
        internal override void OnBrushExit(EditableObject target)
        {
            base.OnBrushExit(target);

            if(m_EditableObjectsData.ContainsKey(target))
                m_EditableObjectsData.Remove(target);
        }

		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
		{
            if (!m_LikelyToSupportVertexSculpt)
                return;

            if(!m_EditableObjectsData.ContainsKey(target.editableObject))
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

            EditableObjectData data = m_EditableObjectsData[target.editableObject];
            List <Vector3> brushNormalOnBeginApply= BrushNormalsOnBeginApply(target.editableObject);

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
						n = hit.normal;

					scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
				}

				for(int i = 0; i < data.CommonVertexCount; i++)
				{
					int index = data.CommonVertices[i][0];

					if(hit.weights[index] < .0001f || (s_IgnoreOpenEdges && ContainsIndexInNonManifoldIndices(target.editableObject,index)))
						continue;

					if(s_RaiseLowerDirection == PolyDirection.VertexNormal)
					{
						n = data.NormalLookup[index];
						scale = 1f / ( Vector3.Scale(target.transform.lossyScale, n).magnitude );
					}

					Vector3 pos = data.Vertices[index] + n * (hit.weights[index] * maxMoveDistance * scale);

					int[] indices = data.CommonVertices[i];

					for(int it = 0; it < indices.Length; it++)
						data.Vertices[indices[it]] = pos;
				}
			}

			mesh.vertices = data.Vertices;
            target.editableObject.modifiedChannels |= MeshChannel.Position;

			base.OnBrushApply(target, settings);

            // different than setting weights on temp component,
            // which is what BrushModeMesh.OnBrushApply does.
            UpdateWireframe(target, settings);
		}

        /// <summary>
        /// Draw gizmos taking into account handling of normal by raiser lower brush mode.
        /// </summary>
        /// <param name="target">Current target Object</param>
        /// <param name="settings">Current brush settings</param>
        internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
        {
            if(!m_EditableObjectsData.ContainsKey(target.editableObject))
                return;

            EditableObjectData data = m_EditableObjectsData[target.editableObject];

            UpdateBrushGizmosColor();
            int rayCount = target.raycastHits.Count;
            List <Vector3> brushNormalOnBeginApply= BrushNormalsOnBeginApply(target.editableObject);

            for (int ri = 0; ri < rayCount; ri++)
            {
                PolyRaycastHit hit = target.raycastHits[ri];

                Vector3 normal = hit.normal;
                switch (s_RaiseLowerDirection.value)
                {
                    case PolyDirection.BrushNormal:
                    case PolyDirection.VertexNormal:
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
                };

                normal = settings.isUserHoldingControl ? normal * -1f : normal;

                PolyHandles.DrawBrush(hit.position,
                    normal,
                    settings,
                    target.localToWorldMatrix,
                    innerColor,
                    outerColor);
            }
        }
    }
}
