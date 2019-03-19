using UnityEngine;

#if PROBUILDER_4_0_OR_NEWER
using UnityEngine.ProBuilder;
#endif

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
            return gameObject.GetComponent<ProBuilderMesh>() != null;
#else
            return false;
#endif
        }
    }
}
