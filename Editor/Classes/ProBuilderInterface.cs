#define PROBUILDER_4_0_OR_NEWER

using UnityEngine;

namespace UnityEditor.Polybrush
{
	/// <summary>
	/// Static helper methods for working with reflection.  Mostly used for ProBuilder compatibility.
	/// </summary>
	static class ProBuilderInterface
	{
		/// <summary>
		/// Tests if a GameObject is a ProBuilder mesh or not.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		internal static bool IsProBuilderObject(GameObject gameObject)
        {
#if PROBUILDER_4_0_OR_NEWER
            if (ProBuilderBridge.ProBuilderExists())
                return ProBuilderBridge.IsValidProBuilderMesh(gameObject);
            return false;
#endif
        }
    }
}
