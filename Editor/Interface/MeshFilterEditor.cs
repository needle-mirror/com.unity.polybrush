using UnityEngine;
using UnityEngine.Polybrush;


namespace UnityEditor.Polybrush
{
    /// <summary>
    /// A Custom editor for mesh filters
    /// </summary>
	[CustomEditor(typeof(MeshFilter)), CanEditMultipleObjects]
	internal class MeshFilterEditor : Editor
	{
        GUIContent m_GCSaveButton = new GUIContent("Save to Asset", "Save this instance mesh to an Asset so that you can use it as a prefab.");

        public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedProperty mesh = serializedObject.FindProperty("m_Mesh");

			if(mesh != null)
				EditorGUILayout.PropertyField(mesh);

			Mesh m = (Mesh) mesh.objectReferenceValue;

			if(m != null)
			{
				ModelSource source = PolyEditorUtility.GetMeshGUID(m);
                GameObject go = ((MeshFilter)serializedObject.targetObject).gameObject;
                bool isPBMesh = ProBuilderInterface.IsProBuilderObject(go);

                if (source == ModelSource.Scene && !isPBMesh)
				{
					if(GUILayout.Button(m_GCSaveButton))
						PolyEditorUtility.SaveMeshAsset(m);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
