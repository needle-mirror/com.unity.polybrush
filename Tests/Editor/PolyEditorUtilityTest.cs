using UnityEngine.TestTools;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Polybrush;

namespace UnityEngine.Polybrush.EditorTests
{
    public class PolyEditorUtilityTest : IPrebuildSetup, IPostBuildCleanup
    {
        private GameObject _testGameObject, _childTestGameObject, _prefab;
        private static string _prefabPath;
        private const string path = "Assets/Utility/";

#pragma warning disable 0618
        public static string CreateShaderMetadataTest(string shaderName)
        {
            //load a polybrush shader
            Shader shaderToTest = Shader.Find(shaderName);
            Assert.IsNotNull(shaderToTest);

            //create base meta data
            AttributeLayout[] attributes = new AttributeLayout[]
            {
                new AttributeLayout(MeshChannel.Color, ComponentIndex.R, Vector2.up, 0, "_Texture1"),
                new AttributeLayout(MeshChannel.Color, ComponentIndex.G, Vector2.up, 0, "_Texture2"),
                new AttributeLayout(MeshChannel.Color, ComponentIndex.B, Vector2.up, 0, "_Texture3"),
                new AttributeLayout(MeshChannel.Color, ComponentIndex.A, Vector2.up, 0, "_Texture4"),
            };


            //call the function that will create the asset containing meta data
            string path = ShaderMetaDataUtility.SaveMeshAttributesData(shaderToTest, attributes, true);
            return path;
        }
#pragma warning restore 0618

        public void Setup()
        {
            //ensure directory existence
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //create a gameobject from a primitive and a prefab from it
            GameObject testGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testGameObject.name = "TestGameObject";
            GameObject child = new GameObject("Child");
            child.transform.SetParent(testGameObject.transform, false);

            //mesh asset creation
            Mesh mesh = Object.Instantiate(testGameObject.GetComponent<MeshFilter>().sharedMesh);
            mesh.name = "MeshTest";
            AssetDatabase.CreateAsset(mesh, path + "MeshTest.asset");
            testGameObject.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath(path + "MeshTest.asset", typeof(Mesh)) as Mesh;

            _prefabPath = path + testGameObject.name + ".prefab";

            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(testGameObject, _prefabPath, InteractionMode.AutomatedAction);

            //prevents leak?
            Object.DestroyImmediate(testGameObject);
        }

        [SetUp]
        public void SetUp()
        {
            //retrieve a ref from the prefab created at the begining
            _prefab = AssetDatabase.LoadAssetAtPath(_prefabPath, typeof(GameObject)) as GameObject;
            _testGameObject = Object.Instantiate(_prefab);
            _childTestGameObject = _testGameObject.transform.GetChild(0).gameObject;
        }

        [TearDown]
        public void TearDown()
        {
            //cleanup
            UnityEngine.Object.DestroyImmediate(_testGameObject);
        }

        /// <summary>
        /// Warning, not called in 2018.2.0f2 > 2018.2.2f1, only caleed on playmode unit tests (issue may be fixed in further unity version, will let the prefab on the asset folder right now
        /// </summary>
        public void Cleanup()
        {
            //destroy
            AssetDatabase.DeleteAsset(_prefabPath);
        }

        /// <summary>
        /// Will test InSelection function
        /// </summary>
        [Test]
        public void InSelection()
        {
            //null checks
            Assert.DoesNotThrow(() => PolyEditorUtility.InSelection(default(GameObject)));

            //no selection test
            Selection.activeObject = null;
            Assert.IsFalse(PolyEditorUtility.InSelection(_testGameObject));

            //selection test
            Selection.activeObject = _testGameObject;
            Assert.IsTrue(PolyEditorUtility.InSelection(_testGameObject));

            //child selection test
            Assert.IsFalse (PolyEditorUtility.InSelection(_childTestGameObject));
        }

