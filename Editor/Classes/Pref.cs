using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    sealed class Pref<T> : UserSetting<T>
    {
        public Pref(string key, T value, SettingsScope scope = SettingsScope.Project)
            : base(PolybrushSettings.instance, key, value, scope)
        { }

        public Pref(Settings settings, string key, T value, SettingsScope scope = SettingsScope.Project)
            : base(settings, key, value, scope) { }
    }
}
