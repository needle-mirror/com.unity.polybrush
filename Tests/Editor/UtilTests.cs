using NUnit.Framework;
using System.Collections.Generic;

namespace UnityEngine.Polybrush.EditorTests
{
    public class UtilTests
    {
        [Test]
        public void Fill()
        {
            //first signature ------------------------------------
            //weird cases
            Util.Fill(Vector3.zero, -10);

            //real test
            Vector3[] result = Util.Fill(Vector3.zero, 10);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 10);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.IsTrue(result[i] == Vector3.zero);
            }

            //second signature ------------------------------------
            //weird cases
            Util.Fill((x) => new Vector3(x, x, x), -10);

            //real test
            Vector3[] secondResult = Util.Fill((x) => new Vector3(x,x,x), 10);
            //test if the array is filled correctly
            Assert.IsNotNull(secondResult);
            Assert.IsTrue(secondResult.Length == 10);
            for(int index = 0; index < secondResult.Length; index++)
            {
                Assert.IsTrue(secondResult[index] == new Vector3(index, index, index));
            }
        }

        [Test]
        public void Duplicate()
        {
            //null checks
            Assert.DoesNotThrow(() =>
            {
                Util.Duplicate<object>(null);
            });

            float[] originalArray = new float[] { 0, 10, 5, 3 };
            float[] arrayToCompare = Util.Duplicate(originalArray);
            Assert.AreNotSame(originalArray, arrayToCompare);
            for (int i = 0; i < arrayToCompare.Length; i++)
            {
                Assert.AreEqual(arrayToCompare[i], originalArray[i]);
            }
        }

        [Test]
        new public void ToString()
        {
            float[] originalArray = new float[] { 0, 10, 5, 3 };

            string result = originalArray.ToString("_");
            Assert.IsTrue(result == "0_10_5_3");
        }

        [Test]
        public void GetCommonLookup()
        {
            List<List<int>> testList = null;

            //weird null checks -----------------------------
            testList = new List<List<int>>()
            {
                new List<int>(){ 0, 1, 2 },
                new List<int>(){ 0, 1, 2 },
                new List<int>(){ 0, 1, 2 }
            };
            //Assert.DoesNotThrow(() =>
            //{
            //    Dictionary<int, int> nullResult = testList.GetCommonLookup();
            //    Assert.IsNull(nullResult);
            //});

            testList = new List<List<int>>()
            {
                null, null, null
            };
            Assert.DoesNotThrow(() =>
            {
                Dictionary<int, int> emptyResult = testList.GetCommonLookup();
                Assert.IsNotNull(emptyResult);
                Assert.IsTrue(emptyResult.Count == 0);
            });

            //normal test -----------------------------
            testList = new List<List<int>>()
            {
                new List<int>(){ 0, 1, 2 },
                new List<int>(){ 10, 11, 12 },
                new List<int>(){ 20, 21, 22 }
            };

            Dictionary<int, int> result = testList.GetCommonLookup();
            //we should retrieve a dictionnary with 9 entries (all values inside sublists), with those values as keys, with all index (from the top list) as value
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 9);
            //test it
            //for (int i = 0; i < testList.Count; i++)
            //{
            //    for (int j = 0; j < testList[i].Count; j++)
            //    {
            //        int dictKey = testList[i][j];
            //        Assert.IsTrue(result.ContainsKey(dictKey));
            //        Assert.IsTrue(result[dictKey] == i);
            //    }
            //}
        }

        [Test]
        public void Lerp()
        {
            //using full opposite color to track down individual errors on the function
            Color32 white = new Color32(255,255,255,255);
            Color32 black = new Color32(0,0,0,0);

            //test it
            Color32 result = Util.Lerp(white, black, 0.5f);
            //compare, it should be halfway between black and white
            Assert.AreEqual(result, new Color32(127, 127, 127, 127));

            //other signature
            //the mask with all false should only return the left color, which is white in our case
            ColorMask colorMask = new ColorMask(false, false, false, false);
            result = Util.Lerp(white, black, colorMask, 0.5f);
            Assert.AreEqual(result, white);
        }


        public enum TestEnum
        {
            First = 0,
            Second = 100,
            Third = 1000,
        }

        [Test]
        public void IsValid()
        {
            IValidTest nullCheck = null;
            Assert.IsFalse(nullCheck.IsValid());
            nullCheck = new IValidTest();
            Assert.IsTrue(nullCheck.IsValid());
        }

        [Test]
        public void IncrementPrefix()
        {
            Assert.AreEqual("prefix0_polybrush", Util.IncrementPrefix("prefix", "polybrush"));
            Assert.AreEqual("prefix1_polybrush", Util.IncrementPrefix("prefix", "prefix0_polybrush"));
        }

        [Test]
        public void GetMaterials()
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            List<Material> primitiveMaterials = new List<Material>(primitive.GetComponent<Renderer>().sharedMaterials);

            List<Material> resultMats = primitive.GetMaterials();
            Assert.AreEqual(primitiveMaterials, resultMats);
        }

        [Test]
        public void GetMesh()
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);

            Mesh resultMesh = primitive.GetMesh();
            Assert.AreEqual(primitive.GetComponent<MeshFilter>().sharedMesh, resultMesh);
        }
    }

    public class IValidTest : IValid
    {
        public bool IsValid
        {
            get
            {
                return this != null;
            }
        }
    }
}
