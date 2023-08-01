using System;
using System.IO;
using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    static class ShaderMetaDataUtility
    {
#pragma warning disable 0618
        [Obsolete("Format is deprecated. Please use ShaderMetaDataUtility.LoadShaderMetaData and ShaderMetaDataUtility.SaveShaderMetaData.")]
        const string SHADER_ATTRIB_FILE_EXTENSION = "pbs.json";

        /// <summary>
        /// Find a path to the Polybrush metadata for a shader.
        /// </summary>
        /// <param name="shader">Shader associated with the metadata</param>
        /// <returns>The path found, null if not found.</returns>
        [Obsolete("Please use ShaderMetaDataUtility.LoadShaderMetaData.")]
        internal static string FindPolybrushMetaDataForShader(Shader shader)
        {
            if (shader == null)
                return null;

            string path = AssetDatabase.GetAssetPath(shader);

            if (string.IsNullOrEmpty(path))
                return null;

            string filename = Path.GetFileNameWithoutExtension(path);
            string directory = Path.GetDirectoryName(path);

            string[] paths = new string[]
            {
                string.Format("{0}/{1}.{2}", directory, PolyShaderUtil.GetMetaDataPath(shader), SHADER_ATTRIB_FILE_EXTENSION),
                string.Format("{0}/{1}.{2}", directory, filename, SHADER_ATTRIB_FILE_EXTENSION)
            };

            // @todo verify that the json is actually valid
            foreach (string str in paths)
            {
                if (File.Exists(str))
                {
                    // remove `..` from path since `AssetDatabase.LoadAssetAtPath` doesn't like 'em
                    string full = Path.GetFullPath(str).Replace("\\", "/");
                    string resolved = full.Replace(Application.dataPath, "Assets");
                    return resolved;
                }
            }

            return null;
        }

        /// <summary>
        /// Try to read AttributeLayouts from a .pbs.json file located at "path"
        /// </summary>
        /// <param name="path">Path of the file to read from</param>
        /// <param name="container">AttributeLayoutContainer retrieved from the json</param>
        /// <returns>true if it worked, false if the file doesn't exist or is empty</returns>
        [Obsolete("Please use ShaderMetaDataUtility.LoadShaderMetaData.")]
        public static bool TryReadAttributeLayoutsFromJsonFile(string path, out AttributeLayoutContainer container)
        {
            container = null;

            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
                return false;

            container = ScriptableObject.CreateInstance<AttributeLayoutContainer>();
            JsonUtility.FromJsonOverwrite(json, container);

            ResolveShaderReference(container);

            return true;
        }

        [Obsolete("Please use ShaderMetaDataUtility.LoadShaderMetaData.")]
        public static bool TryReadAttributeLayoutsFromJson(string jsonText, out AttributeLayoutContainer container)
        {
            container = ScriptableObject.CreateInstance<AttributeLayoutContainer>();
            JsonUtility.FromJsonOverwrite(jsonText, container);

            ResolveShaderReference(container);

            return true;
        }

        /// <summary>
        /// Store user-set shader attribute information.
        /// </summary>
        /// <param name="container">container that will have the shader and the metadata to write</param>
        /// <param name="overwrite">overwrite data if already existing</param>
        /// <param name="logErrors">log errors or not</param>
        /// <returns>Returns the path written to on success, null otherwise.</returns>
        [Obsolete("Please use ShaderMetaDataUtility.SaveShaderMetaData.")]
        internal static string SaveMeshAttributesData(AttributeLayoutContainer container, bool overwrite = false, bool logErrors = true)
        {
            if (container == null) return string.Empty;

            return SaveMeshAttributesData(container.shader, container.attributes, overwrite);
        }

        /// <summary>
        /// Saves the metadata of the shader passed in parameters, can overwrite if necessary
        /// </summary>
        /// <param name="shader">Shader associated with the metadata</param>
        /// <param name="attributes">Metadata to write</param>
        /// <param name="overwrite">Will overwrite if already existing file</param>param>
        /// <param name="logErrors">Log errors or not</
        /// <returns></returns>
        [Obsolete("Please use ShaderMetaDataUtility.SaveShaderMetaData.")]
        internal static string SaveMeshAttributesData(Shader shader, AttributeLayout[] attributes, bool overwrite = false, bool logErrors = true)
        {
            if (shader == null || attributes == null)
            {
                if (logErrors)
                {
                    Debug.LogError("Cannot save null attributes for shader.");
                }

                return null;
            }

            string path = FindPolybrushMetaDataForShader(shader);
            string shader_path = AssetDatabase.GetAssetPath(shader);
            string shader_directory = Path.GetDirectoryName(shader_path);
            string shader_filename = Path.GetFileNameWithoutExtension(path);

            // metadata didn't exist before
            if (string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(shader_path))
                {
                    // how!?
                    path = EditorUtility.SaveFilePanelInProject(
                        "Save Polybrush Shader Attributes",
                        shader_filename,
                        SHADER_ATTRIB_FILE_EXTENSION,
                        "Please enter a file name to save Polybrush shader metadata to.");

                    if (string.IsNullOrEmpty(path))
                    {
                        Debug.LogWarning(string.Format("Could not save Polybrush shader metadata.  Please try again, possibly with a different file name or folder path."));
                        return null;
                    }
                }
                else
                {
                    shader_filename = Path.GetFileNameWithoutExtension(shader_path);
                    path = string.Format("{0}/{1}.{2}", shader_directory, shader_filename, SHADER_ATTRIB_FILE_EXTENSION);
                }
            }

            if (!overwrite && File.Exists(path))
            {
                // @todo
                Debug.LogWarning("shader metadata exists. calling function refuses to overwrite and lazy developer didn't add a save dialog here.");
                return null;
            }

            try
            {
                AttributeLayoutContainer container = AttributeLayoutContainer.Create(shader, attributes);
                string json = JsonUtility.ToJson(container, true);
                File.WriteAllText(path, json);

                //note: convert it here to be able to load it using AssetDatabase functions
                shader_filename = Path.GetFileNameWithoutExtension(shader_path);
                path = string.Format("{0}/{1}.{2}", shader_directory, shader_filename, SHADER_ATTRIB_FILE_EXTENSION);
                //-------

                return path;
            }
            catch (System.Exception e)
            {
                if (logErrors)
                {
                    Debug.LogError("Failed saving Polybrush Shader MetaData\n" + e.ToString());
                }
                return path;
            }
        }

        /// <summary>
        /// Searches only by looking for a compatibly named file in the same directory.
        /// </summary>
        /// <param name="shader">Shader associated with the metadata</param>
        /// <param name="attributes">result if any metadata found</param>
        /// <returns></returns>
        [Obsolete("Please use ShaderMetaDataUtility.LoadShaderMetaData.")]
        internal static bool FindMeshAttributesForShader(Shader shader, out AttributeLayoutContainer attributes)
        {
            attributes = null;

            string path = AssetDatabase.GetAssetPath(shader);
            string filename = Path.GetFileNameWithoutExtension(path);
            string directory = Path.GetDirectoryName(path);

            string[] paths = new string[]
            {
                string.Format("{0}/{1}.{2}", directory, filename, SHADER_ATTRIB_FILE_EXTENSION),
                string.Format("{0}/{1}.{2}", directory, PolyShaderUtil.GetMetaDataPath(shader), SHADER_ATTRIB_FILE_EXTENSION)
            };

            foreach (string str in paths)
            {
                if (TryReadAttributeLayoutsFromJsonFile(str, out attributes))
                    return true;
            }

            return false;
        }

        [Obsolete("Method is deprecated. Please use ShaderMetaDataUtility.LoadShaderMetaData and ShaderMetaDataUtility.SaveShaderMetaData.")]
        static void ResolveShaderReference(AttributeLayoutContainer container)
        {
            container.shader = Shader.Find(container.shaderPath);
        }

        /// <summary>
        /// Store the shader's attributes in the new format.
        /// Erase the .pbs.json on success.
        /// </summary>
        internal static void ConvertMetaDataToNewFormat(Shader shader)
        {
            if (shader == null)
                throw new NullReferenceException("shader");

            string path = ShaderMetaDataUtility.FindPolybrushMetaDataForShader(shader);

            // If not null, it means we have data stored with the old format.
            // Proceed to conversion.
            if (path != null)
            {
                AttributeLayoutContainer attributesContainer = ScriptableObject.CreateInstance<AttributeLayoutContainer>();
                ShaderMetaDataUtility.TryReadAttributeLayoutsFromJsonFile(path, out attributesContainer);
                if (attributesContainer != null)
                {
                    ShaderMetaDataUtility.SaveShaderMetaData(shader, attributesContainer);
                    FileUtil.DeleteFileOrDirectory(path);
                    FileUtil.DeleteFileOrDirectory(path + ".meta");
                    AssetDatabase.Refresh();
                }
            }
        }
