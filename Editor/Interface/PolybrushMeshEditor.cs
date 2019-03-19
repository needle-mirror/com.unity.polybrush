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
		public override void OnInspectorGUI()
		{
			var polybrushObject = target as PolybrushMesh;

			if(polybrushObject == null)
				return;

			MeshRenderer mr = polybrushObject.gameObject.GetComponent<MeshRenderer>();

			GUILayout.Label("Additional Vertex Streams");

			if(targets.Length > 1)
				EditorGUI.showMixedValue = true;

            if(mr != null)
			    EditorGUILayout.ObjectField(mr.additionalVertexStreams, typeof(Mesh), true);

			EditorGUI.showMixedValue = false;

			if(GUILayout.Button("Delete"))
			{
				foreach(PolybrushMesh polyMesh in targets)
				{
					if(polyMesh == null)
						continue;

					mr = polyMesh.GetComponent<MeshRenderer>();

					if(mr != null)
						mr.additionalVertexStreams = null;

					if(polyMesh.storedMesh!= null)
					{
						Undo.DestroyObjectImmediate(polyMesh);
						Undo.RecordObject(mr, "Delete AdditionalVertexStreams");
					}
				}
			}
		}
	}
}
