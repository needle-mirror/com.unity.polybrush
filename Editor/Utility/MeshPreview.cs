#define PROBUILDER_4_0_OR_NEWER

using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Handle mesh preview. Use it to load a static preview from a native Unity Mesh,
    /// PolybrushMesh or ProBuilderMesh.
    /// </summary>
    public class MeshPreview
    {
        /// <summary>
        /// Preview default size.
        /// </summary>
        static readonly Vector2Int k_PreviewSize = new Vector2Int(128, 128);

        internal enum State
        {
            Loading,
            Loaded,
            Cannot_Load
        }

        /// <summary>
        /// Returns the loaded preview texture.
        /// If there's none, returns a white texture as fallback.
        /// </summary>
        internal Texture2D previewTexture
        {
            get
            {
                if (m_previewState == State.Loaded && m_PreviewTexture != null)
                    return m_PreviewTexture;

                return Texture2D.whiteTexture;
            }
        }

        /// <summary>
        /// Current preview state.
        /// </summary>
        internal State CurrentPreviewState
        {
            get { return m_previewState; }
        }

        /// <summary>
        /// Instance current state.
        /// </summary>
        State m_previewState = State.Loading;

        /// <summary>
        /// Loaded preview texture.
        /// </summary>
        Texture2D m_PreviewTexture;

        /// <summary>
        /// Reference to the asset associated to this preview.
        /// </summary>
        Object m_Asset = null;

        internal MeshPreview(Object asset)
        {
            m_Asset = asset;
            m_PreviewTexture = Texture2D.whiteTexture;
            SetState(State.Loading);
        }

        /// <summary>
        /// Update the current state of the mesh preview.
        /// As AssetPreview API is asynchronous, we need to keep requesting the asset preview
        /// until we have it. Once texture is loaded, current state of the instance moves
        /// to Status.Loaded.
        /// </summary>
        internal void UpdatePreview()
        {
            switch (m_previewState)
            {
                case State.Loading:
                    if (ProBuilderBridge.ProBuilderExists() && ProBuilderInterface.IsProBuilderObject(m_Asset as GameObject))
                        m_PreviewTexture = GenerateProBuilderPreview();
                    else if (PolyEditorUtility.IsPolybrushObject(m_Asset as GameObject))
                        m_PreviewTexture = GeneratePreview();
                    else
                        m_PreviewTexture = AssetPreview.GetAssetPreview(m_Asset);

                    if (previewTexture != null)
                        SetState(State.Loaded);

                    break;
                case State.Loaded:
                    // Failsafe as AssetPreview can return white textures while loading
                    // and erase that texture from memory once the right texture is available.
                    if (m_PreviewTexture == null)
                        SetState(State.Loading);
                    break;
            }
        }

        /// <summary>
        /// Generate a static preview for a PolybrushMesh.
        /// A mesh is generated from a PolybrushMesh then feed to a default Mesh editor.
        /// </summary>
        /// <returns>Static preview of the given PolybrushMesh.</returns>
        internal Texture2D GeneratePreview()
        {
            PolybrushMesh castTarget = (m_Asset as GameObject).GetComponent<PolybrushMesh>();
            Mesh mesh = castTarget.storedMesh;

            Editor meshEditor = Editor.CreateEditor(mesh);

            Texture2D preview = meshEditor.RenderStaticPreview(null, null, k_PreviewSize.x, k_PreviewSize.y);

            ScriptableObject.DestroyImmediate(meshEditor);

            return preview;
        }

        /// <summary>
        /// Generate a static preview for a ProBuilderMesh.
        /// </summary>
        /// <returns>Static preview of the given ProBuilderMesh.</returns>
        internal Texture2D GenerateProBuilderPreview()
        {
            GameObject copy = GameObject.Instantiate<GameObject>(m_Asset as GameObject);
            copy.hideFlags = HideFlags.HideAndDontSave;

#if PROBUILDER_4_0_OR_NEWER
            ProBuilderBridge.ToMesh(copy);
            ProBuilderBridge.Refresh(copy);
#endif
            
            MeshFilter mf = copy.GetComponent<MeshFilter>();

            Editor meshEditor = Editor.CreateEditor(mf.sharedMesh);

            Texture2D preview = meshEditor.RenderStaticPreview(null, null, k_PreviewSize.x, k_PreviewSize.y);

            ScriptableObject.DestroyImmediate(meshEditor);
            ScriptableObject.DestroyImmediate(copy);

            return preview;
        }

        private void SetState(State newState)
        {
            m_previewState = newState;
        }
    }
}
