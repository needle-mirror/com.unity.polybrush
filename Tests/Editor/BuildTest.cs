using NUnit.Framework;
using UnityEditor;
using System.IO;
using UnityEngine.TestTools;

namespace UnityEngine.Polybrush.EditorTests
{
    public class BuildTest
    {
        static readonly string k_ScenesDestinationFolderPath = Application.dataPath + "/Scenes";
        static readonly string k_TestScene1FilePath = TestsUtility.testsRootDirectory + "/Scenes/TestSceneWithPolybrushObject.unity";
        static readonly string k_TestScene1DestinationFilePath = Application.dataPath + "/Scenes/TestSceneWithPolybrushObject.unity";
        static readonly string k_BuildFolder = Application.dataPath + "/../Builds";
        static readonly string k_BuildName = "Test.exe";
        static readonly string[] k_ScenesForBuildSettings = new string[]
        {
            "Assets/Scenes/TestSceneWithPolybrushObject.unity"
        };

        [SetUp]
        public void Setup()
        {
            DeleteBuildDirectory();

            if (!Directory.Exists(k_ScenesDestinationFolderPath))
                Directory.CreateDirectory(k_ScenesDestinationFolderPath);

            FileUtil.CopyFileOrDirectory(k_TestScene1FilePath, k_TestScene1DestinationFilePath);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            FileUtil.DeleteFileOrDirectory(k_TestScene1DestinationFilePath);
            FileUtil.DeleteFileOrDirectory(k_TestScene1DestinationFilePath + ".meta");
            DeleteBuildDirectory();
            AssetDatabase.Refresh();
        }

        //[Test]
        [UnityPlatform(new RuntimePlatform[] { RuntimePlatform.WindowsEditor })]
        public void BuildSceneWithPolybrushObjectOnWindows_Success()
        {
            BuildPlayerOptions options = GenerateBuildOptionsForAllPlatforms();
            options.target = BuildTarget.StandaloneWindows64;
            BuildPipeline.BuildPlayer(options);
        }

        //[Test]
        [UnityPlatform(new RuntimePlatform[] { RuntimePlatform.OSXEditor })]
        public void BuildSceneWithPolybrushObjectOnOSX_Success()
        {
            BuildPlayerOptions options = GenerateBuildOptionsForAllPlatforms();
            options.target = BuildTarget.StandaloneOSX;
            BuildPipeline.BuildPlayer(options);
        }


        private BuildPlayerOptions GenerateBuildOptionsForAllPlatforms()
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = k_ScenesForBuildSettings;
            options.locationPathName = k_BuildFolder + "/" + k_BuildName;

            return options;
        }

        private void DeleteBuildDirectory()
        {
            if (Directory.Exists(k_BuildFolder))
                Directory.Delete(k_BuildFolder, true);
        }
    }
}
