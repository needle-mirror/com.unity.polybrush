using UnityEngine;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Interface for objects that contain a set of default values.
    /// </summary>
    internal interface IHasDefault
	{
        /// <summary>
        /// Set this object to use default values.
        /// </summary>
        void SetDefaultValues();
	}
}
