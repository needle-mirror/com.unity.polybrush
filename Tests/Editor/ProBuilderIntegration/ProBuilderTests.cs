using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using UnityEditor.Polybrush;

namespace UnityEngine.Polybrush.EditorTests
{
    public class ProBuilderTests
    {
        //[Test]
        public void MethodAndPropertyAreSet()
        {
            Assert.DoesNotThrow(ProBuilderBridge.TestsUtility.ValidateIntegration);
        }

        //[Test]
        public void ProBuilder_OnSelectModeChangedListenerDefinition_Exists()
        {
            Assert.DoesNotThrow(ProBuilderBridge.TestsUtility.ValidatePolybrushListeners);
        }
    }
}
