using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// The default editor for SplatWeight.
    /// </summary>
    [CustomEditor(typeof(SplatWeight))]
	internal class SplatWeightEditor : Editor
	{
		static int thumbSize = 64;

        /// <summary>
        /// Editor for blend.  Returns true if blend has been modified.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blend"></param>
        /// <param name="attribs"></param>
        /// <returns></returns>
        internal static int OnInspectorGUI(int index, ref SplatWeight blend, AttributeLayout[] attribs)
		{
			// if(blend == null && attribs != null)
			// 	blend = new SplatWeight( SplatWeight.GetChannelMap(attribs) );

			// bool mismatchedOrNullAttributes = blend == null || !blend.MatchesAttributes(attribs);

			Rect r = GUILayoutUtility.GetLastRect();
			int yPos = (int) Mathf.Ceil(r.y + r.height);

			index = PolyGUILayout.ChannelField(index, attribs, thumbSize, yPos);

			return index;
		}
	}
}
