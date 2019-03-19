using UnityEditor;

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
    }
}
