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
                if (m_SubMeshes == null)
                    return 0;

				return m_SubMeshes.Length;
			}
		}
        
        internal PolyMesh()
        {
            uv0 = new List<Vector4>();
            uv1 = new List<Vector4>();
            uv2 = new List<Vector4>();
            uv3 = new List<Vector4>();
        }

        /// <summary>
        /// Initializes PolyMesh with data coming from an Unity Mesh.
        /// </summary>
        /// <param name="mesh"></param>
        internal void InitializeWithUnityMesh(Mesh mesh)
        {
            m_Mesh = mesh;

            name = mesh.name;

            ApplyAttributesFromUnityMesh(mesh, MeshChannel.All);
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

        /// <summary>
        /// Initializes submeshes based on mesh's submeshes.
        /// </summary>
        /// <param name="mesh"></param>
        internal void SetSubMeshes(Mesh mesh)
        {
            m_SubMeshes = new SubMesh[mesh.subMeshCount];

            for (int i = 0; i < m_SubMeshes.Length; ++i)
                m_SubMeshes[i] = new SubMesh(mesh, i);
        }

        /// <summary>
        /// Refreshes current triangles list based on current submeshes.
        /// </summary>
        void RefreshTriangles()
        {
            m_Triangles = m_SubMeshes.SelectMany(x => x.indexes).ToArray();
        }

        static Vector3[] s_PerTriangleNormalsBuffer = new Vector3[4096];
        static int[] s_PerTriangleAvgBuffer = new int[4096];

        /// <summary>
        /// Recalculates the normals of the Mesh from the triangles and vertices.
        /// </summary>
        internal void RecalculateNormals()
        {
            if (s_PerTriangleNormalsBuffer.Length < vertexCount)
            {
                Array.Resize<Vector3>(ref s_PerTriangleNormalsBuffer, vertexCount);
                Array.Resize<int>(ref s_PerTriangleAvgBuffer, vertexCount);
            }                

            for (int i = 0; i < vertexCount; ++i)
            {
                s_PerTriangleNormalsBuffer[i].x = 0;
                s_PerTriangleNormalsBuffer[i].y = 0;
                s_PerTriangleNormalsBuffer[i].z = 0;
                s_PerTriangleAvgBuffer[i] = 0;
            }

            int[] tris = GetTriangles();
            
            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];

                Vector3 cross = Math.Normal(vertices[a], vertices[b], vertices[c]);

                s_PerTriangleNormalsBuffer[a].x += cross.x;
                s_PerTriangleNormalsBuffer[b].x += cross.x;
                s_PerTriangleNormalsBuffer[c].x += cross.x;

                s_PerTriangleNormalsBuffer[a].y += cross.y;
                s_PerTriangleNormalsBuffer[b].y += cross.y;
                s_PerTriangleNormalsBuffer[c].y += cross.y;

                s_PerTriangleNormalsBuffer[a].z += cross.z;
                s_PerTriangleNormalsBuffer[b].z += cross.z;
                s_PerTriangleNormalsBuffer[c].z += cross.z;

                s_PerTriangleAvgBuffer[a]++;
                s_PerTriangleAvgBuffer[b]++;
                s_PerTriangleAvgBuffer[c]++;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                normals[i].x = s_PerTriangleNormalsBuffer[i].x * (float)s_PerTriangleAvgBuffer[i];
                normals[i].y = s_PerTriangleNormalsBuffer[i].y * (float)s_PerTriangleAvgBuffer[i];
                normals[i].z = s_PerTriangleNormalsBuffer[i].z * (float)s_PerTriangleAvgBuffer[i];
                Math.Divide(normals[i], normals[i].magnitude, ref normals[i]);
            }
        }


        /// <summary>
        /// Apply the vertex attributes to a UnityEngine mesh (does not set triangles)
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="attrib"></param>
        internal void ApplyAttributesToUnityMesh(Mesh mesh, MeshChannel attrib = MeshChannel.All)
		{
			// I guess the default value for attrib makes the compiler think that else is never
			// activated?
#pragma warning disable 0162
			if(attrib == MeshChannel.All)
			{
				mesh.vertices = vertices;
				mesh.normals = normals;
				mesh.colors32 = colors;
				mesh.tangents = tangents;

				mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
				mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
				mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
				mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);

                mesh.subMeshCount = m_SubMeshes.Length;

                for(int i = 0; i < subMeshCount; ++i)
                    mesh.SetIndices(m_SubMeshes[i].indexes, m_SubMeshes[i].topology, i);

                RefreshTriangles();
			}
			else
			{
				if((attrib & MeshChannel.Position) > 0) mesh.vertices = vertices;
				if((attrib & MeshChannel.Normal) > 0) mesh.normals = normals;
				if((attrib & MeshChannel.Color) > 0) mesh.colors32 = colors;
				if((attrib & MeshChannel.Tangent) > 0) mesh.tangents = tangents;
				if((attrib & MeshChannel.UV0) > 0) mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
				if((attrib & MeshChannel.UV2) > 0) mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
				if((attrib & MeshChannel.UV3) > 0) mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
				if((attrib & MeshChannel.UV4) > 0) mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);
			}
#pragma warning restore 0162
		}

        internal void ApplyAttributesFromUnityMesh(Mesh mesh, MeshChannel attrib = MeshChannel.All)
        {
#pragma warning disable 0162
            if (attrib == MeshChannel.All)
            {
                vertices = mesh.vertices;
                normals = mesh.normals;
                colors = mesh.colors32;
                tangents = mesh.tangents;

                mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
                mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
                mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
                mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);

                /// Update submeshes only if there were none set or if we are no
                /// about to change the submeshCount.
                if (subMeshCount == 0 || mesh.subMeshCount == subMeshCount)
                {
                    SetSubMeshes(mesh);
                    RefreshTriangles();
                }
            }
            else
            {
                if ((attrib & MeshChannel.Position) > 0) vertices = mesh.vertices;
                if ((attrib & MeshChannel.Normal) > 0) normals = mesh.normals;
                if ((attrib & MeshChannel.Color) > 0) colors = mesh.colors32;
                if ((attrib & MeshChannel.Tangent) > 0) tangents = mesh.tangents;
                if ((attrib & MeshChannel.UV0) > 0) mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
                if ((attrib & MeshChannel.UV2) > 0) mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
                if ((attrib & MeshChannel.UV3) > 0) mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
                if ((attrib & MeshChannel.UV4) > 0) mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);
            }
#pragma warning restore 0162
        }

        /// <summary>
        /// Get the mesh stored and update local values
        /// If doesn't exist, create a new mesh from the data stored and return it
        /// </summary>
        /// <returns></returns>
        internal Mesh ToUnityMesh()
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
        internal void UpdateMeshFromData()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                m_Mesh.name = name;
            }

            ApplyAttributesToUnityMesh(m_Mesh);
        }

        /// <summary>
        /// Returns true if PolyMesh has valid data.
        /// </summary>
        /// <returns></returns>
        internal bool IsValid()
        {
            if (vertexCount == 0)
                return false;

            if (m_Triangles.Length == 0)
                return false;

            return true;
        }
	}
}
