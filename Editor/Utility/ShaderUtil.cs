using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Utility methods for working with shaders.
    /// </summary>
    internal static class PolyShaderUtil
	{
        /// <summary>
        /// Attempt to read the shader source code to a string.  
        /// </summary>
        /// <param name="material">The material we want it's shader to be read</param>
        /// <returns>If source can't be found (built-in shaders are in binary bundles)
        /// an empty string is returned.</returns>
        internal static string GetSource(Material material)
		{
			if(material == null || material.shader == null)
				return null;

            return GetSource(material.shader);
        }

		internal static string GetSource(Shader shader)
		{
			string path = AssetDatabase.GetAssetPath(shader);

            // built-in shaders don't have a valid path.
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns true if shader has a COLOR attribute.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static bool SupportsVertexColors(Shader source)
		{
			return SupportsVertexColors(GetSource(source));
		}

		internal static bool SupportsVertexColors(string source)
		{
			return Regex.Match(source, "float4\\s.*\\s:\\sCOLOR;").Success || Regex.Match(source, "UnityEditor.ShaderGraph.VertexColorNode").Success;
		}

        /// <summary>
        /// Parse the shader source for a Z_SHADER_METADATA line with the path
        /// to the shader's polybrush metadata.  Path should be relative to the
        /// directory of the shader.
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        internal static string GetMetaDataPath(Shader shader)
		{
            if(shader == null)
            {
                return null;
            }

			string src = GetSource(shader);

			if(!string.IsNullOrEmpty(src))
			{
				Match match = Regex.Match(src, "(?<=SHADER_METADATA).*");

				if(match.Success)
				{
					string res = match.Value.Trim();
					res = res.Replace(".pbs", "");
					res = res.Replace(".shader", "");
					return res;
				}
			}

			return null;
		}

        /// <summary>
        /// Loads AttributeLayout data from a shader.  Checks for both legacy (define Z_TEXTURE_CHANNELS) and
        /// .pbs.json metadata.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="attribContainer"></param>
        /// <returns></returns>
        internal static bool GetMeshAttributes(Material material, out AttributeLayoutContainer attribContainer)
		{
			attribContainer = null;

			if(material == null)
				return false;

            // first search for json, then fall back on legacy
            if (ShaderMetaDataUtility.FindMeshAttributesForShader(material.shader, out attribContainer))
            {
				Dictionary<string, int> shaderProperties = new Dictionary<string, int>();

				for(int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); i++)
					shaderProperties.Add(ShaderUtil.GetPropertyName(material.shader, i), i);

				foreach(AttributeLayout a in attribContainer.attributes)
				{
					int index = -1;

					if(shaderProperties.TryGetValue(a.propertyTarget, out index))
					{
						if(ShaderUtil.GetPropertyType(material.shader, index) == ShaderUtil.ShaderPropertyType.TexEnv)
							a.previewTexture = (Texture2D) material.GetTexture(a.propertyTarget);
					}
				}

				return true;
			}
			return false;
		}
    }	
}
