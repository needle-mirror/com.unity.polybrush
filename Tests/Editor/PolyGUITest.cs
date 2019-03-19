using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Polybrush;
using UnityEditor;
using System.Linq;
using UnityEngine.TestTools;

namespace UnityEngine.Polybrush.EditorTests
{
    public class PolyGUITest
    {
        private const string _testContent = "TestContent";
        private const string _testTooltip = "TestTooltip";

        [Test]
        public void TempContent()
        {
            GUIContent content = PolyGUI.TempContent(_testContent, _testTooltip);

            Assert.IsNotNull(content);
            Assert.IsTrue(content.text == _testContent);
            Assert.IsTrue(content.tooltip == _testTooltip);
        }

        [Test]
        public void BackgroundColorStack()
        {
            //save the current stack
            Stack<Color> savedStack = new Stack<Color>(PolyGUI.s_BackgroundColor);
            Color savedColor = GUI.backgroundColor;

            //reset
            GUI.backgroundColor = Color.white;
            PolyGUI.s_BackgroundColor = new Stack<Color>();

            //push one color
            Color previousBackgroundColor = GUI.backgroundColor;

            PolyGUI.PushBackgroundColor(Color.yellow);

            Assert.IsTrue(PolyGUI.s_BackgroundColor.Count == 1);
            Assert.IsTrue(PolyGUI.s_BackgroundColor.First() == previousBackgroundColor);
            Assert.IsTrue(GUI.backgroundColor == Color.yellow);

            //push a second color
            previousBackgroundColor = GUI.backgroundColor;

            PolyGUI.PushBackgroundColor(Color.red);

            Assert.IsTrue(PolyGUI.s_BackgroundColor.Count == 2);
            Assert.IsTrue(PolyGUI.s_BackgroundColor.First() == previousBackgroundColor);
            Assert.IsTrue(GUI.backgroundColor == Color.red);

            //pop one color
            PolyGUI.PopBackgroundColor();
            Assert.IsTrue(PolyGUI.s_BackgroundColor.Count == 1);
            //the popped color should be assigned to background color
            Assert.IsTrue(GUI.backgroundColor == Color.yellow);

            //pop the last color
            PolyGUI.PopBackgroundColor();
            Assert.IsTrue(PolyGUI.s_BackgroundColor.Count == 0);
            //color should be the same as the one that the background was at first before starting pushing/poping
            Assert.IsTrue(GUI.backgroundColor == Color.white);

            //pop nothing
            PolyGUI.PopBackgroundColor();
            Assert.IsTrue(GUI.backgroundColor == Color.white);

            //reset to previous data
            PolyGUI.s_BackgroundColor = new Stack<Color>(savedStack);
            GUI.backgroundColor = savedColor;
        }

        [Test]
        public void BackgroundColorStyle()
        {
            Assert.IsNotNull(PolyGUI.BackgroundColorStyle);
        }

        [Test]
        public void CenteredStyle()
        {
            Assert.IsNotNull(PolyGUI.CenteredStyle);
        }
    }
}
