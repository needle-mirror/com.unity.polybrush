using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Editor preferences and defaults.
    /// </summary>
    internal static class PrefUtility
	{
        internal const string productName                           = "Polybrush";

        internal const string documentationLink                     = "https://unity-technologies.github.io/procore-legacy-docs/polybrush/polybrush-gh-pages";
	    internal const string documentationSettingsLink             = documentationLink + "/settings/";
	    internal const string documentationBrushSettingsLink        = documentationLink + "/brushSettings/";
	    internal const string documentationBrushMirroringLink       = documentationLink + "/brushMirroring/";
	    internal const string documentationPrefabPlacementBrushLink = documentationLink + "/modes/place/";
	    internal const string documentationColorBrushLink           = documentationLink + "/modes/color/";
	    internal const string documentationSculptBrushLink          = documentationLink + "/modes/sculpt/";
	    internal const string documentationSmoothBrushLink          = documentationLink + "/modes/smooth/";
	    internal const string documentationTextureBrushLink         = documentationLink + "/modes/texture/";

        internal const string contactLink                           = "mailto:contact@procore3d.com";
        internal const string websiteLink                           = "http://www.procore3d.com";

        internal const string POLYBRUSH_VERSION                     = "0.9.9b2";

        public const int menuEditor = 200;
	    public const int menuBakeVertexStreams = 300;

        /// <summary>
        /// Check if the last opened version of Polybrush matches this one.
        /// </summary>
        /// <returns>Returns true if matches, false otherwise.</returns>
        internal static bool VersionCheck()
		{
			if( !EditorPrefs.GetString("pref_version", "null").Equals(PrefUtility.POLYBRUSH_VERSION) )
			{
				EditorPrefs.SetString("pref_version", PrefUtility.POLYBRUSH_VERSION);
				return false;
			}
			return true;
		}

        internal static void ClearPrefs()
        {
            Settings settings = PolybrushSettings.instance;
            ISettingsRepository projectRepository = settings.GetRepository(SettingsScope.Project);
        }
    }
}
