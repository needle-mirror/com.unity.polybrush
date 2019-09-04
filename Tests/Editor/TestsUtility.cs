using System.IO;

#if UNITY_2019_2_OR_NEWER
using System.Reflection;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif


namespace UnityEngine.Polybrush.EditorTests
{
    public class TestsUtility
    {
        public static string k_PackagePath = "Packages/com.unity.polybrush/Tests";
        public static string k_PackageTestsFullPath = "Packages/com.unity.polybrush.tests/Tests";

        public static string testsRootDirectory
        {
            get
            {
#if UNITY_2019_2_OR_NEWER
                var packageName = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()).name;
                return "Packages/" + packageName + "/Tests";
#else
                if (Directory.Exists(k_PackagePath))
                    return k_PackagePath;
                return k_PackageTestsFullPath;
#endif
            }
        }
    }
}