        [Test]
        public void GetMeshGUID()
        {
            //null checks
            Assert.DoesNotThrow(() =>
            {
                ModelSource errorModelSource = PolyEditorUtility.GetMeshGUID(null);
                Assert.AreEqual(errorModelSource, ModelSource.Error);
            });

            //getting a gameobject with a mesh asset linked into it
            Mesh mesh = _testGameObject.GetComponent<MeshFilter>().sharedMesh;
            string guidExpected = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mesh));
            string guidResult = string.Empty;
            ModelSource modelSource = PolyEditorUtility.GetMeshGUID(mesh, ref guidResult);

            //should return an GUID because it's an asset
            Assert.IsFalse(string.IsNullOrEmpty(guidResult));
            //should be equal to the GUID extracted from the asset
            Assert.IsTrue(string.Compare(guidExpected, guidResult) == 0);
            //should return Asset modelsource
            Assert.AreEqual(modelSource, ModelSource.Asset);

            //test non asset mesh
            Mesh duplicatedMesh = Object.Instantiate(mesh);
            ModelSource duplicatedModelSource = PolyEditorUtility.GetMeshGUID(duplicatedMesh);
            Assert.AreEqual(duplicatedModelSource, ModelSource.Scene);
        }

        private static void RemoveAllGenericTypeAssets()
        {
            //get rid first of all previously created data
            string[] allGUIDs = AssetDatabase.FindAssets("t:" + typeof(UnitTestGenericType));
            foreach (string guid in allGUIDs)
            {
                UnitTestGenericType genericObject = AssetDatabase.LoadAssetAtPath<UnitTestGenericType>(AssetDatabase.GUIDToAssetPath(guid));
                if (genericObject != null)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                }
            }
        }

        [Test]
        public void GetFirstOrNew()
        {
            RemoveAllGenericTypeAssets();

            //make sure that there is more asset of that type on the project
            string[] allGUIDs = AssetDatabase.FindAssets("t:" + typeof(UnitTestGenericType));
            Assert.IsTrue(allGUIDs == null || allGUIDs.Length == 0);

            //the function will create a new asset with the type passed in the generic parameter
            UnitTestGenericType unitTestGenericType = PolyEditorUtility.GetFirstOrNew<UnitTestGenericType>();

            //test it again, should return 1 value
            allGUIDs = AssetDatabase.FindAssets("t:" + typeof(UnitTestGenericType));
            Assert.IsTrue(allGUIDs != null && allGUIDs.Length == 1);
            UnitTestGenericType result = AssetDatabase.LoadAssetAtPath<UnitTestGenericType>(AssetDatabase.GUIDToAssetPath(allGUIDs[0]));
            Assert.IsNotNull(result);

            //call it one more time, ensure it will return the same result
            UnitTestGenericType secondResult = AssetDatabase.LoadAssetAtPath<UnitTestGenericType>(AssetDatabase.GUIDToAssetPath(allGUIDs[0]));
            Assert.AreEqual(result, secondResult);

            //cleanup
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(allGUIDs[0]));
        }

        [Test]
        public void GetAll()
        {
            RemoveAllGenericTypeAssets();

            //create two assets
            UnitTestGenericType asset1 = ScriptableObject.CreateInstance<UnitTestGenericType>();
            UnitTestGenericType asset2 = ScriptableObject.CreateInstance<UnitTestGenericType>();
            EditorUtility.SetDirty(asset1);
            EditorUtility.SetDirty(asset2);
            AssetDatabase.CreateAsset(asset1, path + "UnitTestGenericType1.asset");
            AssetDatabase.CreateAsset(asset2, path + "UnitTestGenericType2.asset");
            //check if existing
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<UnitTestGenericType>(path + "UnitTestGenericType1.asset"));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<UnitTestGenericType>(path + "UnitTestGenericType2.asset"));

            //now load them with the utility function
            List<UnitTestGenericType> allAssets = PolyEditorUtility.GetAll<UnitTestGenericType>();
            Assert.IsNotNull(allAssets);
            Assert.IsTrue(allAssets.Count == 2);

            //cleanup
            AssetDatabase.DeleteAsset(path + "UnitTestGenericType1.asset");
            AssetDatabase.DeleteAsset(path + "UnitTestGenericType2.asset");
        }
    }
}
