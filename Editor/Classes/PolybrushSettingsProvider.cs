using System;
using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    static class PolybrushSettingsProvider
    {
        const string k_PreferencesPath = "Preferences/Polybrush";

#if UNITY_2018_3_OR_NEWER
        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new UserSettingsProvider(k_PreferencesPath,
                    PolybrushSettings.instance,
                    new[] { typeof(PolybrushSettingsProvider).Assembly });

            return provider;
        }

#else

        [NonSerialized]
        static UserSettingsProvider s_SettingsProvider;

        [PreferenceItem("Polybrush")]
        static void ProBuilderPreferencesGUI()
        {
            if (s_SettingsProvider == null)
            {
                s_SettingsProvider = new UserSettingsProvider(PolybrushSettings.instance, new[] { typeof(PolybrushSettingsProvider).Assembly });
            }

            s_SettingsProvider.OnGUI(null);
        }
#endif
    }
}
