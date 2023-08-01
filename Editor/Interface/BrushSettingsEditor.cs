using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// The default editor for BrushSettings.
    /// </summary>
    [CustomEditor(typeof(BrushSettings))]
	internal class BrushSettingsEditor : Editor
	{
		internal bool showSettingsBounds = false;

        GUIContent m_GCRadius = new GUIContent("Outer Radius", "Radius: The distance from the center of a brush to it's outer edge.\n\nShortcut: 'Ctrl + Mouse Wheel'");
        GUIContent m_GCFalloff = new GUIContent("Inner Radius", "Inner Radius: The distance from the center of a brush at which the strength begins to linearly taper to 0.  This value is normalized, 1 means the entire brush gets full strength, 0 means the very center point of a brush is full strength and the edges are 0.\n\nShortcut: 'Shift + Mouse Wheel'");
        GUIContent m_GCFalloffCurve = new GUIContent("Falloff Curve", "Falloff: Sets the Falloff Curve.");
        GUIContent m_GCStrength = new GUIContent("Strength", "Strength: The effectiveness of this brush.  The actual applied strength also depends on the Falloff setting.\n\nShortcut: 'Ctrl + Shift + Mouse Wheel'");
        GUIContent m_GCRadiusMin = new GUIContent("Brush Radius Min", "The minimum value the brush radius slider can access");
        GUIContent m_GCRadiusMax = new GUIContent("Brush Radius Max", "The maximum value the brush radius slider can access");
        GUIContent m_GCAllowUnclampedFalloff = new GUIContent("Unclamped Falloff", "If enabled, the falloff curve will not be limited to values between 0 and 1.");
        GUIContent m_GCBrushSettingsMinMax = new GUIContent("Brush Radius Min / Max", "Set the minimum and maximum brush radius values");

		private static readonly Rect RECT_ONE = new Rect(0,0,1,1);

        private const float k_BrushSizeMaxValue = 10000f;

		SerializedProperty 	radius,
							falloff,
							strength,
							brushRadiusMin,
							brushRadiusMax,
							brushStrengthMin,
							brushStrengthMax,
							curve,
							allowNonNormalizedFalloff;

		internal void OnEnable()
		{
			/// User settable
			radius = serializedObject.FindProperty("_radius");
			falloff = serializedObject.FindProperty("_falloff");
			curve = serializedObject.FindProperty("_curve");
			strength = serializedObject.FindProperty("_strength");

			/// Bounds
			brushRadiusMin = serializedObject.FindProperty("brushRadiusMin");
			brushRadiusMax = serializedObject.FindProperty("brushRadiusMax");
			allowNonNormalizedFalloff = serializedObject.FindProperty("allowNonNormalizedFalloff");
		}

		private bool approx(float lhs, float rhs)
		{
			return Mathf.Abs(lhs-rhs) < .0001f;
		}

		public override void OnInspectorGUI()
		{
            serializedObject.Update();

            // Manually show the settings header in PolyEditor so that the preset selector can be included in the block
            // if(PolyGUILayout.HeaderWithDocsLink(PolyGUI.TempContent("Brush Settings")))
            // 	Application.OpenURL("http://procore3d.github.io/polybrush/brushSettings/");

            showSettingsBounds = PolyGUILayout.Foldout(showSettingsBounds, m_GCBrushSettingsMinMax);

            if (showSettingsBounds)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        brushRadiusMin.floatValue = PolyGUILayout.FloatField(m_GCRadiusMin, brushRadiusMin.floatValue);
                        brushRadiusMin.floatValue = Mathf.Clamp(brushRadiusMin.floatValue, .0001f, k_BrushSizeMaxValue);

                        brushRadiusMax.floatValue = PolyGUILayout.FloatField(m_GCRadiusMax, brushRadiusMax.floatValue);
                        brushRadiusMax.floatValue = Mathf.Clamp(brushRadiusMax.floatValue, brushRadiusMin.floatValue + .001f, k_BrushSizeMaxValue);

                        allowNonNormalizedFalloff.boolValue = PolyGUILayout.Toggle(m_GCAllowUnclampedFalloff, allowNonNormalizedFalloff.boolValue);
                    }
                }
            }

            radius.floatValue = PolyGUILayout.FloatFieldWithSlider(m_GCRadius, radius.floatValue, brushRadiusMin.floatValue, brushRadiusMax.floatValue);
            falloff.floatValue = PolyGUILayout.FloatFieldWithSlider(m_GCFalloff, falloff.floatValue, 0f, 1f);
            strength.floatValue = PolyGUILayout.FloatFieldWithSlider(m_GCStrength, strength.floatValue, 0f, 1f);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(m_GCFalloffCurve, GUILayout.Width(100));

                if (allowNonNormalizedFalloff.boolValue)
                    curve.animationCurveValue = EditorGUILayout.CurveField(curve.animationCurveValue, GUILayout.MinHeight(22));
                else
                    curve.animationCurveValue = EditorGUILayout.CurveField(curve.animationCurveValue, Color.green, RECT_ONE, GUILayout.MinHeight(22));
            }

            Keyframe[] keys = curve.animationCurveValue.keys;

            if ((approx(keys[0].time, 0f) && approx(keys[0].value, 0f) && approx(keys[1].time, 1f) && approx(keys[1].value, 1f)))
            {
                Keyframe[] rev = new Keyframe[keys.Length];

                for (int i = 0; i < keys.Length; i++)
                    rev[keys.Length - i - 1] = new Keyframe(1f - keys[i].time, keys[i].value, -keys[i].outTangent, -keys[i].inTangent);

                curve.animationCurveValue = new AnimationCurve(rev);
            }
            serializedObject.ApplyModifiedProperties();

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Create a New BrushSettings Asset
        /// </summary>
        /// <returns>the newly created BrushSettings</returns>
		internal static BrushSettings AddNew(BrushSettings prevSettings = null)
        {
            string path = PolyEditorUtility.UserAssetDirectory + "Brush Settings";

			if(string.IsNullOrEmpty(path))
				path = "Assets";

			path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Brush.asset");

			if(!string.IsNullOrEmpty(path))
			{
				BrushSettings settings = ScriptableObject.CreateInstance<BrushSettings>();
                if (prevSettings != null) {
                    string name = settings.name;
                    prevSettings.CopyTo(settings);
                    settings.name = name;	// want to retain the unique name generated by AddNew()
                }
                else
                {
                    settings.SetDefaultValues();
                }

				AssetDatabase.CreateAsset(settings, path);
				AssetDatabase.Refresh();

				EditorGUIUtility.PingObject(settings);

				return settings;
			}

			return null;
		}

        static internal BrushSettings LoadBrushSettingsAssets(string guid)
        {
            BrushSettings settings;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            settings = AssetDatabase.LoadAssetAtPath<BrushSettings>(path);
            return settings;
        }

        static internal BrushSettings[] GetAvailableBrushes()
        {
            List<BrushSettings> brushes = PolyEditorUtility.GetAll<BrushSettings>();

            if (brushes.Count < 1)
                brushes.Add(PolyEditorUtility.GetFirstOrNew<BrushSettings>());

            return brushes.ToArray();
        }
    }
}
