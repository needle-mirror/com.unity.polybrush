namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Describes the origin of a mesh.
    /// </summary>
    internal enum ModelSource
	{
		Imported = 0x0,
		Asset = 0x1,
		Scene = 0x2,
		AdditionalVertexStreams = 0x3,
        Error = 0x4
	}
}
