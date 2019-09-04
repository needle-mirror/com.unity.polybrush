using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEditor.Polybrush;
using UnityEngine.TestTools;

namespace UnityEngine.Polybrush
{
    public class PlaymodeTest
    {
        int m_ReceivedLogsCount = 0;

        [SetUp]
        public void Setup()
        {
            Application.logMessageReceived += Application_logMessageReceived;

            MenuItems.MenuInitEditorWindow();
            PolyEditor.instance.Focus();
            m_ReceivedLogsCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= Application_logMessageReceived;

            PolyEditor.instance.Close();
        }

        //[UnityTest]
        public IEnumerator EditorEnterPlayMode_NoConsoleLogs()
        {
            Assume.That(PolyEditor.instance != null);

            yield return new EnterPlayMode();

            Assert.That<int>(m_ReceivedLogsCount, Is.Zero);

            yield return new ExitPlayMode();
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            m_ReceivedLogsCount += 1;
        }
    }
}
