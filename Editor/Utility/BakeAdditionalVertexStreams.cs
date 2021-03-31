using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Utility for applying vertex stream data directly to a mesh.  Can either override the existing
    /// mesh arrays or create a new mesh from the composite.
    /// </summary>
    internal class BakeAdditionalVertexStreams : EditorWindow
	{
		void OnEnable()
		{
			Undo.undoRedoPerformed += UndoRedoPerformed;
			autoRepaintOnSceneChange = true;
			OnSelectionChange();
		}

		void OnFocus()
		{
			OnSelectionChange();
		}

		void OnDisable()
		{
			Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		private List<PolybrushMesh> m_polyMeshes = new List<PolybrushMesh>();
		private Vector2 scroll = Vector2.zero;

		private GUIContent m_BatchNewMesh = new GUIContent("Create New Composite Mesh", "Create a new mesh for each selected mesh, automatically prefixing the built meshes with  and index.  This is useful in situations where you have used Additional Vertex Streams to paint a single mesh source many times and would like to ensure that all meshes remain unique.");

		void OnGUI()
		{
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Selected", EditorStyles.boldLabel);

                    scroll = EditorGUILayout.BeginScrollView(scroll, false, true);

                    foreach (PolybrushMesh polyMesh in m_polyMeshes)
                    {
                        if (polyMesh != null)
                            EditorGUILayout.LabelField(string.Format("{0} ({1})", polyMesh.gameObject.name, polyMesh.storedMesh == null ? "null" : polyMesh.storedMesh.name));
                    }

                    EditorGUILayout.EndScrollView();
                }

                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Bake Options", EditorStyles.boldLabel);

                    GUI.enabled = m_polyMeshes.Count == 1;

                    if (GUILayout.Button("Apply to Current Mesh"))
                    {
                        if (EditorUtility.DisplayDialog("Apply Vertex Streams to Mesh", "This action is not undo-able, are you sure you want to continue?", "Yes", "Cancel"))
                        {
                            foreach (var polyMesh in m_polyMeshes)
                                CreateComposite(polyMesh, true);
                        }

                        m_polyMeshes.Clear();
                    }

                    if (GUILayout.Button("Create New Composite Mesh"))
                    {
                        foreach (var polyMesh in m_polyMeshes)
                            CreateComposite(polyMesh, false);

                        m_polyMeshes.Clear();
                    }

                    GUI.enabled = m_polyMeshes.Count > 0;

                    EditorGUILayout.LabelField("Batch Options", EditorStyles.boldLabel);

                    if (GUILayout.Button(m_BatchNewMesh))
                    {
                        string path = EditorUtility.OpenFolderPanel("Save Vertex Stream Meshes", "Assets", "");

                        for (int i = 0; i < m_polyMeshes.Count; i++)
                        {
                            path = path.Replace(Application.dataPath, "Assets");

                            if (m_polyMeshes[i] == null || m_polyMeshes[i].storedMesh == null)
                                continue;

                            CreateComposite(m_polyMeshes[i], false, string.Format("{0}/{1}.asset", path, m_polyMeshes[i].storedMesh.name));
                        }

                        m_polyMeshes.Clear();
                    }
                }
            }
		}

		void OnSelectionChange()
		{
			m_polyMeshes = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<PolybrushMesh>()).ToList();
			Repaint();
		}

		void UndoRedoPerformed()
		{
			foreach(Mesh m in Selection.transforms.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Select(y => y.sharedMesh))
			{
				if(m != null)
					m.UploadMeshData(false);
			}
		}

        /// <summary>
        /// Create a Composite Mesh from AdditionnalVertexStreams
        /// </summary>
        /// <param name="vertexStream">the object used to create the composite mesh</param>
        /// <param name="applyToCurrent">Create a new Mesh or apply it directly to the current one?</param>
        /// <param name="path">where to save the new mesh</param>
		void CreateComposite(PolybrushMesh polyMesh, bool applyToCurrent, string path = null)
		{
			GameObject go = polyMesh.gameObject;

			Mesh source = go.GetMesh();
			Mesh mesh = polyMesh.storedMesh;

			if(source == null || mesh == null)
			{
				Debug.LogWarning("Mesh filter or vertex stream mesh is null, cannot continue.");
				return;
			}

			if(applyToCurrent)
			{
				CreateCompositeMesh(source, mesh, polyMesh.sourceMesh);

				MeshRenderer renderer = go.GetComponent<MeshRenderer>();

				if(renderer != null)
					renderer.additionalVertexStreams = null;

                Undo.DestroyObjectImmediate(polyMesh);
			}
			else
			{
				Mesh composite = new Mesh();
				CreateCompositeMesh(source, mesh, composite);

				if( string.IsNullOrEmpty(path) )
				{
					PolyEditorUtility.SaveMeshAsset(composite, go.GetComponent<MeshFilter>(), go.GetComponent<SkinnedMeshRenderer>());
				}
				else
				{
					AssetDatabase.CreateAsset(composite, path);

					MeshFilter mf = go.GetComponent<MeshFilter>();

					SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();

					if(mf != null)
						mf.sharedMesh = composite;
					else if(smr != null)
						smr.sharedMesh = composite;
				}


				Undo.DestroyObjectImmediate(polyMesh);
			}
		}

		void CreateCompositeMesh(Mesh source, Mesh mesh, Mesh composite)
		{
			int vertexCount = source.vertexCount;
			bool isNewMesh = composite.vertexCount != vertexCount;

			composite.name = source.name;

			composite.vertices = mesh.vertices != null && mesh.vertices.Length == vertexCount ?
				mesh.vertices :
				source.vertices;

			composite.normals = mesh.normals != null  && mesh.normals.Length == vertexCount ?
				mesh.normals :
				source.normals;

			composite.tangents = mesh.tangents != null && mesh.tangents.Length == vertexCount ?
				mesh.tangents :
				source.tangents;

			composite.boneWeights = mesh.boneWeights != null && mesh.boneWeights.Length == vertexCount ?
				mesh.boneWeights :
				source.boneWeights;

			composite.colors32 = mesh.colors32 != null && mesh.colors32.Length == vertexCount ?
				mesh.colors32 :
				source.colors32;

			composite.bindposes = mesh.bindposes != null && mesh.bindposes.Length == vertexCount ?
				mesh.bindposes :
				source.bindposes;

			List<Vector4> uvs = new List<Vector4>();

			mesh.GetUVs(0, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(0, uvs);
			composite.SetUVs(0, uvs);

			mesh.GetUVs(1, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(1, uvs);
			composite.SetUVs(1, uvs);

			mesh.GetUVs(2, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(2, uvs);
			composite.SetUVs(2, uvs);

			mesh.GetUVs(3, uvs);
			if(uvs == null || uvs.Count != vertexCount)
				source.GetUVs(3, uvs);
			composite.SetUVs(3, uvs);

			if(isNewMesh)
			{
				composite.subMeshCount = source.subMeshCount;

				for(int i = 0; i < source.subMeshCount; i++)
					composite.SetIndices(source.GetIndices(i), source.GetTopology(i), i);
			}
		}
	}
}
