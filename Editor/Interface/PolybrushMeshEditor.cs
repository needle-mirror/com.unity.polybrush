using System;
using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Custom Editor for AdditionalVertexStreams
    /// </summary>
	[CanEditMultipleObjects]
    [CustomEditor(typeof(PolybrushMesh))]
    public class PolybrushMeshEditor : Editor
    {
        static class Styles
        {
            internal const string k_WarningCannotApplyAVS = "Warning! This object's base mesh (shown in the 'Mesh Filter' component) has a different vertex count than the 'Additional Vertex Streams' mesh. Polybrush will not apply AVS on this object until this is fixed.";
            internal const string k_WarningCannotApplyAVSMultipleObjectsStringFormat = "Warning! For {0} of the selected objects, the base mesh (shown in the 'Mesh Filter' component) has a different vertex count than the 'Additional Vertex Streams' mesh. Polybrush will not apply AVS on these objects until this is fixed.";
            internal const string k_DeleteButtonLabel = "Delete";
            internal const string k_UndoDeleteRecordLabel = "Delete AdditionalVertexStreams";
            internal const string k_FormatUndoSwitchMode = "PolybrushMesh mode to {0}";

            internal static GUIContent k_AVSLabel = new GUIContent("Additional Vertex Streams");
            internal static GUIContent k_ExportButtonLabel = new GUIContent("Export Mesh Asset", "Save this instance mesh to an Asset so that you can use it as a prefab.");
            internal static GUIContent k_ApplyAsLabel = new GUIContent("Apply as");

            internal static string k_DisplayDialogTitle = "Delete Mesh Data?";
            internal static string k_DisplayDialogMessage = "You are removing the Polybrush Mesh component, which contains all sculpting, painting, etc on this object. If you do not want to lose these changes, please export the mesh to an asset before removing the PolybrushMesh component.";
            internal static string k_DisplayDialogOkLabel = "Remove";
            internal static string k_DisplayDialogCancelLabel = "Cancel";

            internal static GUIContent[] k_ApplyAsOptions = new GUIContent[]
            {
                new GUIContent("Overwrite Mesh"),
                new GUIContent("Additional Vertex Stream")
            };
        }

        void OnEnable()
        {
            if(PolybrushEditor.instance == null)
            {
                var mesh = ( target as PolybrushMesh );
#if UNITY_2019_1_OR_NEWER
                if(mesh != null && mesh.gameObject.TryGetComponent<ZoomOverride>(out var zoom))
                    DestroyImmediate(zoom);
#else
                if(mesh != null)
                    DestroyImmediate(mesh.gameObject.GetComponent<ZoomOverride>());
#endif
            }
        }

        /// <summary>
        /// Overriding function to make a custom IMGUI inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            PolybrushMesh polybrushmesh = target as PolybrushMesh;

            EditorGUILayout.Space();
            DrawModeGUI();

            if (polybrushmesh.mode == PolybrushMesh.Mode.AdditionalVertexStream)
                DrawAdditionalVertexStreamsGUI();

            EditorGUILayout.Space();
            DrawExtraActions();
        }

        void DrawModeGUI()
        {
            EditorGUI.BeginChangeCheck();

            PolybrushMesh polybrushmesh = target as PolybrushMesh;

            if (polybrushmesh.type == PolybrushMesh.ObjectType.SkinnedMesh)
                GUI.enabled = false;

            PolybrushMesh.Mode newMode = (PolybrushMesh.Mode)EditorGUILayout.Popup(Styles.k_ApplyAsLabel, (int)polybrushmesh.mode, Styles.k_ApplyAsOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(polybrushmesh, string.Format(Styles.k_FormatUndoSwitchMode, newMode.ToString()));
                polybrushmesh.mode = newMode;
            }

            if (polybrushmesh.type == PolybrushMesh.ObjectType.SkinnedMesh)
                GUI.enabled = true;
        }

        void DrawAdditionalVertexStreamsGUI()
        {
            PolybrushMesh polybrushmesh = target as PolybrushMesh;
            MeshRenderer mr = polybrushmesh.gameObject.GetComponent<MeshRenderer>();

            // Don't draw AVS section when working on SkinnedMeshes as
            // they don't support it.
            if (polybrushmesh.type == PolybrushMesh.ObjectType.SkinnedMesh)
                return;

            if (targets.Length > 1)
                EditorGUI.showMixedValue = true;

            GUI.enabled = false;
            if (mr != null)
                EditorGUILayout.ObjectField(Styles.k_AVSLabel, mr.additionalVertexStreams, typeof(Mesh), true);
            GUI.enabled = true;

            EditorGUI.showMixedValue = false;

            int count = 0;
            foreach (PolybrushMesh polybrushMesh in targets)
            {
                if (!polybrushMesh.CanApplyAdditionalVertexStreams())
                {
                    // Do the following to make sure to catch cases where
                    // an user would change the referenced mesh in MeshFilter.
                    if (polybrushMesh.hasAppliedAdditionalVertexStreams)
                        polybrushMesh.RemoveAdditionalVertexStreams();

                    ++count;
                }
            }

            if (count > 1)
                EditorGUILayout.HelpBox(string.Format(Styles.k_WarningCannotApplyAVSMultipleObjectsStringFormat, count.ToString()), MessageType.Warning, true);
            else if (count == 1)
                EditorGUILayout.HelpBox(Styles.k_WarningCannotApplyAVS, MessageType.Warning, true);

        }

        void DrawExtraActions()
        {
            PolybrushMesh polybrushmesh = target as PolybrushMesh;
            MeshRenderer mr = polybrushmesh.gameObject.GetComponent<MeshRenderer>();

            using (new EditorGUILayout.HorizontalScope())
            {
                // Export button
                if (GUILayout.Button(Styles.k_ExportButtonLabel))
                    PolyEditorUtility.SaveMeshAsset(polybrushmesh.polyMesh.ToUnityMesh());

                // Reset button
                if (GUILayout.Button(Styles.k_DeleteButtonLabel))
                {
                    if (EditorUtility.DisplayDialog(Styles.k_DisplayDialogTitle,
                        Styles.k_DisplayDialogMessage, Styles.k_DisplayDialogOkLabel, Styles.k_DisplayDialogCancelLabel))
                    {

                        foreach (PolybrushMesh polybrushMesh in targets)
                        {
                            if (polybrushMesh == null)
                                continue;

                            polybrushmesh.SetMesh(polybrushmesh.sourceMesh);
                            Undo.DestroyObjectImmediate(polybrushmesh);
                        }
                    }
                }
            }
        }

        bool HasFrameBounds()
        {
            var mesh = ( target as PolybrushMesh );
#if UNITY_2019_1_OR_NEWER
            if(mesh != null && mesh.gameObject.TryGetComponent<ZoomOverride>(out var zoom))
                return zoom.HasFrameBounds();
#else
            if(mesh != null)
            {
                var zoom = mesh.gameObject.GetComponent<ZoomOverride>();
                if(zoom != null)
                    return zoom.HasFrameBounds();
            }
#endif
            return false;
        }

        Bounds OnGetFrameBounds()
        {
            var mesh = ( target as PolybrushMesh );
#if UNITY_2019_1_OR_NEWER
            if(mesh != null && mesh.gameObject.TryGetComponent<ZoomOverride>(out var zoom))
                return zoom.OnGetFrameBounds();
#else
            if(mesh != null)
            {
                var zoom = mesh.gameObject.GetComponent<ZoomOverride>();
                if(zoom != null)
                    return zoom.OnGetFrameBounds();
            }
#endif
            return new Bounds();
        }
    }
}
