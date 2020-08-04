namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Describes different mesh painting modes.
    /// </summary>
    internal enum PaintMode
	{
		// A brush with radius and falloff.
		Brush,
		// Fill hovered polygons with a uniform color.
		Fill,
		// Flood fill the entire selection
		Flood
	}
}
