using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace UnityEditor.Polybrush
{
	[InitializeOnLoad]
	internal static class IconUtility
	{
		const string k_IconFolder = "Packages/com.unity.polybrush/Content/Icons/";
		static Dictionary<string, Texture2D> m_icons = new Dictionary<string, Texture2D>();

		internal static Texture2D GetIcon(string iconName)
		{
			return GetTextureInFolder(k_IconFolder, iconName + ((EditorGUIUtility.pixelsPerPoint > 1f) ? "@2x" : ""));
		}

		internal static Texture2D GetTextureInFolder(string folder, string name)
		{
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

			int ext = name.LastIndexOf('.');
			string nameWithoutExtension = ext < 0 ? name : name.Substring(0, ext);
			Texture2D icon = null;

			if(!m_icons.TryGetValue(nameWithoutExtension, out icon))
			{
				string fullPath = string.Format("{0}{1}.png", folder, nameWithoutExtension);

				icon = (Texture2D) AssetDatabase.LoadAssetAtPath(fullPath, typeof(Texture2D));

				if(icon == null)
				{
					m_icons.Add(nameWithoutExtension, null);
					return null;
				}

				m_icons.Add(nameWithoutExtension, icon);
			}

			return icon;
		}
	}
}
