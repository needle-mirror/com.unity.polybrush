namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Tool enum for brush modes.
    /// </summary>
    internal enum BrushTool
	{
		None = 0,
		RaiseLower = 1,
		Smooth = 2,
		Paint = 3,
		Prefab = 4,
		Texture = 5
	}

    /// <summary>
    /// Utility class for BrushTool enum
    /// </summary>
	internal static class BrushToolUtility
	{
        /// <summary>
        /// Return the Brush Mode type corresponding to a BrushTool enum value
        /// </summary>
        /// <param name="tool"></param>
        /// <returns>The Type of the tool</returns>
		internal static System.Type GetModeType(this BrushTool tool)
		{
			switch(tool)
			{
				case BrushTool.RaiseLower:
					return typeof(BrushModeRaiseLower);

				case BrushTool.Smooth:
					return typeof(BrushModeSmooth);

				case BrushTool.Paint:
					return typeof(BrushModePaint);

				case BrushTool.Prefab:
					return typeof(BrushModePrefab);

				case BrushTool.Texture:
					return typeof(BrushModeTexture);
			}

			return null;
		}
	}
}
