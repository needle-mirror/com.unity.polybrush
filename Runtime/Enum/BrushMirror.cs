namespace UnityEngine.Polybrush
{
	[System.Flags]
	internal enum BrushMirror
	{
		None = 0x0,
		X = 0x1,
		Y = 0x2,
		Z = 0x4
	}


    /// <summary>
    /// Helper functions for working with Mirror enum.
    /// </summary>
    internal static class BrushMirrorUtility
	{
        /// <summary>
        /// Convert a mirror enum to it's corresponding vector value.
        /// </summary>
        internal static Vector3 ToVector3(this BrushMirror mirror)
		{
			uint m = (uint) mirror;

            bool xMirror = (m & (uint)BrushMirror.X) > 0;
            bool yMirror = (m & (uint)BrushMirror.Y) > 0;
            bool zMirror = (m & (uint)BrushMirror.Z) > 0;

            //out of range
            if(mirror < 0 || ((int)mirror > (int)BrushMirror.X + (int)BrushMirror.Y + (int)BrushMirror.Z))
            {
                return Vector3.one;
            }

            Vector3 reflection = Vector3.one;

			if(xMirror)
				reflection.x = -1f;

			if(yMirror)
				reflection.y = -1f;

			if(zMirror)
				reflection.z = -1f;

			return reflection;
		}
	}
}
