using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;
using Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Stores a cache of the unmodified mesh and meshrenderer
    /// so that the PolyEditor can work non-destructively.  Also
    /// handles ProBuilder compatibility so that brush modes don't
    /// have to deal with it.
    /// </summary>
    class EditableObject : IEquatable<EditableObject>, IValid
	{
		internal const string k_MeshInstancePrefix = "PolybrushMesh";

        /// <summary>
        /// Set true to rebuild mesh normals when sculpting the object.
        /// </summary>
        [UserSetting("General Settings", "Rebuild Normals", "After a mesh modification the normals will be recalculated.")]
        internal static Pref<bool> s_RebuildNormals = new Pref<bool>("Mesh.RebuildNormals", true, SettingsScope.Project);

        /// <summary>
        /// Set true to rebuild collider when sculpting the object.
        /// </summary>
        /// <remarks>Only works if gameobject has a MeshCollider component.</remarks>
        [UserSetting("General Settings", "Rebuild MeshCollider", "After a mesh modification the mesh collider will be recalculated.")]
        internal static Pref<bool> s_RebuildCollisions = new Pref<bool>("Mesh.RebuildColliders", true, SettingsScope.Project);

        /// <summary>
        /// Set true to use additional vertex stream when applying brush modification.
        /// Data will be stored in a PolybrushMesh component.
        /// </summary>
        [UserSetting("General Settings", "Use Additional Vertex Streams", "Instead of applying changes directly to the mesh, modifications will be stored in an additionalVertexStreams mesh.  This option can be more performance friendly in some cases.")]
        internal static Pref<bool> s_UseAdditionalVertexStreams = new Pref<bool>("Mesh.UseAdditionalVertexStream", false, SettingsScope.Project);


        private static HashSet<string> UnityPrimitiveMeshNames = new HashSet<string>()
		{
			"Sphere",
			"Capsule",
			"Cylinder",
			"Cube",
			"Plane",
			"Quad"
		};

		// The GameObject being modified.
		internal GameObject gameObjectAttached = null;

		// The mesh that is
		private Mesh _graphicsMesh = null;

		internal Mesh graphicsMesh { get { return _graphicsMesh; } }

		[System.Obsolete("Use graphicsMesh or editMesh instead")]
		internal Mesh mesh { get { return _graphicsMesh; } }

		private PolyMesh _editMesh = null;
        private PolyMesh _skinMeshBaked;
		internal PolyMesh editMesh
        {
            get
            {
                return _editMesh;
            }
        }

        //used only for brush position (raycasting)
        //if it's editing a normal mesh, returns the editMesh (standard behavior).
        //if editing a skinmesh, returns the BakeMesh (a mesh without any skin information but with vertex set to correct positions)
        //it's not intended to edit this mesh, it's used only in read-only (no setter)
        internal PolyMesh visualMesh
        {
            get
            {
                if (_skinMeshRenderer != null)
                {
                    Mesh mesh = new Mesh();
                    _skinMeshRenderer.BakeMesh(mesh);

                    if (_skinMeshBaked == null)
                        _skinMeshBaked = new PolyMesh();

                    _skinMeshBaked.InitializeWithUnityMesh(mesh);

                    return _skinMeshBaked;
                }
                else
                {
                    return _editMesh;
                }
            }
        }

        private SkinnedMeshRenderer _skinMeshRenderer;

		// The original mesh.  Can be the same as mesh.
		internal Mesh originalMesh { get; private set; }

        internal MeshChannel modifiedChannels = MeshChannel.Null;

        // Where this mesh originated.
        internal ModelSource source { get; private set; }

		// If mesh was an asset or model, save the original GUID
		// internal string sourceGUID { get; private set; }

		// Marks this object as having been modified.
		internal bool modified = false;

		private T GetAttribute<T>(System.Func<Mesh, T> getter) where T : IList
		{
			if (m_PolybrushMesh.mode == PolybrushMesh.Mode.AdditionalVertexStream)
			{
				int vertexCount = originalMesh.vertexCount;
				T arr = getter(this.graphicsMesh);
				if(arr != null && arr.Count == vertexCount)
					return arr;
			}
			return getter(originalMesh);
		}

        /// <summary>
        /// Return a mesh that is the combination of both additionalVertexStreams and the originalMesh.
        /// 	- Position
        /// 	- UV0
        /// 	- UV2
        /// 	- UV3
        /// 	- UV4
        /// 	- Color
        /// 	- Tangent
        /// </summary>
        /// <returns>The new PolyMesh object</returns>
        private void GenerateCompositeMesh()
		{
			if(_editMesh == null)
				_editMesh = polybrushMesh.polyMesh;

			_editMesh.Clear();
			_editMesh.name = originalMesh.name;
			_editMesh.vertices	= GetAttribute(x => x.vertices);
			_editMesh.normals	= GetAttribute(x => x.normals);
			_editMesh.colors 	= GetAttribute(x => x.colors);
			_editMesh.tangents	= GetAttribute(x => x.tangents);
			_editMesh.uv0 = GetAttribute(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(0, l); return l; } );
			_editMesh.uv1 = GetAttribute(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(1, l); return l; } );
			_editMesh.uv2 = GetAttribute(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(2, l); return l; } );
			_editMesh.uv3 = GetAttribute(x => { List<Vector4> l = new List<Vector4>(); x.GetUVs(3, l); return l; } );

            _editMesh.SetSubMeshes(originalMesh);
		}

		internal int vertexCount { get { return originalMesh.vertexCount; } }

		// Convenience getter for gameObject.GetComponent<MeshFilter>().
		internal MeshFilter meshFilter { get; private set; }

		// Convenience getter for gameObject.transform
		internal Transform transform { get { return gameObjectAttached.transform; } }

		// Convenience getter for gameObject.renderer
		internal Renderer renderer { get { return gameObjectAttached.GetComponent<MeshRenderer>(); } }

		// If this object's mesh has been edited, isDirty will be flagged meaning that the mesh should not be
		// cleaned up when finished editing.
		internal bool isDirty = false;

        // Is the mesh owned by ProBuilder?
        internal bool isProBuilderObject { get; private set; }

		// Container for polyMesh. @todo remove when Unity fixes
		PolybrushMesh m_PolybrushMesh;

        public PolybrushMesh  polybrushMesh
        {
            get
            {
                if (m_PolybrushMesh == null)
                {
                    Initialize(gameObjectAttached);
                }

                return m_PolybrushMesh;
            }
        }

        // Did this mesh already have an additionalVertexStreams mesh?
        private bool hadVertexStreams = true;

		/// <summary>
		/// Shorthand for checking if object and mesh are non-null.
		/// </summary>
		public bool IsValid
		{
			get
			{
				if(gameObjectAttached == null || graphicsMesh == null)
					return false;

                if (isProBuilderObject)
                {
                    if (ProBuilderBridge.IsValidProBuilderMesh(gameObjectAttached)
                        && _editMesh != null
                        && _editMesh.vertexCount != ProBuilderBridge.GetVertexCount(gameObjectAttached))
                    {
                        return false;
                    }
                }

                return true;
			}
		}

        /// <summary>
        /// Public constructor for editable objects.  Guarantees that a mesh
        /// is editable and takes care of managing the asset.
        /// </summary>
        /// <param name="go">The GameObject used to create the EditableObject</param>
        /// <returns>a new EditableObject if possible, null otherwise</returns>
        internal static EditableObject Create(GameObject go)
		{
			if(go == null)
				return null;

			MeshFilter mf = go.GetComponent<MeshFilter>();
			SkinnedMeshRenderer sf = go.GetComponent<SkinnedMeshRenderer>();

			if(!mf && !sf)
			{
				mf = go.GetComponentsInChildren<MeshFilter>().FirstOrDefault();
				sf = go.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault();
			}

			if((mf == null || mf.sharedMesh == null) && (sf == null || sf.sharedMesh == null))
				return null;

            return new EditableObject(go);
		}

        private void Initialize(GameObject go)
        {
            CheckBackwardCompatiblity(go);

            gameObjectAttached = go;
            isProBuilderObject = false;

            if (ProBuilderBridge.ProBuilderExists())
                isProBuilderObject = ProBuilderBridge.IsValidProBuilderMesh(gameObjectAttached);

            Mesh mesh = null;
            MeshRenderer meshRenderer = gameObjectAttached.GetComponent<MeshRenderer>();
            meshFilter = gameObjectAttached.GetComponent<MeshFilter>();
            _skinMeshRenderer = gameObjectAttached.GetComponent<SkinnedMeshRenderer>();

            originalMesh = go.GetMesh();

            if (originalMesh == null && _skinMeshRenderer != null)
                originalMesh = _skinMeshRenderer.sharedMesh;

            m_PolybrushMesh = gameObjectAttached.GetComponent<PolybrushMesh>();

            if (m_PolybrushMesh == null)
            {
                m_PolybrushMesh = Undo.AddComponent<PolybrushMesh>(gameObjectAttached);
                m_PolybrushMesh.Initialize();
                m_PolybrushMesh.mode = (s_UseAdditionalVertexStreams) ? PolybrushMesh.Mode.AdditionalVertexStream : PolybrushMesh.Mode.Mesh;
            }

            //attach the skinmesh ref to the polybrushmesh
            //it will be used when making a prefab containing a skin mesh. The limitation here is that the skin mesh must comes from an asset (which is 99.9999% of the time)
            if (_skinMeshRenderer != null)
            {
                Mesh sharedMesh = _skinMeshRenderer.sharedMesh;
                if (AssetDatabase.Contains(sharedMesh))
                {
                    m_PolybrushMesh.skinMeshRef = sharedMesh;
                }
            }

            // if it's a probuilder object rebuild the mesh without optimization
            if (isProBuilderObject)
            {
                if (ProBuilderBridge.IsValidProBuilderMesh(gameObjectAttached))
                {
                    ProBuilderBridge.ToMesh(gameObjectAttached);
                    ProBuilderBridge.Refresh(gameObjectAttached);
                }
            }

            if (meshRenderer != null || _skinMeshRenderer != null)
            {
                mesh = m_PolybrushMesh.storedMesh;

                if (mesh == null)
                {
                    mesh = PolyMeshUtility.DeepCopy(originalMesh);
                    hadVertexStreams = false;
                }
                else
                {
                    //prevents leak
                    if (!MeshInstanceMatchesGameObject(mesh, gameObjectAttached))
                    {
                        mesh = PolyMeshUtility.DeepCopy(mesh);
                    }
                }

                mesh.name = k_MeshInstancePrefix + gameObjectAttached.GetInstanceID();
            }

            polybrushMesh.SetMesh(mesh);
            PrefabUtility.RecordPrefabInstancePropertyModifications(polybrushMesh);
            _graphicsMesh = m_PolybrushMesh.storedMesh;

            source = polybrushMesh.mode == PolybrushMesh.Mode.AdditionalVertexStream? ModelSource.AdditionalVertexStreams : PolyEditorUtility.GetMeshGUID(originalMesh);

            GenerateCompositeMesh();
        }

        /// <summary>
        /// Internal constructor.
        /// \sa Create
        /// </summary>
        /// <param name="go"></param>
        private EditableObject(GameObject go)
		{
            Initialize(go);
		}

        ~EditableObject()
		{
            // clean up the composite mesh (if required)
            // delayCall ensures Destroy is called on main thread
            // if(editMesh != null)
            // 	EditorApplication.delayCall += () => { GameObject.DestroyImmediate(editMesh); };
        }

        /// <summary>
        /// Applies mesh changes back to the pb_Object (if necessary).  Optionally does a
        /// mesh rebuild.
        /// </summary>
        /// <param name="rebuildMesh">Only applies to ProBuilder meshes.</param>
        /// <param name="optimize">Determines if the mesh collisions are rebuilt (if that option is enabled) or if
        /// the mehs is a probuilder object, the mesh is optimized (condensed to share verts, other
        /// otpimziations etc) </param>
        internal void Apply(bool rebuildMesh, bool optimize = false)
        {
            if (m_PolybrushMesh.mode == PolybrushMesh.Mode.AdditionalVertexStream)
            {
                if (PolybrushEditor.instance.tool == BrushTool.RaiseLower ||
                    PolybrushEditor.instance.tool == BrushTool.Smooth)
                {
                    if (s_RebuildNormals.value && (modifiedChannels & MeshChannel.Position) > 0)
                        PolyMeshUtility.RecalculateNormals(editMesh);

                    if (optimize)
                    {
                        graphicsMesh.RecalculateBounds();
                        UpdateMeshCollider();
                    }
                }

                editMesh.ApplyAttributesToUnityMesh(graphicsMesh, modifiedChannels);
                graphicsMesh.UploadMeshData(false);
                EditorUtility.SetDirty(gameObjectAttached.GetComponent<Renderer>());

                if (m_PolybrushMesh.componentsCache.MeshFilter)
                    Undo.RecordObject(m_PolybrushMesh.componentsCache.MeshFilter, "Assign Polymesh to MeshFilter");

                if (m_PolybrushMesh)
                    m_PolybrushMesh.SynchronizeWithMeshRenderer();
            }

            // if it's a probuilder object rebuild the mesh without optimization
            if (isProBuilderObject)
            {
                ProBuilderBridge.SetPositions(gameObjectAttached, editMesh.vertices);
                ProBuilderBridge.SetTangents(gameObjectAttached, editMesh.tangents);

                if (editMesh.colors != null && editMesh.colors.Length == editMesh.vertexCount)
                {
                    Color[] colors = System.Array.ConvertAll(editMesh.colors, x => (Color) x);
                    ProBuilderBridge.SetColors(gameObjectAttached, colors);
                }

                if (rebuildMesh)
                {
                    ProBuilderBridge.ToMesh(gameObjectAttached);
                    ProBuilderBridge.Refresh(gameObjectAttached,
                        optimize
                            ? ProBuilderBridge.RefreshMask.All
                            : (ProBuilderBridge.RefreshMask.Colors
                               | ProBuilderBridge.RefreshMask.Normals
                               | ProBuilderBridge.RefreshMask.Tangents));
                }
            }

            if (m_PolybrushMesh.mode == PolybrushMesh.Mode.AdditionalVertexStream)
            {
                modifiedChannels = MeshChannel.Null;
                return;
            }

            if (PolybrushEditor.instance.tool == BrushTool.RaiseLower ||
                PolybrushEditor.instance.tool == BrushTool.Smooth)
            {
                if (s_RebuildNormals.value)// && (modifiedChannels & MeshChannel.Position) > 0)
                    PolyMeshUtility.RecalculateNormals(editMesh);

                if (optimize)
                {
                    UpdateMeshCollider();
                    graphicsMesh.RecalculateBounds();
                }
            }

            editMesh.ApplyAttributesToUnityMesh(graphicsMesh, modifiedChannels);

            if (m_PolybrushMesh.componentsCache.MeshFilter)
                Undo.RecordObject(m_PolybrushMesh.componentsCache.MeshFilter, "Assign Polymesh to MeshFilter");

            m_PolybrushMesh.SynchronizeWithMeshRenderer();

            modifiedChannels = MeshChannel.Null;
        }
        /// <summary>
        /// Update the mesh collider
        /// expensive call, delay til optimize is enabled.
        /// </summary>
        private void UpdateMeshCollider()
        {
            if (s_RebuildCollisions.value)
            {
                var mc = gameObjectAttached.GetComponent<MeshCollider>();
                if (mc != null)
                {
                    if(mc.sharedMesh != graphicsMesh)
                        Undo.RecordObject(mc, "Assign PolybrushMesh to MeshCollider");

                    //The shared mesh should update automatically but doesn't,
                    //re-assigning it does the trick but might be costly
                    mc.sharedMesh = graphicsMesh;
                }
            }
        }

        /// <summary>
        /// Apply the mesh channel attributes to the graphics mesh.
        /// </summary>
        /// <param name="channel"></param>
        internal void ApplyMeshAttributes(MeshChannel channel = MeshChannel.All)
		{
			editMesh.ApplyAttributesToUnityMesh(_graphicsMesh, channel);

            if (m_PolybrushMesh.mode == PolybrushMesh.Mode.AdditionalVertexStream)
				_graphicsMesh.UploadMeshData(false);
		}

        /// <summary>
        /// Set the MeshFilter or SkinnedMeshRenderer back to originalMesh.
        /// </summary>
        internal void Revert()
		{
            if (isProBuilderObject)
                Apply(true, true);

            RemovePolybrushComponentsIfNecessary();

            if (m_PolybrushMesh.mode == PolybrushMesh.Mode.AdditionalVertexStream)
			{
				if(!hadVertexStreams)
				{
					GameObject.DestroyImmediate(graphicsMesh);
					MeshRenderer mr = gameObjectAttached.GetComponent<MeshRenderer>();
                    if(mr != null)
                    {
                        mr.additionalVertexStreams = null;
                    }
                }
                return;
			}

            if (isProBuilderObject)
                return;

            if (originalMesh == null || (source == ModelSource.Scene && !UnityPrimitiveMeshNames.Contains(originalMesh.name)))
                return;

            if (graphicsMesh != null)
                GameObject.DestroyImmediate(graphicsMesh);

            PrefabUtility.RecordPrefabInstancePropertyModifications(polybrushMesh);
        }

		public bool Equals(EditableObject rhs)
		{
			return rhs.GetHashCode() == GetHashCode();
		}

		public override bool Equals(object rhs)
		{
			if(rhs == null)
				return gameObjectAttached == null ? true : false;
			else if(gameObjectAttached == null)
				return false;

			if(rhs is EditableObject)
				return rhs.Equals(this);
			else if(rhs is GameObject)
				return ((GameObject)rhs).GetHashCode() == gameObjectAttached.GetHashCode();

			return false;
		}

		public override int GetHashCode()
		{
			return gameObjectAttached != null ? gameObjectAttached.GetHashCode() : base.GetHashCode();
		}

		internal static int GetMeshId(Mesh mesh)
		{
			if (mesh == null)
				return -1;

			int meshId = -1;
			string meshName = mesh.name;

			if(!meshName.StartsWith(k_MeshInstancePrefix) || !int.TryParse(meshName.Replace(k_MeshInstancePrefix, ""), out meshId))
				return meshId;

			return meshId;
		}

		static bool MeshInstanceMatchesGameObject(Mesh mesh, GameObject go)
		{
            if (ProBuilderBridge.IsValidProBuilderMesh(go))
                return true;

            int gameObjectId = go.GetInstanceID();
			int meshId = GetMeshId(mesh);

			// If the mesh id doesn't parse to an ID it's definitely not an instance
			if(meshId == -1)
				return false;

			// If the mesh id matches the instance id, it's already a scene instance owned by this object. If doesn't match,
			// next check that the mesh id gameObject does not exist. If it does exist, that means this mesh was duplicated
			// and already belongs to another object in the scene. If it doesn't exist, then it just means that the GameObject
			// id was changed as a normal part of the GameObject lifecycle.
			if (meshId == gameObjectId)
				return true;

			// If it is an instance, and the IDs don't match but no existing GameObject claims this mesh, claim it.
			if (EditorUtility.InstanceIDToObject(meshId) == null)
			{
				mesh.name = k_MeshInstancePrefix + go.GetInstanceID();
				return true;
			}

			// The mesh did not match the gameObject id, and the mesh id points to an already existing object in the scene.
			return false;
		}

        internal void RemovePolybrushComponentsIfNecessary()
        {
            if (isProBuilderObject)
            {
                GameObject.DestroyImmediate(m_PolybrushMesh);
                return;
            }

            // Check if there's any modification on the PolybrushMesh component.
            // If there is none, remove it from the GameObject.
            if (!m_PolybrushMesh.hasAppliedChanges)
            {
                polybrushMesh.SetMesh(originalMesh);
            }
        }

        internal void ClearMeshBuffers()
        {
            visualMesh.ClearBuffers();
            _editMesh.ClearBuffers();
        }

#pragma warning disable 612
        /// <summary>
        /// Checks if object contains old data structure. If so, converts to our new format.
        /// Will trigger if object has been edited with the Asset Store (Beta) version.
        /// </summary>
        void CheckBackwardCompatiblity(GameObject go)
        {

            z_AdditionalVertexStreams oldFormat = go.GetComponent<z_AdditionalVertexStreams>();
            if (oldFormat != null)
                PolyEditorUtility.ConvertGameObjectToNewFormat(oldFormat);
        }
#pragma warning restore 612
    }
}
