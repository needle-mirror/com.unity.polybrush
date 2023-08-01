namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Describes the origin of a mesh.
    /// </summary>
    [System.Flags]
	internal enum SelectionRenderState
	{
		None = 0x0,
		Wireframe = 0x1,
		Outline = 0x2,
	}
}
