using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEditor.Polybrush;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace UnityEngine.Polybrush.EditorTests
{
    public class PolySceneUtilityTest
    {
        private static string _prefabPath;
        private const string k_TestsFolderRelativePath = "Assets/Templates/PolySceneUtility/";
        private static string k_TestsFolderFullPath = Application.dataPath + "/Templates/PolySceneUtility/";

        private GameObject _testGameObject;

        [OneTimeSetUp]
        public void SetUp()
        {
            if (!Directory.Exists(k_TestsFolderFullPath))
                Directory.CreateDirectory(k_TestsFolderFullPath);

            //create a cube to ensure consistent and easier results to test with functions like WorldRaycast
            GameObject testGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testGameObject.name = "PolySceneUtilityTest";

            _prefabPath = k_TestsFolderRelativePath + testGameObject.name + ".prefab";

            PrefabUtility.SaveAsPrefabAssetAndConnect(testGameObject, _prefabPath, InteractionMode.AutomatedAction);

            GameObject prefab = AssetDatabase.LoadAssetAtPath(_prefabPath, typeof(GameObject)) as GameObject;
            _testGameObject = Object.Instantiate(prefab);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Templates");
            AssetDatabase.Refresh();
        }

        [Test]
        public void WorldRaycast()
        {
            //null/weird checks
            Assert.DoesNotThrow(() =>
            {
                PolyRaycastHit temp;
                //null
                bool tempHasHit = PolySceneUtility.WorldRaycast(new Ray(), null, null, null, out temp);
                Assert.IsFalse(tempHasHit);
                //empty vertices/triangles
                tempHasHit = PolySceneUtility.WorldRaycast(new Ray(), _testGameObject.transform, new Vector3[0], new int[0], out temp);
                Assert.IsFalse(tempHasHit);
                //empty vertices only
                tempHasHit = PolySceneUtility.WorldRaycast(new Ray(), _testGameObject.transform, new Vector3[0], new int[1] { 0 }, out temp);
                Assert.IsFalse(tempHasHit);
                tempHasHit = PolySceneUtility.WorldRaycast(new Ray(), _testGameObject.transform, new Vector3[0], new int[3] { 0, 1, 2 }, out temp);
                Assert.IsFalse(tempHasHit);
                //empty triangles only
                tempHasHit = PolySceneUtility.WorldRaycast(new Ray(), _testGameObject.transform, new Vector3[1] { Vector3.zero }, new int[0], out temp);
                Assert.IsFalse(tempHasHit);
            });

            Ray ray = GetTestRay();

            Vector3[] vertices = _testGameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
            int[] triangles = _testGameObject.GetComponent<MeshFilter>().sharedMesh.triangles;

            PolyRaycastHit hit;
            bool hasHit = PolySceneUtility.WorldRaycast(ray, _testGameObject.transform, vertices, triangles, out hit);

            Assert.IsTrue(hasHit);
            Assert.IsNotNull(hit);
            Assert.IsTrue(hit.distance == 2.5f); //should be equal to 3 - length of the cube divided by 2, wish is 0.5
            Assert.IsTrue(hit.normal == Vector3.back); //because the ray is directed to forward, the normal should face the opposite
            Assert.IsTrue(hit.position == _testGameObject.transform.position + new Vector3(0, 0, -0.5f)); //position of the cube - 0.5, half the length of the cube
            Assert.IsTrue(hit.triangle == 4); //because trust me...jk, pure regression test here.
        }

        private Ray GetTestRay()
        {
            return new Ray(_testGameObject.transform.position - _testGameObject.transform.forward * 3f, _testGameObject.transform.forward);
        }

        [Test]
        public void CalculateWeightedVertices()
        {
            //null checks
            Assert.DoesNotThrow(() =>
            {
                PolySceneUtility.CalculateWeightedVertices(null, null);
            });

            //setup the test
            BrushTarget target = new BrushTarget(EditableObject.Create(_testGameObject));
            BrushSettings brushSettings = ScriptableObject.CreateInstance<BrushSettings>();
            Assert.IsNotNull(target);
            Assert.IsNotNull(brushSettings);

            //fill raycasthits, will have to call WorldRaycast, then feed the raycastHits of the target
            Ray ray = GetTestRay();
            PolyRaycastHit hit;
            bool hasHit = PolySceneUtility.WorldRaycast(ray, _testGameObject.transform, target.editableObject.editMesh.vertices, target.editableObject.editMesh.GetTriangles(), out hit);
            Assert.IsTrue(hasHit);
            target.raycastHits.Add(hit);

            //then the function will set the weights
            PolySceneUtility.CalculateWeightedVertices(target, brushSettings);

            //check if the weights are not all equal to 0
            bool emptyWeights = true;
            foreach(float weight in target.GetAllWeights())
            {
                if(weight != 0)
                {
                    emptyWeights = false;
                    break;
                }
            }
            if(emptyWeights)
            {
                Assert.Fail("Weights are empty.");
            }
        }

        [Test]
        public void FindInstancesInScene()
        {
            //null checks
            Assert.DoesNotThrow(() =>
            {
                PolySceneUtility.FindInstancesInScene(null, null);
            });

            //setup, create few gameobject with various names
            GameObject object1 = CreateFindInstanceTestObject("Object1_Test");
            GameObject object2 = CreateFindInstanceTestObject("Object2_Test");
            GameObject object3 = CreateFindInstanceTestObject("Object3_Test");
            GameObject object4 = CreateFindInstanceTestObject("Object4_NoTest");

            List<GameObject> goList = new List<GameObject>() { object1, object2, object3, object4 };

            //the result should contains only object with "_Test" substring, so object1, object2, object3 only
            List<GameObject> result = PolySceneUtility.FindInstancesInScene(goList, (GameObject go) => 
            {
                if (go.name.Contains("_Test")) return go.name;
                else return "";
            }).ToList();

            Assert.IsTrue(result.Contains(object1));
            Assert.IsTrue(result.Contains(object2));
            Assert.IsTrue(result.Contains(object3));
            Assert.IsFalse(result.Contains(object4));

            //cleanup
            Object.DestroyImmediate(object1);
            Object.DestroyImmediate(object2);
            Object.DestroyImmediate(object3);
            Object.DestroyImmediate(object4);
        }

        /// <summary>
        /// Ensure that the test object is destroyed on the scene before creating a new one
        /// </summary>
        /// <param name="name">name of the object</param>
        /// <returns>the object created</returns>
        private static GameObject CreateFindInstanceTestObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
            go = new GameObject(name);
            return go;
        }
    }
}
