using Polybrush;
using UnityEditor;
using UnityEngine;
using UnityEngine.Polybrush;
using UnityEngine.SceneManagement;

namespace UnityEditor.Polybrush
{
    static class MenuItems
    {
        static PolyEditor editor
        {
            get { return PolyEditor.instance; }
        }

        [MenuItem("Tools/" + PrefUtility.productName + "/Polybrush Window %#v", false, PrefUtility.menuEditor)]
        public static void MenuInitEditorWindow()
        {
            EditorWindow.GetWindow<PolyEditor>(PolyEditor.s_FloatingWindow).Show();
        }

        [MenuItem("Tools/" + PrefUtility.productName + "/Bake Vertex Streams", false, PrefUtility.menuBakeVertexStreams)]
        public static void Init()
        {
            EditorWindow.GetWindow<BakeAdditionalVertexStreams>(true, "Bake Vertex Streams", true);
        }

        [MenuItem("Tools/Polybrush/Next Brush", true, 100)]
        static bool VerifyCycleBrush()
        {
            return editor != null;
        }

#pragma warning disable 612
        [MenuItem("Tools/Polybrush/Update Z_AdditionalVertexStreams")]
        static void Convert()
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene s = SceneManager.GetSceneAt(i);
                foreach (GameObject root in s.GetRootGameObjects())
                {
                    foreach (z_AdditionalVertexStreams item in root.GetComponentsInChildren<z_AdditionalVertexStreams>(includeInactive: true))
                        PolyEditorUtility.ConvertGameObjectToNewFormat(item);
                }
            }
        }
#pragma warning restore 612


        [MenuItem("Tools/" + PrefUtility.productName + "/Update Shader Meta", false)]
        static void UpdateShaderMetaToNewFormat()
        {
            foreach (Shader s in Selection.objects)
                ShaderMetaDataUtility.ConvertMetaDataToNewFormat(s);
        }
#pragma warning restore 612

        [MenuItem("Tools/" + PrefUtility.productName + "/Update Shader Meta", true)]
        static bool ValidateUpdateShaderMetaToNewFormat()
        {
            foreach (Object s in Selection.objects)
            {
                if (!(s is Shader))
                    return false;
            }
            return true;
        }
    }
}
