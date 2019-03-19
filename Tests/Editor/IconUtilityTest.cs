using NUnit.Framework;
using UnityEditor.Polybrush;

namespace UnityEngine.Polybrush.EditorTests
{
    public class IconUtilityTest
    {
        static readonly string k_SculpIconPath = "Toolbar/Sculpt";
       [Test]
       public void GetIcon()
        {
            //null checks
            //Assert.DoesNotThrow(() => IconUtility.GetIcon(string.Empty));
            //Assert.IsNotNull(IconUtility.GetIcon(k_SculpIconPath));
        }
    }
}
