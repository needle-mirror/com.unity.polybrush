using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// A set of colors.
    /// </summary>
    [CreateAssetMenuAttribute(menuName = "Polybrush/Color Palette", fileName = "Color Palette", order = 801)]
	[System.Serializable]
	internal class ColorPalette : PolyAsset, IHasDefault, ICustomSettings
	{
        // The currently selected color.
        [SerializeField] internal Color current = Color.white;

        // All colors in this palette.
        [SerializeField] internal List<Color> colors;

        public string assetsFolder { get { return "Color Palette/"; } }

        public void SetDefaultValues()
		{
			colors = new List<Color>()
			{
				new Color(1f, 1f, 1f),
				new Color(0.866f, 0.866f, 0.866f),
				new Color(0.733f, 0.733f, 0.733f),
				new Color(0.6f, 0.6f, 0.6f),
				new Color(0.466f, 0.466f, 0.466f),
				new Color(0.333f, 0.333f, 0.333f),
				new Color(0.2f, 0.2f, 0.2f),
				new Color(0f, 0f, 0f),

				new Color(1f, 0f, 0f, 1f),
				new Color(0f, 1f, 0f, 1f),
				new Color(0f, 0f, 1f, 1f),

				new Color(0.090f, 0.270f, 0.607f),
				new Color(0.929f, 0.929f, 0f),
				new Color(0.247f, 0.8f, 0f),
				new Color(0.827f, 0f, 0f),
				new Color(0.929f, 0.933f, 0.725f),
				new Color(0.352f, 0.533f, 0f),
				new Color(0.976f, 0.729f, 0f),
				new Color(0.258f, 0.737f, 1f),
			};
		}

		internal void CopyTo(ColorPalette target)
		{
			target.colors = new List<Color>(colors);
		}

        protected override void Reset()
        {
            base.Reset();
            SetDefaultValues();
        }
    }
}
