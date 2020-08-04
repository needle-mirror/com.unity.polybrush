using UnityEngine;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Interface for objects that contain some Custom Settings 
    /// </summary>
    internal interface ICustomSettings
    {
        /// <summary>
        /// The folder where assets of `ICustomSettings` should be saved
        /// </summary>
        /// <returns>The name of the custom folder</returns>
        string assetsFolder { get;}
    }
}
