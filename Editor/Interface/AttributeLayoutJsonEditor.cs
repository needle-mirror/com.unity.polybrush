using UnityEngine;
using System.Linq;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Custom inspector for .pbs.json files.
    /// </summary>
    [CustomEditor(typeof(TextAsset), true)]
	internal class AttributeLayoutJsonEditor : Editor
	{
		[SerializeField] AttributeLayoutContainer container = null;
		[SerializeField] Editor editor = null;
		[SerializeField] bool modified = false;

		[MenuItem("Assets/Create/Polybrush/Shader Metadata", true, 50)]
		static bool VerifyCreateShaderMetaData()
		{
			return Selection.objects.Any(x => x != null && x is Shader);
		}

        /// <summary>
        /// Creates a New Shader MetaData File
        /// </summary>
		[MenuItem("Assets/Create/Polybrush/Shader Metadata", false, 50)]
		static void CreateShaderMetaData()
		{
			string path = "";

			foreach(Shader shader in Selection.objects)
			{
				AttributeLayout[] attributes = new AttributeLayout[]
				{
					new AttributeLayout(MeshChannel.Color, ComponentIndex.R, Vector2.up, 0, "_Texture1"),
					new AttributeLayout(MeshChannel.Color, ComponentIndex.G, Vector2.up, 0, "_Texture2"),
					new AttributeLayout(MeshChannel.Color, ComponentIndex.B, Vector2.up, 0, "_Texture3"),
					new AttributeLayout(MeshChannel.Color, ComponentIndex.A, Vector2.up, 0, "_Texture4"),
				};
#pragma warning disable 0618
                path = ShaderMetaDataUtility.SaveMeshAttributesData(shader, attributes, true);
#pragma warning restore 0618
            }

            AssetDatabase.Refresh();

			TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

			if(asset != null)
				EditorGUIUtility.PingObject(asset);
		}

		private void ReloadJson()
		{
			editor = null;
			container = null;
			modified = false;
			AssetDatabase.Refresh();
		}

		public override void OnInspectorGUI()
		{
			TextAsset asset = target as TextAsset;

			if( asset == null || !asset.name.EndsWith(".pbs") )
			{
				// sfor whatever reason this doesn't work
				// DrawDefaultInspector();
				DrawTextAssetInspector();

				return;
			}

			if(editor == null)
			{
#pragma warning disable 0618
                ShaderMetaDataUtility.TryReadAttributeLayoutsFromJson(asset.text, out container);
#pragma warning disable 0618
                editor = Editor.CreateEditor(container);
			}

			GUI.enabled = true;

			EditorGUI.BeginChangeCheck();

			editor.OnInspectorGUI();

			if( EditorGUI.EndChangeCheck() )
				modified = true;

			GUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();

			GUI.enabled = modified;

			if(GUILayout.Button("Revert"))
				ReloadJson();

			if(GUILayout.Button("Apply"))
			{
			    ShaderMetaDataUtility.SaveMeshAttributesData(container, true);
				ReloadJson();
			}

			GUILayout.EndHorizontal();

			GUI.enabled = false;
		}

		private static GUIStyle m_TextStyle;

        /// <summary>
        /// Copy/paste of Unity TextAssetInspector, since DrawDefaultInspector doesn't work with TextAssets.
        /// Not using reflection because this is such a small function that it makes more sense to just c/p
        /// and avoid the issues of Unity possibly changing names or signatures in the future. 
        /// </summary>
        private void DrawTextAssetInspector()
		{
			if (m_TextStyle == null)
				m_TextStyle = "ScriptText";

			bool enabled = GUI.enabled;
			GUI.enabled = true;

			TextAsset textAsset = this.target as TextAsset;

			if (textAsset != null)
			{
				string text;

				if (base.targets.Length > 1)
				{
					text = string.Format("{0} Text Assets", base.targets.Length);
				}
				else
				{
					text = textAsset.ToString();
					if (text.Length > 7000)
					{
						text = text.Substring(0, 7000) + "...\n\n<...etc...>";
					}
				}
				Rect rect = GUILayoutUtility.GetRect(PolyGUI.TempContent(text), m_TextStyle);
				rect.x = 0f;
				rect.y -= 3f;
				rect.width = EditorGUIUtility.currentViewWidth - 1; // GUIClip.visibleRect.width + 1f;
				GUI.Box(rect, text, m_TextStyle);
			}
			GUI.enabled = enabled;
		}
	}
}
