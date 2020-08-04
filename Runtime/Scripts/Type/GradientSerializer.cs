using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Serializer for Gradient with some helper functions
    /// </summary>
	internal static class GradientSerializer
	{
		internal static string Serialize(this Gradient gradient)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach(GradientColorKey c in gradient.colorKeys)
			{
				sb.Append(c.color.ToString("F8"));
				sb.Append("&");
				sb.Append(c.time.ToString("F8"));
				sb.Append("|");
			}

			sb.Append("\n");

			foreach(GradientAlphaKey a in gradient.alphaKeys)
			{
				sb.Append(a.alpha.ToString("F8"));
				sb.Append("&");
				sb.Append(a.time.ToString("F8"));
				sb.Append("|");
			}

			return sb.ToString();
		}

        internal static Gradient Deserialize(string str)
        {
            Gradient gradient = new Gradient();
            Deserialize(str, out gradient);
            return gradient;
        }

		internal static bool Deserialize(string str, out Gradient gradient)
		{
			gradient = null;

			string[] arrays = str.Split('\n');

			if(arrays.Length < 2)
				return false;

			string[] colors_str = arrays[0].Split('|');
			string[] alphas_str = arrays[1].Split('|');

			if(colors_str.Length < 2 || alphas_str.Length < 2)
				return false;

			List<GradientColorKey> colors = new List<GradientColorKey>();
			List<GradientAlphaKey> alphas = new List<GradientAlphaKey>();

			foreach(string s in colors_str)
			{
				string[] key = s.Split('&');

				if(key.Length < 2)
					continue;

				Color value;
				float time;

				if(!TryParseColor(key[0], out value))
					continue;

				if(!float.TryParse(key[1], out time))
					continue;

				colors.Add( new GradientColorKey(value, time) );
			}

			foreach(string s in alphas_str)
			{
				string[] key = s.Split('&');

				if(key.Length < 2)
					continue;

				float alpha, time;

				if(!float.TryParse(key[0], out alpha))
					continue;
				if(!float.TryParse(key[1], out time))
					continue;

				alphas.Add( new GradientAlphaKey(alpha, time) );
			}

			gradient = new Gradient();
			gradient.SetKeys(colors.ToArray(), alphas.ToArray());

			return true;
		}

		private static bool TryParseColor(string str, out Color value)
		{
			string[] rep = str.Replace("RGBA(", "").Replace(")", "").Split(',');

			value = Color.white;

			if(rep.Length != 4)
				return false;

			float a = 1f;

			if(!float.TryParse(rep[0], out value.r))
				return false;

			if(!float.TryParse(rep[1], out value.g))
				return false;

			if(!float.TryParse(rep[2], out value.b))
				return false;

			if(!float.TryParse(rep[3], out a))
				return false;

			value.a = a / 255f;

			return true;
		}

        /// <summary>
        /// Compare the content of two Gradient
        /// </summary>
        /// <param name="original"></param>
        /// <param name="compareWith"></param>
        /// <returns>true if both Gradient have the same content, false otherwise</returns>
        internal static bool CompareContentWith(this Gradient original, Gradient compareWith)
        {
            if (original.alphaKeys.Length != compareWith.alphaKeys.Length) return false;
            if (original.colorKeys.Length != compareWith.colorKeys.Length) return false;
            for (int index = 0; index < original.alphaKeys.Length; index++)
            {
                if (!original.alphaKeys[index].CompareContentWith(compareWith.alphaKeys[index])) return false;
            }
            for (int index = 0; index < original.colorKeys.Length; index++)
            {
                if (!original.colorKeys[index].CompareContentWith(compareWith.colorKeys[index])) return false;
            }

            return true;
        }

        /// <summary>
        /// Compare the content of two GradientAlphaKey
        /// </summary>
        /// <param name="original"></param>
        /// <param name="compareWith"></param>
        /// <returns>true if both GradientAlphaKey have the same content, false otherwise</returns>
        internal static bool CompareContentWith(this GradientAlphaKey original, GradientAlphaKey compareWith)
        {
            if (original.alpha != compareWith.alpha) return false;
            if (original.time != compareWith.time) return false;
            return true;
        }

        /// <summary>
        /// Compare the content of two GradientColorKey
        /// </summary>
        /// <param name="original"></param>
        /// <param name="compareWith"></param>
        /// <returns>true if both GradientColorKey have the same content, false otherwise</returns>
        internal static bool CompareContentWith(this GradientColorKey original, GradientColorKey compareWith)
        {
            if (original.color != compareWith.color) return false;
            if (original.time != compareWith.time) return false;
            return true;
        }
    }
}
