using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using System;

namespace UnityEngine.Polybrush.EditorTests
{
    public class MathTest
    {
        [Test]
        public void RayIntersectsTriangle2()
        {
            //create a triangle
            Vector3 vert0 = new Vector3(0f, 0f, 0f);
            Vector3 vert1 = new Vector3(0.5f, 0f, 0.87f);
            Vector3 vert2 = new Vector3(1f, 0f, 0f);

            Vector3 origin = new Vector3(0.5f, 0.5f, 0.43f);
            Vector3 direction = Vector3.down;

            //result
            Vector3 hitNormal = Vector3.zero;
            float distance = 0f;

            //should intersects
            bool intersects = Math.RayIntersectsTriangle2(origin, direction, vert0, vert1, vert2, ref distance, ref hitNormal);

            Assert.IsTrue(intersects);
            Assert.IsTrue(distance == 0.5f);
            //the hitnormal is not normalized, I don't know why (????)
            Assert.IsTrue(hitNormal.normalized == Vector3.up);

            //should not intersects
            direction = Vector3.up;
            intersects = Math.RayIntersectsTriangle2(origin, direction, vert0, vert1, vert2, ref distance, ref hitNormal);
            Assert.IsFalse(intersects);

            //should not intersects
            origin = new Vector3(5f, 0.5f, 0f);
            direction = Vector3.down;
            intersects = Math.RayIntersectsTriangle2(origin, direction, vert0, vert1, vert2, ref distance, ref hitNormal);
            Assert.IsFalse(intersects);
        }

        class NormalWithThreePointsTestData
        {
            public static IEnumerable Data
            {
                get
                {
                    Vector3 v0 = new Vector3(0f, 0f, 0f);
                    Vector3 v1 = new Vector3(0.5f, 0f, 0.87f);
                    Vector3 v2 = new Vector3(1f, 0f, 0f);

                    yield return new TestCaseData(v0, v1, v2)
                        .Returns(Vector3.up)
                        .SetName("CalculateNormalWithPointsCollection_Up");
                    yield return new TestCaseData(v0, v2, v1)
                        .Returns(Vector3.down)
                        .SetName("CalculateNormalWithPointsCollection_Down");
                }
            }
        }

        [Test, TestCaseSource(typeof(NormalWithThreePointsTestData), "Data")]
        public Vector3 TestMathNormalWithThreePoints(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Math.Normal(v0, v1, v2);
        }
        
        class NormalWithPointsCollectionTestData
        {
            public static IEnumerable Data
            {
                get
                {
                    Vector3 v0 = new Vector3(0f, 0f, 0f);
                    Vector3 v1 = new Vector3(0.5f, 0f, 0.87f);
                    Vector3 v2 = new Vector3(1f, 0f, 0f);

                    yield return new TestCaseData(new Vector3[] {v0, v1, v2, v0, v2, v1})
                        .Returns(Vector3.zero)
                        .SetName("CalculateNormalWithPointsCollection_Zero");
                    yield return new TestCaseData(new Vector3[] {v0, v1, v2, v0, v2})
                        .Returns(Vector3.up)
                        .SetName("CalculateNormalWithPointsCollection_One");
                    yield return new TestCaseData(new Vector3[] {v0})
                        .Returns(Vector3.zero)
                        .SetName("CalculateNormalWithPointsCollection_Zero");
                }
            }
        }
        
        [Test, TestCaseSource(typeof(NormalWithPointsCollectionTestData), "Data")]
        public Vector3 CalculateNormalWithPointsCollection(Vector3[] points)
        {
            return Math.Normal(points);
        }

        [Test]
        public void Average_ArgumentVector2Null_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Math.Average((IList<Vector2>)null, null);
            });
        }

        [Test]
        public void Average_ArgumentVector3Null_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Math.Average((IList<Vector3>)null, null);
            });
        }

        [Test]
        public void Average_ArgumentVector4Null_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Math.Average((IList<Vector4>)null, null);
            });
        }

        [Test]
        public void Average_ColorArgumentNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Math.Average((IList<Color>)null, null);
            });
        }

        [Test]
        public void Average()
        {
            Vector3[] vertices = GetVerticesSample();

            //create a list of indexes that will be used 
            List<int> indexes = new List<int>() { 1, 2, 3, 4 };

            Vector3 average = Math.Average(vertices, indexes);
            Assert.IsTrue(average == new Vector3(0.5f, 0f, 0.5f));
        }

        private static Vector3[] GetVerticesSample()
        {
            //build up an array with vector3, like an array of vertices. Add some values that will be filtered by the function
            Vector3[] values = new Vector3[]
            {
                //unused value
                new Vector3(Random.Range(-Mathf.Infinity, Mathf.Infinity), Random.Range(-Mathf.Infinity, Mathf.Infinity),Random.Range(-Mathf.Infinity, Mathf.Infinity)),
                //square
                new Vector3(0,0,0),
                new Vector3(0,0,1),
                new Vector3(1,0,1),
                new Vector3(1,0,0),
                //unused value
                new Vector3(Random.Range(-Mathf.Infinity, Mathf.Infinity), Random.Range(-Mathf.Infinity, Mathf.Infinity),Random.Range(-Mathf.Infinity, Mathf.Infinity))
            };
            return values;
        }

        [Test]
        public void WeightedAverage()
        {
            //null checks
            Assert.DoesNotThrow(() =>
            {
                Math.WeightedAverage(null, null, null);
            });

            Vector3[] vertices = GetVerticesSample();

            //create a list of indexes that will be used 
            List<int> indexes = new List<int>() { 1, 2, 3, 4 };

            //create a list of weights, size must be equal to the size of the vertices array
            float[] weights = new float[] { 0, 1, 1, 1, 1, 0 };
            //the weights should not affect the result on this test
            Vector3 average = Math.WeightedAverage(vertices, indexes, weights);
            Assert.IsTrue(average == new Vector3(0.5f, 0f, 0.5f));

            //this time only the third corner of the square should be returned
            weights = new float[] { 0, 0, 0, 1, 0, 0 };
            average = Math.WeightedAverage(vertices, indexes, weights);
            Assert.IsTrue(average == new Vector3(1f, 0f, 1f));

            //trying with weights higher than 1
            weights = new float[] { 0, 0, 1, 3, 0, 0 };
            average = Math.WeightedAverage(vertices, indexes, weights);
            Assert.IsTrue(average == new Vector3(0.75f, 0f, 1f));
        }

        [Test]
        public void VectorIsUniform()
        {
            Assert.IsTrue(Math.VectorIsUniform(Vector3.one));
            Assert.IsFalse(Math.VectorIsUniform(Vector3.right));
        }
    }
}
