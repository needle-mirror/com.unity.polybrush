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
            if (ProBuilderBridge.ProBuilderExists())
                return ProBuilderBridge.IsValidProBuilderMesh(gameObject);
            return false;
        }
    }
}
