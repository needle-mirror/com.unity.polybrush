using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// GUI field extensions.
    /// </summary>
    internal static class PolyGUI
	{
		internal static readonly Color k_BoxBackgroundLight = new Color(.8f, .8f, .8f, 1f);
		internal static readonly Color k_BoxBackgroundDark = new Color(.24f, .24f, .24f, 1f);
		internal static readonly Color k_BoxOutlineLight = new Color(0.6745f, 0.6745f, 0.6745f, 1f);
		internal static readonly Color k_BoxOutlineDark = new Color(.29f, .29f, .29f, 1f);

		/// Used as a container to pass text to GUI functions requiring a GUIContent without allocating
		/// a new GUIContent isntance.
		static GUIContent s_CachedGUIContent = new GUIContent("", "");

		internal static GUIContent TempContent(string text, string tooltip = null)
		{
			s_CachedGUIContent.text = text;
			s_CachedGUIContent.tooltip = tooltip;
			return s_CachedGUIContent;
		}

		/// Maintain GUI.backgroundColor history.
		internal static Stack<Color> s_BackgroundColor = new Stack<Color>();

		internal static void PushBackgroundColor(Color bg)
		{
			s_BackgroundColor.Push(GUI.backgroundColor);
			GUI.backgroundColor = bg;
		}

		internal static void PopBackgroundColor()
		{
            if(s_BackgroundColor.Count > 0)
            {
                GUI.backgroundColor = s_BackgroundColor.Pop();
            }
        }

		static GUIStyle _backgroundColorStyle = null;

		internal static GUIStyle BackgroundColorStyle
		{
			get
			{
				if(_backgroundColorStyle == null)
				{
					_backgroundColorStyle = new GUIStyle();
					_backgroundColorStyle.margin = new RectOffset(4,4,4,4);
					_backgroundColorStyle.padding = new RectOffset(4,4,4,4);
					_backgroundColorStyle.normal.background = EditorGUIUtility.whiteTexture;
				}

				return _backgroundColorStyle;
			}
		}

		static GUIStyle _centeredStyle = null;

		internal static GUIStyle CenteredStyle
		{
			get
			{
				if(_centeredStyle == null)
				{
					_centeredStyle = new GUIStyle();
					_centeredStyle.alignment = TextAnchor.MiddleCenter;
					_centeredStyle.normal.textColor = new Color(.85f, .85f, .85f, 1f);
					_centeredStyle.wordWrap = true;
				}
				return _centeredStyle;
			}
		}
	}
}
