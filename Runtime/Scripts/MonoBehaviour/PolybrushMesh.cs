namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Script that will be attached at all time on any polybrush object
    /// It will be used to store a copy of the mesh inside a polymesh object (that is serializable).
    /// It will also handle the advs switch mode (on/off)
    /// </summary>
    [ExecuteInEditMode]
    internal class PolybrushMesh : MonoBehaviour
    {
        //seriazlied polymesh stored on the component
        [SerializeField]
        private PolyMesh m_PolyMesh;

        //mesh ref from the skinmesh asset, if any
        [SerializeField]
        private Mesh m_SkinMeshRef;

        //Mesh renderer component cache
        MeshRenderer m_MeshRenderer;

        //Skinmesh renderer component cache
        SkinnedMeshRenderer m_SkinMeshRenderer;

        //Mesh filter component cache
        MeshFilter m_MeshFilter;

        /// <summary>
        /// Returns true if there are applied modification on this mesh.
        /// </summary>
        internal bool hasAppliedChanges
        {
            get
            {
                if (meshFilter)
                    return m_PolyMesh.GetMeshAsUnityRepresentation() == meshFilter.sharedMesh;
                if (skinMeshRenderer)
                    return m_PolyMesh.GetMeshAsUnityRepresentation() == skinMeshRenderer.sharedMesh;
                return false;
            }
        }

        //access to the polymesh directly
        internal PolyMesh polyMesh
        {
            get
            {
                return m_PolyMesh;
            }
        }

        //get: will convert the data stored inside polymesh to a mesh, return it
        internal Mesh storedMesh
        {
            get
            {
                if (m_PolyMesh != null)
                {
                    return m_PolyMesh.GetMeshAsUnityRepresentation();
                }
                else
                {
                    return null;
                }
            }
        }

        //because this script can't access to all editor stuff (due to assembly structure), there is no way it can retrieve the advs state by itself
        //that's why I've put a static here that will be filled when changing the setting
        internal static bool s_UseADVS { private get; set; }

        internal MeshRenderer meshRenderer
        {
            get
            {
                if (m_MeshRenderer == null)
                {
                    m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
                }

                return m_MeshRenderer;
            }
        }

        internal SkinnedMeshRenderer skinMeshRenderer
        {
            get
            {
                if (m_SkinMeshRenderer == null)
                {
                    m_SkinMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
                }

                return m_SkinMeshRenderer;
            }
        }
        
        internal Mesh skinMeshRef
        {
            get
            {
                return m_SkinMeshRef;
            }
            set
            {
                m_SkinMeshRef = value;
            }
        }

        internal MeshFilter meshFilter
        {
            get
            {
                if (m_MeshFilter == null)
                    m_MeshFilter = gameObject.GetComponent<MeshFilter>();

                return m_MeshFilter;
            }
        }

        /// <summary>
        /// Update Polymesh data with the given Unity Mesh information.
        /// </summary>
        /// <param name="unityMesh">Unity mesh.</param>
        internal void SetMesh(Mesh unityMesh)
        {
            if (m_PolyMesh == null)
            {
                m_PolyMesh = new PolyMesh();
            }

            m_PolyMesh.SetUnityMesh(unityMesh);
        }

        public void UpdateMesh()
        {
            if (m_PolyMesh == null)
                return;

            m_PolyMesh.UpdateMeshFromData();

            if(meshFilter != null)
                meshFilter.sharedMesh = m_PolyMesh.GetMeshAsUnityRepresentation();

            if(skinMeshRenderer != null && skinMeshRef != null)
                UpdateSkinMesh();

            //set advs depending on settings value (external assignment)
            if(meshRenderer != null)
                meshRenderer.additionalVertexStreams = s_UseADVS ? m_PolyMesh.GetMeshAsUnityRepresentation() : null;
        }

        /// <summary>
        /// Takes the mesh from the skinmesh in the asset (saved by EditableObject during creation) and apply skinmesh information to the regular stored mesh.
        /// Then set it to the skinmeshrenderer.
        /// </summary>
        void UpdateSkinMesh()
        {
            Mesh mesh = skinMeshRef;

            m_PolyMesh.UpdateMeshFromData();
            Mesh polyMesh = m_PolyMesh.GetMeshAsUnityRepresentation();
            polyMesh.boneWeights = mesh.boneWeights;
            polyMesh.bindposes = mesh.bindposes;
            skinMeshRenderer.sharedMesh = polyMesh;
        }

        private void OnEnable()
        {
            m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
            m_SkinMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            m_MeshFilter = gameObject.GetComponent<MeshFilter>();

            UpdateMesh();
        }

        void OnDestroy()
        {
            //when destroying the component, remove the advs from the mesh renderer
            if (meshRenderer != null)
                meshRenderer.additionalVertexStreams = null;
        }
    }
}
