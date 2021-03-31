using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polybrush
{
    /// <summary>
    /// Workaround for bug in `MeshRenderer.additionalVertexStreams`.
    /// Namely, the mesh is not persistent in the editor and needs to be "refreshed" constantly.
    /// </summary>
	[ExecuteInEditMode]
    [Obsolete()]
	public class z_AdditionalVertexStreams : MonoBehaviour
	{
        /// <summary>
        /// The Mesh set as additional vertex stream
        /// </summary>
		public Mesh m_AdditionalVertexStreamMesh = null;

		MeshRenderer _meshRenderer;

		MeshRenderer meshRenderer
		{
			get {
				if(_meshRenderer == null)
					_meshRenderer = gameObject.GetComponent<MeshRenderer>();
				return _meshRenderer;
			}
		}

		void Start()
		{
			SetAdditionalVertexStreamsMesh(m_AdditionalVertexStreamMesh);
		}

        /// <summary>
        /// Setting the additional vertex streams
        /// </summary>
        /// <param name="mesh">The Mesh to use for additionalVertexStreams</param>
		public void SetAdditionalVertexStreamsMesh(Mesh mesh)
		{
            if (meshRenderer != null)
            {
                this.m_AdditionalVertexStreamMesh = mesh;
                meshRenderer.additionalVertexStreams = mesh;
            }
		}

#if UNITY_EDITOR
		void Update()
		{
			if(meshRenderer == null || m_AdditionalVertexStreamMesh == null || EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			meshRenderer.additionalVertexStreams = m_AdditionalVertexStreamMesh;
		}
#endif
	}
}
