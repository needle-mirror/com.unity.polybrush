using System.Linq;
using System.Collections.Generic;
using System;

namespace UnityEngine.Polybrush
{

    /// <summary>
    /// Caches the attributes of UnityEngine.Mesh class that Polybrush can edit.
    /// 
    /// Necessary because accessing attributes on UnityEngine.Mesh always invokes
    /// an expensive C# <-> C++ trip, plus it copies data, which 99% of the time
    /// we don't need.
    /// </summary>
    [Serializable]
	internal class PolyMesh
	{
        [SerializeField]
		internal string name = "";
        [SerializeField]
        internal Vector3[] vertices = null;
        [SerializeField]
        internal Vector3[] normals = null;
        [SerializeField]
        internal Color32[] colors = null;
        [SerializeField]
        internal Vector4[] tangents = null;
        [SerializeField]
        internal List<Vector4> uv0 = null;
        [SerializeField]
        internal List<Vector4> uv1 = null;
        [SerializeField]
        internal List<Vector4> uv2 = null;
        [SerializeField]
        internal List<Vector4> uv3 = null;

        [SerializeField]
		int[] m_Triangles = null;
        [SerializeField]
        SubMesh[] m_SubMeshes = null;
        [SerializeField]
        Mesh m_Mesh = null;

        internal int vertexCount { get { return vertices != null ? vertices.Length : 0; } }

        internal SubMesh[] subMeshes
        {
            get { return m_SubMeshes; }
        }

		internal int subMeshCount
		{
			get
			{
				return m_SubMeshes.Length;
			}
		}
        
        internal PolyMesh()
        {

        }

        internal PolyMesh(Mesh mesh)
        {
            SetUnityMesh(mesh);
        }

        internal void SetUnityMesh(Mesh mesh)
        {
            m_Mesh = mesh;

            name = mesh.name;

            vertices = mesh.vertices;
            normals = mesh.normals;
            colors = mesh.colors32;
            tangents = mesh.tangents;

            SetUVs(0, uv0);
            SetUVs(1, uv1);
            SetUVs(2, uv2);
            SetUVs(3, uv3);

            SetSubMeshes(mesh);
        }
        /// <summary>
        /// Gets the UVs of the Mesh.
        /// </summary>
        /// <param name="channel">The UV channel. Indices start at 0, which corresponds to uv. Note that 1 corresponds to uv2.</param>
        /// <returns>Gets the Mesh's UVs (texture coordinates) as a List of either Vector2, Vector3, or Vector4.</returns>
		internal List<Vector4> GetUVs(int channel)
		{
			if(channel == 0) return uv0;
			else if(channel == 1) return uv1;
			else if(channel == 2) return uv2;
			else if(channel == 3) return uv3;
			return null;
		}

        /// <summary>
        /// Sets the UVs of the Mesh.
        /// </summary>
        /// <param name="channel">The UV channel. Indices start at 0, which corresponds to uv. Note that 1 corresponds to uv2</param>
        /// <param name="uvs">List of UVs to set for the given index.</param>
		internal void SetUVs(int channel, List<Vector4> uvs)
		{
			if(channel == 0) uv0 = uvs;
			else if(channel == 1) uv1 = uvs;
			else if(channel == 2) uv2 = uvs;
			else if(channel == 3) uv3 = uvs;
		}

        /// <summary>
        /// Clears all vertex data and all triangle indices.
        /// </summary>
		internal void Clear()
		{
			vertices = null;
			normals = null;
			colors 	= null;
			tangents = null;
			uv0 = null;
			uv1 = null;
			uv2 = null;
			uv3 = null;
            m_SubMeshes = null;
		}

        /// <summary>
        /// Fetches the triangle list of this object.
        /// Each integer in the returned triangle list is a vertex index, which is used as an offset into the Mesh's vertex arrays.
        /// The triangle list contains a multiple of three indices, one for each corner in each triangle.
        /// </summary>
        /// <returns>a list of triangles</returns>
		internal int[] GetTriangles()
		{
            if (m_Triangles == null)
                RefreshTriangles();

            return m_Triangles;
		}

