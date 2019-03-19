using UnityEditor;
using UnityEngine;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Settings placement like rotation and scale for prefab painting
    /// </summary>
	[System.Serializable]
	internal class PlacementSettings
	{
        [SerializeField] private float m_Strength;

        // Random Rotation and Scale Ranges
        [SerializeField] private Vector3 m_RotationRangeMin;
        [SerializeField] private Vector3 m_RotationRangeMax;
        [SerializeField] private Vector3 m_ScaleRangeMin;
        [SerializeField] private Vector3 m_ScaleRangeMax;
        [SerializeField] private Vector2 m_UniformScale;

        // Activate or desactivate certain settings
        [SerializeField] private bool m_UniformBool;
        [SerializeField] private bool m_XRotationBool;
        [SerializeField] private bool m_YRotationBool;
        [SerializeField] private bool m_ZRotationBool;
        [SerializeField] private bool m_XScaleBool;
        [SerializeField] private bool m_YScaleBool;
        [SerializeField] private bool m_ZScaleBool;

        public float strength { get { return m_Strength; } set { m_Strength = value; } }

        public Vector3 rotationRangeMin { get { return m_RotationRangeMin; } set { m_RotationRangeMin = value; } }
        public Vector3 rotationRangeMax { get { return m_RotationRangeMax; } set { m_RotationRangeMax = value; } }
        public Vector3 scaleRangeMin { get { return m_ScaleRangeMin; } set { m_ScaleRangeMin = value; } }
        public Vector3 scaleRangeMax { get { return m_ScaleRangeMax; } set { m_ScaleRangeMax = value; } }
        public Vector3 uniformScale { get { return m_UniformScale; } set { m_UniformScale = value; } }

        public bool uniformBool { get { return m_UniformBool; } set { m_UniformBool = value; } }
        public bool xRotationBool { get { return m_XRotationBool; } set { m_XRotationBool = value; } }
        public bool yRotationBool { get { return m_YRotationBool; } set { m_YRotationBool = value; } }
        public bool zRotationBool { get { return m_ZRotationBool; } set { m_ZRotationBool = value; } }
        public bool xScaleBool { get { return m_XScaleBool; } set { m_XScaleBool = value; } }
        public bool yScaleBool { get { return m_YScaleBool; } set { m_YScaleBool = value; } }
        public bool zScaleBool { get { return m_ZScaleBool; } set { m_ZScaleBool = value; } }

        internal PlacementSettings()
        {
            uniformBool = true;
            strength = 100;
            rotationRangeMin = new Vector3(0f, 0f, 0f);
            rotationRangeMax = new Vector3(0f, 0f, 0f);
            scaleRangeMin = new Vector3(1f, 1f, 1f);
            scaleRangeMax = new Vector3(1f, 1f, 1f);
            uniformScale = new Vector2(1f, 1f);
            xRotationBool = false;
            yRotationBool = false;
            zRotationBool = false;
            xScaleBool = false;
            yScaleBool = false;
            zScaleBool = false;
        }

        internal static void PopulateSerializedProperty(SerializedProperty prop)
        {
            prop.FindPropertyRelative("m_Strength").floatValue = 50.0f;
            prop.FindPropertyRelative("m_RotationRangeMin").vector3Value = new Vector3(0f, 0f, 0f);
            prop.FindPropertyRelative("m_RotationRangeMax").vector3Value = new Vector3(0f, 0f, 0f);
            prop.FindPropertyRelative("m_ScaleRangeMin").vector3Value = new Vector3(1f, 1f, 1f);
            prop.FindPropertyRelative("m_ScaleRangeMax").vector3Value = new Vector3(1f, 1f, 1f);
            prop.FindPropertyRelative("m_UniformScale").vector2Value = new Vector3(1f, 1f);
        }

        internal static void CopySerializedProperty(SerializedProperty src, SerializedProperty dest)
        {
            var strength = src.FindPropertyRelative("m_Strength").floatValue;
            var rotationRangeMin = src.FindPropertyRelative("m_RotationRangeMin").vector3Value;
            var rotationRangeMax = src.FindPropertyRelative("m_RotationRangeMax").vector3Value;
            var scaleRangeMin = src.FindPropertyRelative("m_ScaleRangeMin").vector3Value;
            var scaleRangeMax = src.FindPropertyRelative("m_ScaleRangeMax").vector3Value;
            var uniformScale = src.FindPropertyRelative("m_UniformScale").vector2Value;
            var xRotationBool = src.FindPropertyRelative("m_XRotationBool").boolValue;
            var yRotationBool = src.FindPropertyRelative("m_YRotationBool").boolValue;
            var zRotationBool = src.FindPropertyRelative("m_ZRotationBool").boolValue;
            var xScaleBool = src.FindPropertyRelative("m_XScaleBool").boolValue;
            var yScaleBool = src.FindPropertyRelative("m_YScaleBool").boolValue;
            var zScaleBool = src.FindPropertyRelative("m_ZScaleBool").boolValue;
            var uniformBool = src.FindPropertyRelative("m_UniformBool").boolValue;
           
            dest.FindPropertyRelative("m_Strength").floatValue = strength;
            dest.FindPropertyRelative("m_RotationRangeMin").vector3Value = rotationRangeMin;
            dest.FindPropertyRelative("m_RotationRangeMax").vector3Value = rotationRangeMax;
            dest.FindPropertyRelative("m_ScaleRangeMin").vector3Value = scaleRangeMin;
            dest.FindPropertyRelative("m_ScaleRangeMax").vector3Value = scaleRangeMax;
            dest.FindPropertyRelative("m_UniformScale").vector2Value = uniformScale;
            dest.FindPropertyRelative("m_XRotationBool").boolValue = xRotationBool;
            dest.FindPropertyRelative("m_YRotationBool").boolValue = yRotationBool;
            dest.FindPropertyRelative("m_ZRotationBool").boolValue = zRotationBool;
            dest.FindPropertyRelative("m_XScaleBool").boolValue = xScaleBool;
            dest.FindPropertyRelative("m_YScaleBool").boolValue = yScaleBool;
            dest.FindPropertyRelative("m_ZScaleBool").boolValue = zScaleBool;
            dest.FindPropertyRelative("m_UniformBool").boolValue = uniformBool;
        }       
    }

    /// <summary>
    /// A prefab object with some placement settings for prefab painting
    /// </summary>
	[System.Serializable]
	internal class PrefabAndSettings
	{
        [SerializeField] internal GameObject gameObject;
        [SerializeField] internal PlacementSettings settings;
        [SerializeField] internal string name;

        internal PrefabAndSettings(GameObject go)
		{
			gameObject = go;
            name = go.name;
            settings = new PlacementSettings();
        }
    }
}
