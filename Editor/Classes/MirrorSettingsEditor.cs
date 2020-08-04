using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    internal class MirrorSettingsEditor
    {
        static class Styles
        {
            public static readonly GUIContent headerLabel = new GUIContent("Brush Mirroring");
            public static readonly GUIContent[] mirrorSpaces = new GUIContent[]
                    {
                    new GUIContent("World", "Mirror rays in world space"),
                    new GUIContent("Camera", "Mirror rays in camera space")
                    };
            public static readonly string axesFieldTooltip = "Set Brush Mirroring";
            public static readonly string[] axesNameArray = System.Enum.GetNames(typeof(BrushMirror));
        }

        /// <summary>
        /// Mask of active axes.
        /// </summary>
        [UserSetting]
        internal static Pref<BrushMirror> s_MirrorAxes = new Pref<BrushMirror>("Brush.MirrorAxis", BrushMirror.None, SettingsScope.Project);

        /// <summary>
        /// Space coordinate in which the brush ray will be flipped.
        /// </summary>
        [UserSetting]
        internal static Pref<MirrorCoordinateSpace> s_MirrorSpace = new Pref<MirrorCoordinateSpace>("Brush.MirrorSpace", MirrorCoordinateSpace.World, SettingsScope.Project);

        MirrorSettings m_Settings = new MirrorSettings()
        {
            Axes = s_MirrorAxes,
            Space = s_MirrorSpace
        };

        internal MirrorSettings settings
        {
            get { return m_Settings; }
        }

        void RefreshSettings()
        {
            m_Settings.Axes = s_MirrorAxes;
            m_Settings.Space = s_MirrorSpace;
        }

        internal void OnGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                if (PolyGUILayout.HeaderWithDocsLink(Styles.headerLabel))
                    Application.OpenURL(PrefUtility.documentationBrushMirroringLink);

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    s_MirrorAxes.value = (BrushMirror)PolyGUILayout.BitMaskField((uint)s_MirrorAxes.value, Styles.axesNameArray, Styles.axesFieldTooltip);
                    s_MirrorSpace.value = (MirrorCoordinateSpace)GUILayout.Toolbar((int)s_MirrorSpace.value, Styles.mirrorSpaces);
                    if (EditorGUI.EndChangeCheck())
                    {
                        PolybrushSettings.Save();
                        RefreshSettings();
                    }
                }
            }
        }
    }
}