        internal void SetSubMeshes(Mesh mesh)
        {
            m_SubMeshes = new SubMesh[mesh.subMeshCount];

            for (int i = 0; i < m_SubMeshes.Length; ++i)
                m_SubMeshes[i] = new SubMesh(mesh, i);
        }

        void RefreshTriangles()
        {
            this.m_Triangles = m_SubMeshes.SelectMany(x => x.indexes).ToArray();
        }
        
		/// <summary>
        /// Recalculates the normals of the Mesh from the triangles and vertices.
        /// </summary>
		internal void RecalculateNormals()
		{
			Vector3[] perTriangleNormal = new Vector3[vertexCount];
			int[] perTriangleAvg = new int[vertexCount];
			int[] tris = GetTriangles();

			for(int i = 0; i < tris.Length; i += 3)
			{
				int a = tris[i], b = tris[i + 1], c = tris[i + 2];

				Vector3 cross = PolyMath.Normal(vertices[a], vertices[b], vertices[c]);

				perTriangleNormal[a].x += cross.x;
				perTriangleNormal[b].x += cross.x;
				perTriangleNormal[c].x += cross.x;

				perTriangleNormal[a].y += cross.y;
				perTriangleNormal[b].y += cross.y;
				perTriangleNormal[c].y += cross.y;

				perTriangleNormal[a].z += cross.z;
				perTriangleNormal[b].z += cross.z;
				perTriangleNormal[c].z += cross.z;

				perTriangleAvg[a]++;
				perTriangleAvg[b]++;
				perTriangleAvg[c]++;
			}


			for(int i = 0; i < vertexCount; i++)
			{
				normals[i].x = perTriangleNormal[i].x * (float) perTriangleAvg[i];
				normals[i].y = perTriangleNormal[i].y * (float) perTriangleAvg[i];
				normals[i].z = perTriangleNormal[i].z * (float) perTriangleAvg[i];
                normals[i].Normalize();
			}
		}

        /// <summary>
        /// Apply the vertex attributes to a UnityEngine mesh (does not set triangles)
        /// </summary>
        /// <param name="m"></param>
        /// <param name="attrib"></param>
        internal void ApplyAttributesToUnityMesh(Mesh m, MeshChannel attrib = MeshChannel.All)
		{
			// I guess the default value for attrib makes the compiler think that else is never
			// activated?
#pragma warning disable 0162
			if(attrib == MeshChannel.All)
			{
				m.vertices = vertices;
				m.normals = normals;
				m.colors32 = colors;
				m.tangents = tangents;

				m.SetUVs(0, uv0);
				m.SetUVs(1, uv1);
				m.SetUVs(2, uv2);
				m.SetUVs(3, uv3);

                m.subMeshCount = m_SubMeshes.Length;

                for(int i = 0; i < subMeshCount; ++i)
                    m.SetIndices(m_SubMeshes[i].indexes, m_SubMeshes[i].topology, i);
			}
			else
			{
				if((attrib & MeshChannel.Position) > 0) m.vertices = vertices;
				if((attrib & MeshChannel.Normal) > 0) m.normals = normals;
				if((attrib & MeshChannel.Color) > 0) m.colors32 = colors;
				if((attrib & MeshChannel.Tangent) > 0) m.tangents = tangents;
				if((attrib & MeshChannel.UV0) > 0) m.SetUVs(0, uv0);
				if((attrib & MeshChannel.UV2) > 0) m.SetUVs(1, uv1);
				if((attrib & MeshChannel.UV3) > 0) m.SetUVs(2, uv2);
				if((attrib & MeshChannel.UV4) > 0) m.SetUVs(3, uv3);
			}
#pragma warning restore 0162
		}

        /// <summary>
        /// Get the mesh stored and update local values
        /// If doesn't exist, create a new mesh from the data stored and return it
        /// </summary>
        /// <returns></returns>
        public Mesh GetMeshAsUnityRepresentation()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                m_Mesh.name = name;

                UpdateMeshFromData();
            }

            return m_Mesh;
        }

        /// <summary>
        /// Will update the stored mesh with serialized data
        /// </summary>
        public void UpdateMeshFromData()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                m_Mesh.name = name;
            }


            ApplyAttributesToUnityMesh(m_Mesh);
        }
	}
}
