using System.IO;
using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    static class ShaderMetaDataUtility
    {
        const string SHADER_ATTRIB_FILE_EXTENSION = "pbs.json";

        /// <summary>
        /// Find a path to the Polybrush metadata for a shader.
        /// </summary>
        /// <param name="shader">Shader associated with the metadata</param>
        /// <returns>The path found, null if not found.</returns>
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
        /// <param name="overwrite">Will overwrite if already existing file</param>
        /// <param name="logErrors">Log errors or not</param>
        /// <returns></returns>
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

        static void ResolveShaderReference(AttributeLayoutContainer container)
        {
            container.shader = Shader.Find(container.shaderPath);
        }
    }
}
