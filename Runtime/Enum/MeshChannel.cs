
namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Mesh property map.
    /// </summary>
    [System.Flags]
	internal enum MeshChannel
	{
		Null		= 0x0,
		Position	= 0x1,
		Normal		= 0x2,
		Color 		= 0x4,
		Tangent 	= 0x8,
		UV0 		= 0x10,
		UV2 		= 0x20,
		UV3 		= 0x40,
		UV4 		= 0x80,
		All			= 0xFF
	};

    /// <summary>
    /// Helper methods for working with MeshChannel
    /// </summary>
	internal static class MeshChannelUtility
	{
        /// <summary>
        /// Try to convert a MeshChannel value into it's corresponding UV index 
        /// </summary>
        /// <param name="channel">value to be converted</param>
        /// <returns>Corresponding index if applicable, -1 otherwise</returns>
		internal static int UVChannelToIndex(MeshChannel channel)
		{
			if(channel == MeshChannel.UV0)
				return 0;
			else if(channel == MeshChannel.UV2)
				return 1;
			else if(channel == MeshChannel.UV3)
				return 2;
			else if(channel == MeshChannel.UV4)
				return 3;

			return -1;
		}
	}
}
