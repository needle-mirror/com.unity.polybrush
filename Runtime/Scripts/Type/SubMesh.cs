using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Polybrush
{
    [System.Serializable]
    internal sealed class SubMesh
    {
        [SerializeField]
        int[] m_Indexes;

        [SerializeField]
        MeshTopology m_Topology;

        /// <value>
        /// Indexes making up this submesh. Can be triangles or quads, check with topology.
        /// </value>
        internal int[] indexes
        {
            get { return m_Indexes; }
            set { m_Indexes = value; }
        }

        /// <value>
        /// What is the topology (triangles, quads) of this submesh?
        /// </value>
        internal MeshTopology topology
        {
            get { return m_Topology; }
            set { m_Topology = value; }
        }

        /// <summary>
        /// Create new Submesh.
        /// </summary>
        /// <param name="submeshIndex">The index of this submesh corresponding to the MeshRenderer.sharedMaterials property.</param>
        /// <param name="topology">What topology is this submesh. Polybrush only recognizes Triangles and Quads.</param>
        /// <param name="indexes">The triangles or quads.</param>
        internal SubMesh(int submeshIndex, MeshTopology topology, IEnumerable<int> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes");

            m_Indexes = indexes.ToArray();
            m_Topology = topology;
        }

        /// <summary>
        /// Create new Submesh from a mesh, submesh index, and material.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="subMeshIndex">Which submesh to read from.</param>
        internal SubMesh(Mesh mesh, int subMeshIndex)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            m_Indexes = mesh.GetIndices(subMeshIndex);
            m_Topology = mesh.GetTopology(subMeshIndex);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", m_Topology.ToString(), m_Indexes != null ? m_Indexes.Length.ToString() : "0");
        }
    }
}