using UnityEngine;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Polybrush;
using Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Helper functions for editor
    /// </summary>
	static class PolyEditorUtility
	{
		const string k_PackageDirectory = "Packages/com.unity.polybrush";
		const string k_UserAssetDirectory = "Assets/Polybrush Data/";

		/// <summary>
		/// True if this gameObject is in the Selection.gameObjects
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		internal static bool InSelection(GameObject gameObject)
		{
            //null check
            if(gameObject == null)
            {
                return false;
            }

            Transform[] selectedTransform = Selection.GetTransforms(SelectionMode.Unfiltered);

            return selectedTransform.Contains<Transform>(gameObject.transform);
		}

        /// <summary>
        /// GetMeshGUID version without the ref parameter
        /// </summary>
        /// <param name="mesh">mesh source</param>
        /// <returns>ModelSource category</returns>
        internal static ModelSource GetMeshGUID(Mesh mesh)
        {
            string temp = string.Empty;
            return GetMeshGUID(mesh, ref temp);
        }

		/// <summary>
		/// Return the mesh source, and the guid if applicable (scene instances don't get GUIDs).
		/// </summary>
		/// <param name="mesh">mesh source</param>
		/// <param name="guid">guid returned</param>
		/// <returns>ModelSource category</returns>
		internal static ModelSource GetMeshGUID(Mesh mesh, ref string guid)
		{
            if(mesh == null)
            {
                return ModelSource.Error;
            }
			string path = AssetDatabase.GetAssetPath(mesh);

			if(path != "")
			{
				AssetImporter assetImporter = AssetImporter.GetAtPath(path);

				if( assetImporter != null )
				{
					// Only imported model (e.g. FBX) assets use the ModelImporter,
					// where a saved asset will have an AssetImporter but *not* ModelImporter.
					// A procedural mesh (one only existing in a scene) will not have any.
					if (assetImporter is ModelImporter)
					{
						guid = AssetDatabase.AssetPathToGUID(path);
						return ModelSource.Imported;
					}
					else
					{
						guid = AssetDatabase.AssetPathToGUID(path);
						return ModelSource.Asset;
					}
				}
				else
				{
					return ModelSource.Scene;
				}
			}

			return ModelSource.Scene;
		}

		internal const int DIALOG_OK = 0;
		internal const int DIALOG_CANCEL = 1;
		internal const int DIALOG_ALT = 2;
		internal const string DO_NOT_SAVE = "DO_NOT_SAVE";

        /// <summary>
        /// Save any modifications to the EditableObject.  If the mesh is a scene mesh or imported mesh, it
        /// will be saved to a new asset.  If the mesh was originally an asset mesh, the asset is overwritten.
        /// </summary>
        /// <param name="mesh">mesh to save</param>
        /// <param name="meshFilter">will update the mesh filter with the new mesh if not null</param>
        /// <param name="skinnedMeshRenderer">will update the skinned mesh renderer with the new mesh if not null</param>
        /// <returns>return true if save was successful, false if user-canceled or otherwise failed.</returns>
        internal static bool SaveMeshAsset(Mesh mesh, MeshFilter meshFilter = null, SkinnedMeshRenderer skinnedMeshRenderer = null, int overridenDialogResult = -1, string overridenPath = "")
		{
            if (mesh == null) return false;

			string save_path = !string.IsNullOrEmpty(overridenPath) ? overridenPath : DO_NOT_SAVE;

			ModelSource source = GetMeshGUID(mesh);

            switch (source)
            {
                case ModelSource.Asset:

                    int saveChanges = overridenDialogResult != -1
                        ? overridenDialogResult
                        : EditorUtility.DisplayDialogComplex(
                            "Save Changes",
                            "Save changes to edited mesh?",
                            "Save", // DIALOG_OK
                            "Cancel", // DIALOG_CANCEL
                            "Save As"); // DIALOG_ALT

                    if(saveChanges == DIALOG_OK)
                        save_path = AssetDatabase.GetAssetPath(mesh);
                    else if(saveChanges == DIALOG_ALT)
                        save_path = EditorUtility.SaveFilePanelInProject("Save Mesh As", mesh.name + ".asset", "asset", "Save edited mesh to");
                    else
                        return false;

                    break;

                case ModelSource.Imported:
                case ModelSource.Scene:
                default:
                    save_path = EditorUtility.SaveFilePanelInProject("Save Mesh As", mesh.name + ".asset", "asset", "Save edited mesh to");
                    break;
            }

            if(!save_path.Equals(DO_NOT_SAVE) && !string.IsNullOrEmpty(save_path))
            {
                Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(save_path);
                bool overwrite = existing != null;
                Mesh dst = overwrite ? existing : new Mesh();
                PolyMeshUtility.Copy(mesh, dst);
                if(!overwrite)
                    AssetDatabase.CreateAsset(dst, save_path);
				AssetDatabase.Refresh();
				return true;
			}

			// Save was canceled
			return false;
		}

        /// <summary>
        /// Load an icon
        /// </summary>
        /// <param name="icon">location of the icon</param>
        /// <returns>The loaded icon</returns>
		internal static Texture2D LoadIcon(string icon)
		{
			MethodInfo loadIconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			Texture2D img = (Texture2D) loadIconMethod.Invoke(null, new object[] { icon } );
			return img;
		}

		internal static string RootFolder
		{
			get
			{
				return k_PackageDirectory;
			}
		}

		internal static string UserAssetDirectory
		{
			get
            {
                return k_UserAssetDirectory;
            }
		}

		internal static T LoadDefaultAsset<T>(string path) where T : UnityEngine.Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(k_PackageDirectory + "/Content/" + path);
		}

        /// <summary>
        /// Fetch a default asset from path relative to the product folder. If not found, a new one is created.
        /// </summary>
        /// <typeparam name="T">>the type to retrieve, must inherit from ScriptableObject and implement IHasDefault and ICustomSettings</typeparam>
        /// <returns>The first asset of the type T found inside the project (order set by AssetDataBase.FindAssets function. If none returns a new object T with default values set by IHasDefault.</returns>
        internal static T GetFirstOrNew<T>() where T : ScriptableObject, IHasDefault, ICustomSettings
		{
			string[] all = AssetDatabase.FindAssets("t:" + typeof(T));

			T asset = all.Length > 0 ? AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(all.First())) : null;

			if(asset == null)
			{
                return CreateNew<T>();
			}

			return asset;
		}

        /// <summary>
        /// Create a new asset at path relative to the product folder.
        /// </summary>
        /// <typeparam name="T">>the type to create, must inherit from ScriptableObject and implement IHasDefault and ICustomSettings</typeparam>
        /// <returns>A new object T with default values set by IHasDefault.</returns>
        internal static T CreateNew<T>() where T : ScriptableObject, IHasDefault, ICustomSettings
        {
            T asset = ScriptableObject.CreateInstance<T>();
            asset.SetDefaultValues();
            EditorUtility.SetDirty(asset);

            string folder = UserAssetDirectory;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string subfolder = folder + asset.assetsFolder;
            if (!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(subfolder + typeof(T).Name + "-Default.asset");

            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        /// <summary>
        /// Fetch all assets of type `T`
        /// </summary>
        /// <typeparam name="T">the type to retrieve, must inherit from ScriptableObject and implement IHasDefault and ICustomSettings</typeparam>
        /// <returns>All the assets of that type on the project</returns>
        internal static List<T> GetAll<T>() where T: ScriptableObject, IHasDefault, ICustomSettings
        {
            var tGuids = AssetDatabase.FindAssets("t:" + typeof(T));
            var Ts = new List<T>();
            foreach (var guid in tGuids)
                Ts.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            return Ts;
        }

        /// <summary>
        /// Set the selected render state for an object.  In Unity 5.4 and lower, this just toggles wireframe on or off.
        /// </summary>
        /// <param name="renderer">Renderer to change selection state</param>
        /// <param name="state">State to be set</param>
        internal static void SetSelectionRenderState(Renderer renderer, SelectionRenderState state)
		{
			#if UNITY_5_3 || UNITY_5_4
				EditorUtility.SetSelectedWireframeHidden(renderer, state == 0);
			#else
				EditorUtility.SetSelectedRenderState(renderer, (EditorSelectedRenderState) state );
			#endif
		}

		internal static SelectionRenderState GetSelectionRenderState()
		{

			#if UNITY_5_3 || UNITY_5_4

			return SelectionRenderState.Wireframe;

			#else

			bool wireframe = false, outline = false;

			try {
				wireframe = (bool) ReflectionUtility.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionWire");
				outline = (bool) ReflectionUtility.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionOutline");
			} catch {
				Debug.LogWarning("Looks like Unity changed the AnnotationUtility \"showSelectionOutline\"\nPlease email contact@procore3d.com and let Karl know!");
			}

			SelectionRenderState state = SelectionRenderState.None;

			if(wireframe) state |= SelectionRenderState.Wireframe;
			if(outline) state |= SelectionRenderState.Outline;

			return state;

			#endif
		}

        /**
        * Returns true if this object is a prefab in the Project view.
        */
        internal static bool IsPrefabAsset(Object go)
	    {
#if UNITY_2018_3_OR_NEWER
	        return PrefabUtility.IsPartOfPrefabAsset(go);
#else
			return PrefabUtility.GetPrefabType(go) == PrefabType.Prefab;
#endif
	    }

        /**
        * Returns true if the given array contains at least one prefab.
        */
        internal static bool ContainsPrefabAssets(UnityEngine.Object[] objects)
	    {
	        for (int i = 0; i < objects.Length; ++i)
	        {
	            UnityEngine.Object obj = objects[i];
	            if (PolyEditorUtility.IsPrefabAsset(obj))
	                return true;
	        }

	        return false;
	    }

        /// <summary>
        /// Returns true if GameObject has a PolybrushMesh component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static bool IsPolybrushObject(GameObject gameObject)
        {
            return gameObject.GetComponent<PolybrushMesh>() != null;
        }

#pragma warning disable 612
        /// <summary>
        /// Utility to help convert objects modified with the Beta version of Polybrush (Asset Store)
        /// to Polybrus 1.x.
        /// </summary>
        /// <param name="component"></param>
        internal static PolybrushMesh ConvertGameObjectToNewFormat(z_AdditionalVertexStreams component)
        {
            GameObject go = component.gameObject;
            PolybrushMesh newComponent = go.GetComponent<PolybrushMesh>();
            MeshFilter mf = go.GetComponent<MeshFilter>();
            Mesh mesh = component.m_AdditionalVertexStreamMesh;

            Undo.DestroyObjectImmediate(component);

            // Cancel conversion if no mesh if found on Z_AdditionalVertexStreams
            if (mesh == null)
                return null;

            if (newComponent == null)
            {
                newComponent = Undo.AddComponent<PolybrushMesh>(go);
                newComponent.Initialize();
            }

            newComponent.mode = PolybrushMesh.Mode.AdditionalVertexStream;
            newComponent.SetMesh(PolyMeshUtility.DeepCopy(mf.sharedMesh));
            newComponent.SetAdditionalVertexStreams(mesh);

            return newComponent;
        }
#pragma warning restore 612
    }
}
