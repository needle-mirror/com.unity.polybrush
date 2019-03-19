using System.Collections;
using NUnit.Framework;

namespace UnityEngine.Polybrush.EditorTests
{
    public class BrushMirrorUtilityTest
    {
        class BrushMirrorToVector3TestCases
        {
            private static IEnumerable Data
            {
                get
                {
                    yield return new TestCaseData(BrushMirror.X)
                        .Returns(new Vector3(-1f, 1f, 1f))
                        .SetName("BrushMirrorToVector3_X_[-1, 1, 1]");
                    yield return new TestCaseData(BrushMirror.Y)
                        .Returns(new Vector3(1f, -1f, 1f))
                        .SetName("BrushMirrorToVector3_Y_[1, -1, 1]");
                    yield return new TestCaseData(BrushMirror.Z)
                        .Returns(new Vector3(1f, 1f, -1f))
                        .SetName("BrushMirrorToVector3_Z_[1, 1, -1]");
                    yield return new TestCaseData(BrushMirror.X | BrushMirror.Y)
                        .Returns(new Vector3(-1f, -1f, 1f))
                        .SetName("BrushMirrorToVector3_(X|Y)_[-1, -1, 1]");
                    yield return new TestCaseData(BrushMirror.X | BrushMirror.Z)
                        .Returns(new Vector3(-1f, 1f, -1f))
                        .SetName("BrushMirrorToVector3_(X|Z)_[-1, 1, -1]");
                    yield return new TestCaseData(BrushMirror.Y | BrushMirror.Z)
                        .Returns(new Vector3(1f, -1f, -1f))
                        .SetName("BrushMirrorToVector3_(Y|Z)_[1, -1, -1]");
                    yield return new TestCaseData(BrushMirror.X | BrushMirror.Y | BrushMirror.Z)
                        .Returns(new Vector3(-1f, -1f, -1f))
                        .SetName("BrushMirrorToVector3_(X|Y|Z)_[-1, -1, -1]");

                }
            }
        }
        
        [Test, TestCaseSource(typeof(BrushMirrorToVector3TestCases), "Data")]
        public Vector3 BrushMirrorToVector3(object mask)
        {
            return ((BrushMirror)mask).ToVector3();
        }
    }

    public class ComponentIndexUtilityTest
    {
        [Test]
        public void ToFlag()
        {
            //wrong tests
            //because C# will let you cast any value outside the enum
            Assert.IsTrue(((ComponentIndex)100).ToFlag() == 100);

            ComponentIndex index = ComponentIndex.R;
            Assert.IsTrue(index.ToFlag() == 1);

            index = ComponentIndex.G;
            Assert.IsTrue(index.ToFlag() == 2);

            index = ComponentIndex.B;
            Assert.IsTrue(index.ToFlag() == 4);

            index = ComponentIndex.A;
            Assert.IsTrue(index.ToFlag() == 8);
        }

        [Test]
        public void GetString()
        {
            //wrong tests
            //because C# will let you cast any value outside the enum
            Assert.IsTrue(((ComponentIndex)100).GetString() == "100");

            ComponentIndex index = ComponentIndex.R;
            Assert.IsTrue(index.GetString(ComponentIndexType.Color) == "R");

            index = ComponentIndex.G;
            Assert.IsTrue(index.GetString(ComponentIndexType.Color) == "G");

            index = ComponentIndex.B;
            Assert.IsTrue(index.GetString(ComponentIndexType.Color) == "B");

            index = ComponentIndex.A;
            Assert.IsTrue(index.GetString(ComponentIndexType.Color) == "A");

            index = ComponentIndex.X;
            Assert.IsTrue(index.GetString(ComponentIndexType.Vector) == "X");

            index = ComponentIndex.Y;
            Assert.IsTrue(index.GetString(ComponentIndexType.Vector) == "Y");

            index = ComponentIndex.Z;
            Assert.IsTrue(index.GetString(ComponentIndexType.Vector) == "Z");

            index = ComponentIndex.W;
            Assert.IsTrue(index.GetString(ComponentIndexType.Vector) == "W");

            index = ComponentIndex.R;
            Assert.IsTrue(index.GetString(ComponentIndexType.Index) == "0");
        }
    }

    public class DirectionUtilTest
    {
        [Test]
        public void ToVector3()
        {
            PolyDirection polyDirection = PolyDirection.BrushNormal;
            Assert.IsTrue(polyDirection.ToVector3() == Vector3.zero);

            polyDirection = PolyDirection.Forward;
            Assert.IsTrue(polyDirection.ToVector3() == Vector3.forward);

            polyDirection = PolyDirection.Right;
            Assert.IsTrue(polyDirection.ToVector3() == Vector3.right);

            polyDirection = PolyDirection.Up;
            Assert.IsTrue(polyDirection.ToVector3() == Vector3.up);

            polyDirection = PolyDirection.VertexNormal;
            Assert.IsTrue(polyDirection.ToVector3() == Vector3.zero);
        }
    }

    public class MeshChannelUtilityTest
    {
        [Test]
        public void UVChannelToIndex()
        {
            //null checks
            Assert.IsTrue(MeshChannelUtility.UVChannelToIndex(MeshChannel.Null) == -1);

            Assert.IsTrue(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0) == 0);
            Assert.IsTrue(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2) == 1);
            Assert.IsTrue(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3) == 2);
            Assert.IsTrue(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4) == 3);
        }
    }
}