using System;
using UnityEngine;

namespace UnityEngine.Polybrush
{

    /// <summary>
    /// RGBA / XYZW / 0123
    /// </summary>
	internal enum ComponentIndex
	{
		R = 0,
		X = 0,

		G = 1,
		Y = 1,

		B = 2,
		Z = 2,

		A = 3,
		W = 3
	};

    /// <summary>
    /// The type of value represented by a `ComponentIndex`
    /// </summary>
	internal enum ComponentIndexType
	{
		Vector = 0,
		Color = 1,
		Index = 2
	};

	internal static class ComponentIndexUtility
	{
        /// <summary>
        /// Convert a `ComponentIndex` enum value into a flag
        /// </summary>
        /// <param name="e"></param>
        /// <returns>the flag value</returns>
		internal static uint ToFlag(this ComponentIndex e)
		{
            //out of range case
            if (!Enum.IsDefined(typeof(ComponentIndex), e)) return (uint)e;

			int i = ((int)e) + 1;
			return (uint)(i < 3 ? i : i == 3 ? 4 : 8);
		}

        /// <summary>
        /// Get the corresponding string associated with a `ComponentIndex` enum 
        /// according to it's `ComponentIndexType`
        /// </summary>
        /// <param name="component"></param>
        /// <param name="type"></param>
        /// <returns>The string representation of component</returns>
		internal static string GetString(this ComponentIndex component, ComponentIndexType type = ComponentIndexType.Vector)
		{
            //out of range case
            if (!Enum.IsDefined(typeof(ComponentIndex), component)) return ((int)component).ToString();

            int ind = ((int)component);

			if(type == ComponentIndexType.Vector)
				return ind == 0 ? "X" : (ind == 1 ? "Y" : (ind == 2 ? "Z" : "W"));
			else if(type == ComponentIndexType.Color)
				return ind == 0 ? "R" : (ind == 1 ? "G" : (ind == 2 ? "B" : "A"));
			else
				return ind.ToString();
		}

        /// <summary>
        /// GUIContent array to display labels for `ComponentIndex` values
        /// Used only by the `AttributeLayoutContainerEditor`
        /// </summary>
		internal static readonly GUIContent[] ComponentIndexPopupDescriptions = new GUIContent[]
		{
			new GUIContent("R"),
			new GUIContent("G"),
			new GUIContent("B"),
			new GUIContent("A")
		};

        /// <summary>
        /// int array containing the possible values of `ComponentIndex`
        /// Used only by the `AttributeLayoutContainerEditor`
        /// </summary>
		internal static readonly int[] ComponentIndexPopupValues = new int[]
		{
			0,
			1,
			2,
			3
		};

	}
}
