using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Utility methods for working with shaders.
    /// </summary>
    internal static class PolyShaderUtil
	{
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
    }	
}
