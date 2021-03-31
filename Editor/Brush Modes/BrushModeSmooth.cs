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
        class EditableObjectData
        {
            public Vector3[] vertices;
            public Dictionary<int, int[]> neighborLookup;
            public int[][] commonVertices;
            public int commonVertexCount;
        }

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

        Dictionary<EditableObject,EditableObjectData> m_EditableObjectsData = new Dictionary<EditableObject, EditableObjectData>();

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

            if (!m_LikelyToSupportVertexSculpt)
                return;

            EditableObjectData data;
            if(!m_EditableObjectsData.TryGetValue(target, out data))
            {
                data = new EditableObjectData();
                m_EditableObjectsData.Add(target, data);
            }
            data.vertices = target.editMesh.vertices;
            data.neighborLookup = PolyMeshUtility.GetAdjacentVertices(target.editMesh);
            data.commonVertices = PolyMeshUtility.GetCommonVertices(target.editMesh);
            data.commonVertexCount = data.commonVertices.Length;
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

            int rayCount = target.raycastHits.Count;

            Vector3 v, t, avg, dirVec = s_SmoothDirection.value.ToVector3();
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            PolyMesh mesh = target.editableObject.editMesh;
            Vector3[] normals = (s_SmoothDirection == PolyDirection.BrushNormal) ? mesh.normals : null;
            int vertexCount = mesh.vertexCount;

            List <Vector3> brushNormalOnBeginApply= BrushNormalsOnBeginApply(target.editableObject);
            var data = m_EditableObjectsData[target.editableObject];

            // don't use target.GetAllWeights because brush normal needs
            // to know which ray to use for normal
            for(int ri = 0; ri < rayCount; ri++)
            {
                PolyRaycastHit hit = target.raycastHits[ri];

                if(hit.weights == null || hit.weights.Length < vertexCount)
                    continue;

                for (int i = 0; i < data.commonVertexCount; i++)
                {
                    int index = data.commonVertices[i][0];

                    if (hit.weights[index] < .0001f || (s_IgnoreOpenEdges && ContainsIndexInNonManifoldIndices(target.editableObject,index)))
                        continue;

                    v = data.vertices[index];

                    if (s_SmoothDirection == PolyDirection.VertexNormal)
                    {
                        avg = Math.Average(data.vertices, data.neighborLookup[index]);
                    }
                    else
                    {
                        avg = Math.WeightedAverage(data.vertices, data.neighborLookup[index], hit.weights);

                        if (s_SmoothDirection == PolyDirection.BrushNormal)
                        {
                            if (s_UseFirstNormalVector)
                                dirVec = brushNormalOnBeginApply[ri].normalized;
                            else
                                dirVec = Math.WeightedAverage(normals, data.neighborLookup[index], hit.weights).normalized;
                        }

                        plane.SetNormalAndPosition(dirVec, avg);
                        avg = v - dirVec * plane.GetDistanceToPoint(v);
                    }

                    t = Vector3.Lerp(v, avg, hit.weights[index]);
                    int[] indices = data.commonVertices[i];

                    Vector3 pos = v + (t-v) * settings.strength * SMOOTH_STRENGTH_MODIFIER;

                    for(int n = 0; n < indices.Length; n++)
                        data.vertices[indices[n]] = pos;
                }
            }

            mesh.vertices = data.vertices;
            base.OnBrushApply(target, settings);
            UpdateWireframe(target, settings);
        }

        /// <summary>
        /// Draw gizmos taking into account handling of normal by smooth brush mode.
        /// </summary>
        /// <param name="target">Current target Object</param>
        ///// <param name="settings">Current brush settings</param>
        internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
        {
            UpdateBrushGizmosColor();
            Vector3 normal = s_SmoothDirection.value.ToVector3();

            int rayCount = target.raycastHits.Count;
            PolyMesh mesh = target.editableObject.editMesh;
            int vertexCount = mesh.vertexCount;
            Vector3[] normals = (s_SmoothDirection == PolyDirection.BrushNormal) ? mesh.normals : null;

            List <Vector3> brushNormalOnBeginApply = BrushNormalsOnBeginApply(target.editableObject);
            var data = m_EditableObjectsData[target.editableObject];

            // don't use target.GetAllWeights because brush normal needs
            // to know which ray to use for normal
            for (int ri = 0; ri < rayCount; ri++)
            {
                PolyRaycastHit hit = target.raycastHits[ri];

                if (hit.weights == null || hit.weights.Length < vertexCount)
                    continue;

                if (s_SmoothDirection == PolyDirection.BrushNormal)
                {
                    if (s_UseFirstNormalVector && brushNormalOnBeginApply.Count > ri)
                    {
                        normal = brushNormalOnBeginApply[ri];
                    }
                    else
                    {
                        // get the highest weighted vertex to use its direction computation
                        float highestWeight = .0001f;
                        int highestIndex = -1;
                        for (int i = 0; i < data.commonVertexCount; i++)
                        {
                            int index = data.commonVertices[i][0];
                            if (hit.weights[index] < .0001f || (s_IgnoreOpenEdges && ContainsIndexInNonManifoldIndices(target.editableObject, index)))
                                continue;

                            if (hit.weights[index] > highestWeight)
                                highestIndex = index;
                        }

                        normal = highestIndex != -1 ?
                            Math.WeightedAverage(normals, data.neighborLookup[highestIndex], hit.weights).normalized :
                            hit.normal;
                    }
                }
                else if (s_SmoothDirection == PolyDirection.VertexNormal)
                {
                    normal = hit.normal;
                }

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
