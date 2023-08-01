using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// General static helper functions.
    /// </summary>
    internal static class Util
	{
        /// <summary>
        /// Returns a new array initialized with the @count and @value.
        /// </summary>
        internal static T[] Fill<T>(T value, int count)
		{
            //return an empty list if the count is negative
            if(count < 0)
            {
                return null;
            }

			T[] arr = new T[count];
			for(int i = 0; i < count; i++)
				arr[i] = value;
			return arr;
		}

        /// <summary>
        /// Returns a new array initialized with the @count and @value.
        /// </summary>
        internal static T[] Fill<T>(System.Func<int, T> constructor, int count)
		{
            //return an empty list if the count is negative
            if (count < 0)
            {
                return null;
            }

            T[] arr = new T[count];
			for(int i = 0; i < count; i++)
				arr[i] = constructor(i);
			return arr;
		}

        /// <summary>
        /// Make a copy of an array
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="array">array to be copied</param>
        /// <returns>the new array of type T</returns>
		internal static T[] Duplicate<T>(T[] array)
		{
            //null checks
			if(array == null)
            {
                return null;
            }

            T[] dup = new T[array.Length];
			System.Array.Copy(array, 0, dup, 0, array.Length);
			return dup;
		}

		internal static string ToString<T>(this IEnumerable<T> enumerable, string delim)
		{
			if(enumerable == null)
				return "";

			return string.Join(delim ?? "", enumerable.Select(x => x != null ? x.ToString() : "").ToArray());
		}


        /// <summary>
        /// Clamp an animation curve's first and last keys.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="firstKeyTime"></param>
        /// <param name="firstKeyValue"></param>
        /// <param name="secondKeyTime"></param>
        /// <param name="secondKeyValue"></param>
        /// <returns></returns>
        public static AnimationCurve ClampAnimationKeys(AnimationCurve curve,
                                                            float firstKeyTime,
                                                            float firstKeyValue,
                                                            float secondKeyTime,
                                                            float secondKeyValue)
        {
            Keyframe[] keys = curve.keys;
            int len = curve.length - 1;

            keys[0].time = firstKeyTime;
            keys[0].value = firstKeyValue;
            keys[len].time = secondKeyTime;
            keys[len].value = secondKeyValue;

            curve.keys = keys;
            return new AnimationCurve(keys);
        }

        /// <summary>
        /// Create a dictionnary with keys created from T values (from sublists items) and the value equal to the index of the top list
        /// Note that the function must receive unique values on sublists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lists"></param>
        /// <returns></returns>
		internal static Dictionary<T, int> GetCommonLookup<T>(this List<List<T>> lists)
		{
			Dictionary<T, int> lookup = new Dictionary<T, int>();

			int index = 0;

			foreach(var kvp in lists)
			{
                if (kvp == null) continue;

				foreach(var val in kvp)
				{
                    if(lookup.ContainsKey(val))
                    {
                        Debug.LogWarning("Error, duplicated values as keys");
                        return null;
                    }
					lookup.Add(val, index);
				}

				index++;
			}

			return lookup;
		}

        /// <summary>
        /// Create a dictionnary with keys created from T values (from sublists items) and the value equal to the index of the top list
        /// Note that the function must receive unique values on sublists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lists"></param>
        /// <returns></returns>
        internal static Dictionary<T, int> GetCommonLookup<T>(this T[][] lists)
        {
            Dictionary<T, int> lookup = new Dictionary<T, int>();

            int index = 0;

            foreach (var kvp in lists)
            {
                if (kvp == null) continue;

                foreach (var val in kvp)
                {
                    if (lookup.ContainsKey(val))
                    {
                        Debug.LogWarning("Error, duplicated values as keys");
                        return null;
                    }
                    lookup.Add(val, index);
                }

                index++;
            }

            return lookup;
        }

        /// <summary>
        /// Lerp between 2 colors using RGB.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="mask"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        internal static Color Lerp(Color lhs, Color rhs, ColorMask mask, float alpha)
        {
            return new Color(	mask.r ? (lhs.r * (1f-alpha) + rhs.r * alpha) : lhs.r,
                mask.g ? (lhs.g * (1f-alpha) + rhs.g * alpha) : lhs.g,
                mask.b ? (lhs.b * (1f-alpha) + rhs.b * alpha) : lhs.b,
                mask.a ? (lhs.a * (1f-alpha) + rhs.a * alpha) : lhs.a );
        }

        /// <summary>
        /// Lerp between 2 colors using RGB.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        internal static Color32 Lerp(Color32 lhs, Color32 rhs, float alpha)
		{
			return new Color32(	(byte)(lhs.r * (1f-alpha) + rhs.r * alpha),
								(byte)(lhs.g * (1f-alpha) + rhs.g * alpha),
								(byte)(lhs.b * (1f-alpha) + rhs.b * alpha),
								(byte)(lhs.a * (1f-alpha) + rhs.a * alpha) );
		}

        /// <summary>
        /// True if object is non-null and valid.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static bool IsValid<T>(this T target) where T : IValid
		{
			return target != null && target.IsValid;
		}

        /// <summary>
        /// Returns a new name with incremented prefix.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string IncrementPrefix(string prefix, string name)
		{
			string str = name;

			Regex regex = new Regex("^(" + prefix + "[0-9]*_)");
			Match match = regex.Match(name);

			if( match.Success )
			{
				string iteration = match.Value.Replace(prefix, "").Replace("_", "");
				int val = 0;

                if (int.TryParse(iteration, out val))
                {
                    str = name.Replace(match.Value, prefix + (val + 1) + "_");
                }
                else
                {
                    str = prefix + "0_" + name;
                }
            }
			else
			{
				str = prefix + "0_" + name;
			}

			return str;
		}

        /// <summary>
        /// Checks a GameObject for SkinnedMeshRenderer & MeshRenderer components
        /// and returns all materials associated with either.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static List<Material> GetMaterials(this GameObject gameObject)
		{
            //null checks
            if(gameObject == null)
            {
                return null;
            }

			List<Material> mats = new List<Material>();

			foreach(Renderer ren in gameObject.GetComponents<Renderer>())
				mats.AddRange(ren.sharedMaterials);

			return mats;
		}

        /// <summary>
        /// Will return the mesh if found
        /// </summary>
        /// <param name="go">the gameobject that contains the mesh</param>
        /// <returns>the mesh found if any</returns>
        internal static Mesh GetMesh(this GameObject go)
        {
            //null checks
            if (go == null)
            {
                return null;
            }

            var mf = go.GetComponent<MeshFilter>();
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            var polyMesh = go.GetComponent<PolybrushMesh>();
            var mr = go.GetComponent<MeshRenderer>();

            //priority order: advs component > vertexstream mesh renderer > mesh filter > skin mesh renderer
            //even if normally having an additionalVertexStreams means that the component advs is on the object, double check it
            if (polyMesh != null && polyMesh.storedMesh!= null)
            {
                return polyMesh.storedMesh;
            }
            else if(mr != null && mr.additionalVertexStreams != null)
            {
                return mr.additionalVertexStreams;
            }
            else if (mf != null && mf.sharedMesh != null)
            {
                return mf.sharedMesh;
            }
            else if (smr != null && smr.sharedMesh != null)
            {
                return smr.sharedMesh;
            }
            else
            {
                return null;
            }
        }
	}
}
