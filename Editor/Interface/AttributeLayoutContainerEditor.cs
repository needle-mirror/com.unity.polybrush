using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Custom Editor for AttributeLayoutContainer
    /// </summary>
	[CustomEditor(typeof(AttributeLayoutContainer), true)]
	internal class AttributeLayoutContainerEditor : Editor
	{
		private static readonly Color LIGHT_GRAY = new Color(.13f, .13f, .13f, .3f);
		private static readonly Color DARK_GRAY = new Color(.3f, .3f, .3f, .3f);

		SerializedProperty p_attributes;

	    Shader shader;

		void OnEnable()
		{
			if(target == null)
			{
				GameObject.DestroyImmediate(this);
				return;
			}

		    AttributeLayoutContainer container = target as AttributeLayoutContainer;
		    shader = container.shader;

			p_attributes = serializedObject.FindProperty("attributes");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

		    EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false);

            for (int i = 0; i < p_attributes.arraySize; i++)
			{
				SerializedProperty attrib = p_attributes.GetArrayElementAtIndex(i);
				
				GUI.backgroundColor = i % 2 == 0 ? LIGHT_GRAY : DARK_GRAY;
				GUILayout.BeginVertical(PolyGUI.BackgroundColorStyle);
				GUI.backgroundColor = Color.white;

				SerializedProperty target = attrib.FindPropertyRelative("propertyTarget");
				SerializedProperty channel = attrib.FindPropertyRelative("channel");
				SerializedProperty index = attrib.FindPropertyRelative("index");
				SerializedProperty range = attrib.FindPropertyRelative("range");
				SerializedProperty mask = attrib.FindPropertyRelative("mask");

				EditorGUILayout.PropertyField(target);
				EditorGUILayout.PropertyField(channel);
				EditorGUILayout.IntPopup(index, ComponentIndexUtility.ComponentIndexPopupDescriptions, ComponentIndexUtility.ComponentIndexPopupValues);
				
				bool old = EditorGUIUtility.wideMode;
				EditorGUIUtility.wideMode = true;
				EditorGUILayout.PropertyField(range);
				EditorGUIUtility.wideMode = old;

				EditorGUILayout.IntPopup(mask, AttributeLayout.DefaultMaskDescriptions, AttributeLayout.DefaultMaskValues, PolyGUI.TempContent("Group"));

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete", EditorStyles.miniButton))
                    p_attributes.DeleteArrayElementAtIndex(i);

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

			if(GUILayout.Button("Add Attribute"))
				p_attributes.arraySize++;

			serializedObject.ApplyModifiedProperties();
		}
	}
}