#pragma warning restore 0618

        /// <summary>
        /// Check if the given shader is an asset we can work with.
        /// We will verify if it comes from the project by checking its importer.
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        internal static bool IsValidShader(Shader shader)
        {
            if (shader == null)
                throw new ArgumentNullException("shader");

            string path = AssetDatabase.GetAssetPath(shader);
            AssetImporter importer = AssetImporter.GetAtPath(path);

            return importer != null;
        }

        /// <summary>
        /// Deserialize the shader's attributes from UserData in the shader's importer.
        /// If none exists, it returns a new AttributeLayoutContainer instance.
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        internal static AttributeLayoutContainer LoadShaderMetaData(Shader shader)
        {
            if (shader == null)
                throw new ArgumentNullException("shader");

            string path = AssetDatabase.GetAssetPath(shader);
            AssetImporter importer = AssetImporter.GetAtPath(path);

            AttributeLayoutContainer data = AttributeLayoutContainer.Create(shader, null);
            JsonUtility.FromJsonOverwrite(importer.userData, data);
            return data;
        }

        /// <summary>
        /// Serialize the shader's attributes as UserData in the shader's importer.
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="attributes"></param>
        internal static void SaveShaderMetaData(Shader shader, AttributeLayoutContainer attributes)
        {
            if (shader == null)
                throw new ArgumentNullException("shader");

            if (attributes == null)
                throw new ArgumentNullException("attributes");

            string path = AssetDatabase.GetAssetPath(shader);
            AssetImporter importer = AssetImporter.GetAtPath(path);

            importer.userData = JsonUtility.ToJson(attributes);
            importer.SaveAndReimport();
        }
    }
}
