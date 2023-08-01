using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Static helper functions for working with meshes.
    /// </summary>
    internal static class PolyMeshUtility
	{
        /// <summary>
        /// Duplicate "src" and return the copy.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        internal static Mesh DeepCopy(Mesh src)
		{
            //null checks
            if (src == null)
            {
                return null;
            }

            Mesh dst = new Mesh();
			Copy(src, dst);
			return dst;
		}

        /// <summary>
        /// Copy "src" mesh values to "dest"
        /// </summary>
        /// <param name="dest">destination</param>
        /// <param name="src">source</param>
        internal static void Copy(Mesh src, Mesh dst)
		{
            //null checks
            if(dst == null || src == null)
            {
                return;
            }

			dst.Clear();
			dst.vertices = src.vertices;

			List<Vector4> uvs = new List<Vector4>();

			src.GetUVs(0, uvs); dst.SetUVs(0, uvs);
			src.GetUVs(1, uvs); dst.SetUVs(1, uvs);
			src.GetUVs(2, uvs); dst.SetUVs(2, uvs);
			src.GetUVs(3, uvs); dst.SetUVs(3, uvs);

			dst.normals = src.normals;
			dst.tangents = src.tangents;
			dst.boneWeights = src.boneWeights;
			dst.colors = src.colors;
			dst.colors32 = src.colors32;
			dst.bindposes = src.bindposes;

			dst.subMeshCount = src.subMeshCount;
            dst.indexFormat = src.indexFormat;

			for(int i = 0; i < src.subMeshCount; i++)
				dst.SetIndices(src.GetIndices(i), src.GetTopology(i), i);

			dst.name = Util.IncrementPrefix("z", src.name);
		}

        /// <summary>
        /// Creates a new mesh using only the "src" positions, normals, and a new color array.
        /// </summary>
        /// <param name="src">source Mesh</param>
        /// <returns></returns>
        internal static Mesh CreateOverlayMesh(PolyMesh src)
		{
            //null checks
            if(src == null)
            {
                return null;
            }

			Mesh m = new Mesh();
			m.name = "Overlay Mesh: " + src.name;
			m.vertices = src.vertices;
			m.normals = src.normals;
            m.colors = Util.Fill<Color>(new Color(0f, 0f, 0f, 0f), m.vertexCount);
            m.indexFormat = src.vertexCount >= ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
			m.subMeshCount = src.subMeshCount;

			for(int i = 0; i < src.subMeshCount; i++)
			{
                SubMesh subMesh = src.subMeshes[i];
                MeshTopology subMeshTopology = subMesh.topology;
                int[] subMeshIndices = subMesh.indexes;

                if (subMeshTopology == MeshTopology.Triangles)
				{
                    int[] tris = subMeshIndices;
                    int[] lines = new int[tris.Length * 2];
					int index = 0;
					for(int n = 0; n < tris.Length; n+=3)
					{
						lines[index++] = tris[n+0];
						lines[index++] = tris[n+1];
						lines[index++] = tris[n+1];
						lines[index++] = tris[n+2];
						lines[index++] = tris[n+2];
						lines[index++] = tris[n+0];
					}
					m.SetIndices(lines, MeshTopology.Lines, i);
				}
				else
				{
					m.SetIndices(subMeshIndices, subMeshTopology, i);
				}
			}
			return m;
		}

		private static readonly Color clear = new Color(0,0f,0,0f);

        static readonly Vector2[] k_VertexBillboardUV0Content = new Vector2[]
        {
            Vector3.zero,
            Vector3.right,
            Vector3.up,
            Vector3.one
        };

        static readonly Vector2[] k_VertexBillboardUV2Content = new Vector2[]
        {
            -Vector3.up-Vector3.right,
            -Vector3.up+Vector3.right,
            Vector3.up-Vector3.right,
            Vector3.up+Vector3.right,
        };

        internal static Mesh CreateVertexBillboardMesh(PolyMesh src, int[][] common)
        {
            if (src == null || common == null)
                return null;

            int vertexCount = System.Math.Min(ushort.MaxValue / 4, common.Count());

            Vector3[] positions = new Vector3[vertexCount * 4];
            Vector2[] uv0       = new Vector2[vertexCount * 4];
            Vector2[] uv2       = new Vector2[vertexCount * 4];
            Color[] colors      = new Color[vertexCount * 4];
            int[] tris          = new int[vertexCount * 6];

            int n = 0;
            int t = 0;

            Vector3[] v = src.vertices;

            for (int i = 0; i < vertexCount; i++)
            {
                int tri = common[i][0];

                positions[t + 0] = v[tri];
                positions[t + 1] = v[tri];
                positions[t + 2] = v[tri];
                positions[t + 3] = v[tri];

                uv0[t + 0] = k_VertexBillboardUV0Content[0];
                uv0[t + 1] = k_VertexBillboardUV0Content[1];
                uv0[t + 2] = k_VertexBillboardUV0Content[2];
                uv0[t + 3] = k_VertexBillboardUV0Content[3];

                uv2[t + 0] = k_VertexBillboardUV2Content[0];
                uv2[t + 1] = k_VertexBillboardUV2Content[1];
                uv2[t + 2] = k_VertexBillboardUV2Content[2];
                uv2[t + 3] = k_VertexBillboardUV2Content[3];

                tris[n + 0] = t + 0;
                tris[n + 1] = t + 1;
                tris[n + 2] = t + 2;
                tris[n + 3] = t + 1;
                tris[n + 4] = t + 3;
                tris[n + 5] = t + 2;

                colors[t + 0] = clear;
                colors[t + 1] = clear;
                colors[t + 2] = clear;
                colors[t + 3] = clear;

                t += 4;
                n += 6;
            }

            Mesh m = new Mesh();

            m.vertices = positions;
            m.uv = uv0;
            m.uv2 = uv2;
            m.colors = colors;
            m.triangles = tris;

            return m;
        }

        /// <summary>
        /// Builds a lookup table for each vertex index and it's average normal with other vertices sharing a position.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        internal static Dictionary<int, Vector3> GetSmoothNormalLookup(PolyMesh mesh)
		{
            //null checks
            if (mesh == null)
            {
                return null;
            }

            Vector3[] n = mesh.normals;
			Dictionary<int, Vector3> normals = new Dictionary<int, Vector3>();

			if(n == null || n.Length != mesh.vertexCount)
				return normals;

			int[][] groups = GetCommonVertices(mesh);

			Vector3 avg = Vector3.zero;
			Vector3 a = Vector3.zero;
			foreach(var group in groups)
			{
				avg.x = 0f;
				avg.y = 0f;
				avg.z = 0f;

				foreach(int i in group)
				{
					a = n[i];

					avg.x += a.x;
					avg.y += a.y;
					avg.z += a.z;
				}

				avg /= (float) group.Count();

				foreach(int i in group)
					normals.Add(i, avg);
			}

			return normals;
		}

        struct CommonVertexCache
        {
            int m_Hash;
            public int[][] indices;

            public CommonVertexCache(Mesh mesh)
            {
                m_Hash = GetHash(mesh);
                Vector3[] v = mesh.vertices;
                // this is _really_ slow
                int[] t = Util.Fill<int>((x) => { return x; }, v.Length);
                indices = t.ToLookup(x => (RndVec3)v[x])
                    .Select(y => y.ToArray())
                    .ToArray();
            }

            public bool IsValidForMesh(Mesh mesh)
            {
                return m_Hash == GetHash(mesh);
            }

            static int GetHash(Mesh mesh)
            {
                unchecked
                {
                    int hash = 27 * 29 + mesh.vertexCount;
                    for(int i = 0, c = mesh.subMeshCount; i < c; i++)
                        hash = hash * 29 + (int) mesh.GetIndexCount(i);
                    return hash;
                }
            }
        }

		/// <summary>
		/// Store a temporary cache of common vertex indices.
		/// </summary>
		static Dictionary<PolyMesh, CommonVertexCache> commonVerticesCache = new Dictionary<PolyMesh, CommonVertexCache>();

        /// <summary>
        /// Builds a list<group> with each vertex index and a list of all other vertices sharing a position.
        /// </summary>
        /// <returns>
        /// key: Index in vertices array
        /// value: List of other indices in positions array that share a point with this index.
        /// </returns>
        internal static int[][] GetCommonVertices(PolyMesh mesh)
		{
            //null checks
            if (mesh == null)
                return null;

            CommonVertexCache cache;

			if(commonVerticesCache.TryGetValue(mesh, out cache))
			{
                if (cache.IsValidForMesh(mesh.mesh))
                    return cache.indices;
            }

			if(!commonVerticesCache.ContainsKey(mesh))
				commonVerticesCache.Add(mesh, cache = new CommonVertexCache(mesh.mesh));
			else
				commonVerticesCache[mesh] = cache = new CommonVertexCache(mesh.mesh);

			return cache.indices;
		}

		internal static List<CommonEdge> GetEdges(PolyMesh m)
		{
			Dictionary<int, int> lookup = GetCommonVertices(m).GetCommonLookup<int>();
			return GetEdges(m, lookup);
		}

		internal static List<CommonEdge> GetEdges(PolyMesh m, Dictionary<int, int> lookup)
		{
			int[] tris = m.GetTriangles();
			int count = tris.Length;

			List<CommonEdge> edges = new List<CommonEdge>(count);

			for(int i = 0; i < count; i += 3)
			{
				edges.Add( new CommonEdge(tris[i+0], tris[i+1], lookup[tris[i+0]], lookup[tris[i+1]]) );
				edges.Add( new CommonEdge(tris[i+1], tris[i+2], lookup[tris[i+1]], lookup[tris[i+2]]) );
				edges.Add( new CommonEdge(tris[i+2], tris[i+0], lookup[tris[i+2]], lookup[tris[i+0]]) );
			}

			return edges;
		}

		internal static HashSet<CommonEdge> GetEdgesDistinct(PolyMesh mesh, out List<CommonEdge> duplicates)
		{
            //null checks
            if (mesh == null)
            {
                duplicates = null;
                return null;
            }

            Dictionary<int, int> lookup = GetCommonVertices(mesh).GetCommonLookup<int>();
			return GetEdgesDistinct(mesh, lookup, out duplicates);
		}

		private static HashSet<CommonEdge> GetEdgesDistinct(PolyMesh m, Dictionary<int, int> lookup, out List<CommonEdge> duplicates)
		{
			int[] tris = m.GetTriangles();
			int count = tris.Length;

			HashSet<CommonEdge> edges = new HashSet<CommonEdge>();
			duplicates = new List<CommonEdge>();

			for(int i = 0; i < count; i += 3)
			{
				CommonEdge a = new CommonEdge(tris[i+0], tris[i+1], lookup[tris[i+0]], lookup[tris[i+1]]);
				CommonEdge b = new CommonEdge(tris[i+1], tris[i+2], lookup[tris[i+1]], lookup[tris[i+2]]);
				CommonEdge c = new CommonEdge(tris[i+2], tris[i+0], lookup[tris[i+2]], lookup[tris[i+0]]);

				if(!edges.Add(a))
					duplicates.Add(a);

				if(!edges.Add(b))
					duplicates.Add(b);

				if(!edges.Add(c))
					duplicates.Add(c);
			}

			return edges;
		}

        /// <summary>
        /// Returns all vertex indices that are on an open edge.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        internal static HashSet<int> GetNonManifoldIndices(PolyMesh mesh)
        {
            if (mesh == null)
                return null;

            List<CommonEdge> duplicates;
            HashSet<CommonEdge> edges = GetEdgesDistinct(mesh, out duplicates);
            edges.ExceptWith(duplicates);
            HashSet<int> hash = CommonEdge.ToHashSet(edges);
            return hash;
        }

        /// <summary>
        /// Builds a lookup with each vertex index and a list of all neighboring indices.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        internal static Dictionary<int, int[]> GetAdjacentVertices(PolyMesh mesh)
		{
            //null checks
            if (mesh == null)
            {
                return null;
            }

            int[][] common = GetCommonVertices(mesh);
			Dictionary<int, int> lookup = common.GetCommonLookup<int>();

            List<CommonEdge> edges = GetEdges(mesh, lookup);
			List<List<int>> map = new List<List<int>>();

			for(int i = 0; i < common.Count(); i++)
				map.Add(new List<int>());

			for(int i = 0; i < edges.Count; i++)
			{
				map[edges[i].cx].Add(edges[i].y);
				map[edges[i].cy].Add(edges[i].x);
			}

			Dictionary<int, int[]> adjacent = new Dictionary<int, int[]>();
			IEnumerable<int> distinctTriangles = mesh.GetTriangles().Distinct();

			foreach(int i in distinctTriangles)
				adjacent.Add(i, map[lookup[i]].ToArray());

			return adjacent;
		}

		static Dictionary<PolyMesh, Dictionary<PolyEdge, List<int>>> adjacentTrianglesCache = new Dictionary<PolyMesh, Dictionary<PolyEdge, List<int>>>();

        /// <summary>
        /// Returns a dictionary where each PolyEdge is mapped to a list of triangle indices that share that edge.
        /// To translate triangle list to vertex indices, multiply by 3 and take those indices (ex, triangles[index+{0,1,2}])
        /// </summary>
        /// <param name="mesh">mesh to use</param>
        /// <returns>see summary</returns>
        internal static Dictionary<PolyEdge, List<int>> GetAdjacentTriangles(PolyMesh mesh)
		{
            //null checks
            if (mesh == null)
            {
                return null;
            }

            int len = mesh.GetTriangles().Length;

			if(len % 3 !=0 || len / 3 == mesh.vertexCount)
				return new Dictionary<PolyEdge, List<int>>();

			Dictionary<PolyEdge, List<int>> lookup = null;

			// @todo - should add some checks to make sure triangle structure hasn't changed
			if(adjacentTrianglesCache.TryGetValue(mesh, out lookup) && lookup.Count == mesh.vertexCount)
				return lookup;

            if (adjacentTrianglesCache.ContainsKey(mesh))
                adjacentTrianglesCache.Remove(mesh);

			int subMeshCount = mesh.subMeshCount;

			lookup = new Dictionary<PolyEdge, List<int>>();
			List<int> connections;

			for(int n = 0; n < subMeshCount; n++)
			{
				int[] tris = mesh.subMeshes[n].indexes;

				for(int i = 0; i < tris.Length; i+=3)
				{
					int index = i/3;

					PolyEdge a = new PolyEdge(tris[i  ], tris[i+1]);
					PolyEdge b = new PolyEdge(tris[i+1], tris[i+2]);
					PolyEdge c = new PolyEdge(tris[i+2], tris[i  ]);

					if(lookup.TryGetValue(a, out connections))
						connections.Add(index);
					else
						lookup.Add(a, new List<int>(){index});

					if(lookup.TryGetValue(b, out connections))
						connections.Add(index);
					else
						lookup.Add(b, new List<int>(){index});

					if(lookup.TryGetValue(c, out connections))
						connections.Add(index);
					else
						lookup.Add(c, new List<int>(){index});
				}
			}

			adjacentTrianglesCache.Add(mesh, lookup);

			return lookup;
		}

		private static Dictionary<PolyMesh, int[][]> commonNormalsCache = new Dictionary<PolyMesh, int[][]>();

        /// <summary>
        /// Vertices that are common, form a seam, and should be smoothed.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        internal static int[][] GetSmoothSeamLookup(PolyMesh mesh)
		{
            //null checks
            if (mesh == null)
            {
                return null;
            }

            Vector3[] normals = mesh.normals;

			if(normals == null)
				return null;

			int[][] lookup = null;

			if(commonNormalsCache.TryGetValue(mesh, out lookup))
				return lookup;

			int[][] common = GetCommonVertices(mesh);

			var z = common
				.SelectMany(x => x.GroupBy( i => (RndVec3)normals[i] ))
					.Where(n => n.Count() > 1)
						.Select(t => t.ToArray())
							.ToArray();

			commonNormalsCache.Add(mesh, z);

			return z;
		}

        /// <summary>
        /// Recalculates a mesh's normals while retaining smoothed common vertices.
        /// </summary>
        /// <param name="mesh"></param>
        internal static void RecalculateNormals(PolyMesh mesh)
		{
            //null checks
            if (mesh == null)
                return;

            int[][] smooth = GetSmoothSeamLookup(mesh);

			mesh.RecalculateNormals();

			if(smooth != null)
			{
				Vector3[] normals = mesh.normals;

				for (int i = 0; i < smooth.Length; ++i)
				{
                    int[] l = smooth[i];
					Vector3 n = Math.Average(normals, l);

					for (int j = 0; j < l.Length; ++j)
						normals[l[j]] = n;
				}

				mesh.normals = normals;
			}
		}
	}
}
