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
        internal static class Styles
        {
            internal const string k_VertexMismatchStringFormat = "Warning! The GameObject \"{0}\" cannot apply it's 'Additional Vertex Streams' mesh, because it's base mesh has changed and has a different vertex count.";
        }

        /// <summary>
        /// References components needed by Polybrush: MeshFilter, MeshRenderer and SkinnedMeshRenderer.
        /// Will cache references if they are found to avoid additional GetComponent() calls.
        /// </summary>
        internal struct MeshComponentsCache
        {
            GameObject          m_Owner;
            MeshFilter          m_MeshFilter;
            MeshRenderer        m_MeshRenderer;
            SkinnedMeshRenderer m_SkinMeshRenderer;

            internal bool IsValid()
            {
                return m_Owner != null;
            }

            internal MeshFilter MeshFilter
            {
                get
                {
                    return m_MeshFilter;
                }
            }

            internal MeshRenderer MeshRenderer
            {
                get
                {
                    return m_MeshRenderer;
                }
            }

            internal SkinnedMeshRenderer SkinnedMeshRenderer
            {
                get
                {
                    return m_SkinMeshRenderer;
                }
            }

            internal MeshComponentsCache(GameObject root)
            {
                m_Owner = root;
                m_MeshFilter = root.GetComponent<MeshFilter>();
                m_MeshRenderer = root.GetComponent<MeshRenderer>();
                m_SkinMeshRenderer = root.GetComponent<SkinnedMeshRenderer>();
            }
        }

        //seriazlied polymesh stored on the component
        [SerializeField]
        private PolyMesh m_PolyMesh;

        //mesh ref from the skinmesh asset, if any
        [SerializeField]
        private Mesh m_SkinMeshRef;

        MeshComponentsCache m_ComponentsCache;

        /// <summary>
        /// Accessor to Unity rendering related components attached to the same GameObject as this component.
        /// </summary>
        internal MeshComponentsCache componentsCache
        {
            get { return m_ComponentsCache; }
        }

        /// <summary>
        /// Returns true if there are applied modification on this mesh.
        /// </summary>
        internal bool hasAppliedChanges
        {
            get
            {
                if (m_ComponentsCache.MeshFilter)
                    return m_PolyMesh.ToUnityMesh() != m_ComponentsCache.MeshFilter.sharedMesh;
                if (m_ComponentsCache.SkinnedMeshRenderer)
                    return m_PolyMesh.ToUnityMesh() != m_ComponentsCache.SkinnedMeshRenderer.sharedMesh;
                return false;
            }
        }

        internal bool hasAppliedAdditionalVertexStreams
        {
            get
            {
                if (m_ComponentsCache.MeshRenderer != null && m_ComponentsCache.MeshRenderer.additionalVertexStreams != null)
                    return m_ComponentsCache.MeshRenderer.additionalVertexStreams == m_PolyMesh.ToUnityMesh();
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
                    return m_PolyMesh.ToUnityMesh();
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

        bool m_Initialized = false;

        /// <summary>
        /// Returns true if internal data has been initialized.
        /// If returns false, use <see cref="Initialize"/>.
        /// </summary>
        internal bool isInitialized
        {
            get { return m_Initialized; }
        }

        /// <summary>
        /// Initializes non-serialized internal cache in the component.
        /// Should be called after instantiation.
        /// Use <see cref="isInitialized"/> to check if it has already been initialized.
        /// </summary>
        internal void Initialize()
        {
            if (isInitialized)
                return;

            if (!m_ComponentsCache.IsValid())
                m_ComponentsCache = new MeshComponentsCache(gameObject);

            if (m_PolyMesh == null)
                m_PolyMesh = new PolyMesh();

            Mesh mesh = null;

            if (m_ComponentsCache.MeshFilter != null)
                mesh = m_ComponentsCache.MeshFilter.sharedMesh;
            else if (m_ComponentsCache.SkinnedMeshRenderer != null)
                mesh = m_ComponentsCache.SkinnedMeshRenderer.sharedMesh;

            if (!polyMesh.IsValid() && mesh)
                SetMesh(mesh);

            m_Initialized = true;
        }

        /// <summary>
        /// Update Polymesh data with the given Unity Mesh information.
        /// </summary>
        /// <param name="unityMesh">Unity mesh.</param>
        internal void SetMesh(Mesh unityMesh)
        {
            m_PolyMesh.InitializeWithUnityMesh(unityMesh);
        }

        internal void SetAdditionalVertexStreams(Mesh vertexStreams)
        {
            m_PolyMesh.ApplyAttributesFromUnityMesh(vertexStreams, MeshChannelUtility.ToMask(vertexStreams));
            SynchronizeWithMeshRenderer();
        }

        /// <summary>
        /// Update GameObject's renderers with PolybrushMesh data.
        /// </summary>
        public void SynchronizeWithMeshRenderer()
        {
            if (m_PolyMesh == null)
                return;

            m_PolyMesh.UpdateMeshFromData();

            if (m_ComponentsCache.SkinnedMeshRenderer != null && skinMeshRef != null)
                UpdateSkinMesh();

            if (!s_UseADVS)
            {
                if (m_ComponentsCache.MeshFilter != null)
                    m_ComponentsCache.MeshFilter.sharedMesh = m_PolyMesh.ToUnityMesh();
                SetAdditionalVertexStreamsOnRenderer(null);
            }
            else
            {
                if (!CanApplyAdditionalVertexStreams())
                {
                    if (hasAppliedAdditionalVertexStreams)
                        RemoveAdditionalVertexStreams();

                    Debug.LogWarning(string.Format(Styles.k_VertexMismatchStringFormat, gameObject.name), this);
                    return;
                }

                SetAdditionalVertexStreamsOnRenderer(m_PolyMesh.ToUnityMesh());
            }
        }

        /// <summary>
        /// Checks if all conditions are met to apply Additional Vertex Streams.
        /// </summary>
        /// <returns></returns>
        internal bool CanApplyAdditionalVertexStreams()
        {
            // If "Use additional vertex streams is enabled in Preferences."
            if (s_UseADVS)
            {
                if (m_ComponentsCache.MeshFilter != null && m_ComponentsCache.MeshFilter.sharedMesh != null)
                {
                    if (m_ComponentsCache.MeshFilter.sharedMesh.vertexCount != polyMesh.vertexCount)
                        return false;
                }
            }
            return true;
        }
        

        /// <summary>
        /// Takes the mesh from the skinmesh in the asset (saved by EditableObject during creation) and apply skinmesh information to the regular stored mesh.
        /// Then set it to the skinmeshrenderer.
        /// </summary>
        void UpdateSkinMesh()
        {
            Mesh mesh = skinMeshRef;

            Mesh generatedMesh = m_PolyMesh.ToUnityMesh();
            generatedMesh.boneWeights = mesh.boneWeights;
            generatedMesh.bindposes = mesh.bindposes;
            m_ComponentsCache.SkinnedMeshRenderer.sharedMesh = generatedMesh;
        }

        /// <summary>
        /// Set the Additional Vertex Streams on the current MeshRenderer.
        /// </summary>
        /// <param name="mesh"></param>
        void SetAdditionalVertexStreamsOnRenderer(Mesh mesh)
        {
            if (m_ComponentsCache.MeshRenderer != null)
                m_ComponentsCache.MeshRenderer.additionalVertexStreams = mesh;
        }

        internal void RemoveAdditionalVertexStreams()
        {
            SetAdditionalVertexStreamsOnRenderer(null);
        }

        void OnEnable()
        {
            m_Initialized = false;
            
            if (!isInitialized)
                Initialize();

            if (m_PolyMesh != null && m_PolyMesh.IsValid())
                SynchronizeWithMeshRenderer();
        }

        void OnDestroy()
        {
            //when destroying the component, remove the advs from the mesh renderer
            SetAdditionalVertexStreamsOnRenderer(null);
        }
    }
}
