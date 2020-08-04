using UnityEngine;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Describes the different directions in which the brush tool can move vertices.
    /// </summary>
    internal enum PolyDirection
	{
		BrushNormal     = 0,
		VertexNormal    = 1,
		Up              = 2,
		Right           = 3,
		Forward         = 4
	}

    /// <summary>
    /// Helper methods for working with Direction.
    /// </summary>
    internal static class DirectionUtil
	{
        /// <summary>
        /// Convert a direction to a vector.  If dir is Normal, 0 is returned.
        /// </summary>
        /// <param name="dir"> direction to be converted</param>
        /// <returns>vector value of the converted direction</returns>
        internal static Vector3 ToVector3(this PolyDirection dir)
		{
			switch(dir)
			{
				case PolyDirection.Up:
					return Vector3.up;
				case PolyDirection.Right:
					return Vector3.right;
				case PolyDirection.Forward:
					return Vector3.forward;
				default:
					return Vector3.zero;
			}
		}
	}
}
