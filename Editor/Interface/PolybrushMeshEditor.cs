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
            internal const string k_AdditionalVertexStreamsLabel = "Additional Vertex Streams";
            internal const string k_DeleteButtonLabel = "Delete";
            internal const string k_UndoDeleteRecordLabel = "Delete AdditionalVertexStreams";
        }
        
		public override void OnInspectorGUI()
		{
			var polybrushObject = target as PolybrushMesh;

			if(polybrushObject == null)
				return;

			MeshRenderer mr = polybrushObject.gameObject.GetComponent<MeshRenderer>();

			GUILayout.Label(Styles.k_AdditionalVertexStreamsLabel);

			if(targets.Length > 1)
				EditorGUI.showMixedValue = true;

            if(mr != null)
			    EditorGUILayout.ObjectField(mr.additionalVertexStreams, typeof(Mesh), true);

			EditorGUI.showMixedValue = false;
            
            int count = 0;
            foreach (PolybrushMesh polybrushMesh in targets)
            {
                if (!polybrushMesh.CanApplyAdditionalVertexStreams())
                {
                    /// Do the following to make sure to catch cases where
                    /// an user would change the referenced mesh in MeshFilter.
                    if (polybrushMesh.hasAppliedAdditionalVertexStreams)
                        polybrushMesh.RemoveAdditionalVertexStreams();

                    ++count;
                }
            }

            if (count > 1)
                EditorGUILayout.HelpBox(string.Format(Styles.k_WarningCannotApplyAVSMultipleObjectsStringFormat, count.ToString()), MessageType.Warning, true);
            else if (count == 1)
                EditorGUILayout.HelpBox(Styles.k_WarningCannotApplyAVS, MessageType.Warning, true);

			if(GUILayout.Button(Styles.k_DeleteButtonLabel))
			{
				foreach(PolybrushMesh polybrushMesh in targets)
				{
					if(polybrushMesh == null)
						continue;

                    polybrushMesh.RemoveAdditionalVertexStreams();

					if(polybrushMesh.storedMesh!= null)
					{
						Undo.DestroyObjectImmediate(polybrushMesh);
						Undo.RecordObject(mr, Styles.k_UndoDeleteRecordLabel);
					}
				}
			}
		}
	}
}
