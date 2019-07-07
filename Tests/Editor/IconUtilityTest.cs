using NUnit.Framework;
using UnityEditor.Polybrush;

namespace UnityEngine.Polybrush.EditorTests
{
    public class IconUtilityTest
    {
        static readonly string k_SculpIconPath = "Toolbar/Sculpt";
        static readonly string k_InvalidPath = "Invalid";

        [Test]
        public void GetIcon_ArgumentEmptyString_NoExceptionThrown()
        {
            Assert.DoesNotThrow(() => IconUtility.GetIcon(string.Empty));
        }

        [Test]
        public void GetIcon_ArgumentEmptyString_ReturnsNull()
        {
            Assert.IsNull(IconUtility.GetIcon(string.Empty));
        }

        [Test]
        public void GetIcon_ArgumentInvalidPathToTexture_ReturnsNull()
        {
            Assert.IsNull(IconUtility.GetIcon(k_InvalidPath));
        }

        [Test]
        public void GetIcon_ArgumentValidPathToTexture_ReturnsValidReference()
        {
            Assert.IsNotNull(IconUtility.GetIcon(k_SculpIconPath));
        }
    }
}
