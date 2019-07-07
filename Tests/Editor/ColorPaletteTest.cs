using NUnit.Framework;
using UnityEditor;
using UnityEditor.Polybrush;
using System.Collections.Generic;

namespace UnityEngine.Polybrush.EditorTests
{
    public class ColorPaletteTest
    {
        private ColorPalette m_Palette;

        [SetUp]
        public void Init()
        {
            m_Palette = PolyEditorUtility.CreateNew<ColorPalette>();
        }

        [TearDown]
        public void Term()
        {
            if (m_Palette)
            {
                var path = AssetDatabase.GetAssetPath(m_Palette);
                AssetDatabase.DeleteAsset(path);
            }
        }

        [Test]
        public void ColorPalette_EditorReset_ColorsMatchWithDefaultPaletteDefinition()
        {
            // Get a copy of the original default colors
            var colors = new List<Color>(m_Palette.colors);

            // add 2 colors, and remove one
            // to make sure palette is actually reset
            m_Palette.colors.Add(Color.blue);
            m_Palette.colors.RemoveAt(3);
            m_Palette.colors.Insert(1, Color.green);

            // This is equivalent to calling Reset from the inspector
            Unsupported.SmartReset(m_Palette);

            // Should still have all the default colors after resetting
            var resetColors = m_Palette.colors;
            Assert.That(resetColors.Count, Is.EqualTo(colors.Count));
            // Check that the reset colors match the original
            Assert.That(resetColors, Is.EquivalentTo(colors));
        }
    }
}
