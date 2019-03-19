using System.Collections.Generic;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Geometry math and Array extensions.
    /// </summary>
    internal static class PolyMath
	{

        /// <summary>
		/// Epsilon to use when comparing vertex positions for equality.
		/// </summary>
		const float floatCompareEpsilon = .0001f;

        #region Geometry

        // Temporary vector3 values
        static Vector3 tv1, tv2, tv3, tv4;

		internal static bool RayIntersectsTriangle2(	Vector3 origin,
													Vector3 dir,
													Vector3 vert0,
													Vector3 vert1,
													Vector3 vert2,
		 											ref float distance,
		 											ref Vector3 normal)
		{
			float det;

            tv1 = vert1 - vert0;
            tv2 = vert2 - vert0;

			tv4 = Vector3.Cross(dir, tv2);
			det = Vector3.Dot(tv1, tv4);

			if(det < Mathf.Epsilon)
				return false;

            tv3 = origin - vert0;

			float u = Vector3.Dot(tv3, tv4);

			if(u < 0f || u > det)
				return false;

            tv4 = Vector3.Cross(tv3, tv1);

			float v = Vector3.Dot(dir, tv4);

			if(v < 0f || u + v > det)
				return false;

			distance = Vector3.Dot(tv2, tv4) * (1f / det);
            normal = Vector3.Cross(tv1, tv2);

			return true;
		}
        #endregion

        #region Normal and Tangents

        /// <summary>
        /// Calculate the unit vector normal of 3 points:  B-A x C-A
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        internal static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
		{
            Vector3 a = p1 - p0;
            Vector3 b = p2 - p0;

            Vector3 cross = Vector3.zero;
            cross = Vector3.Cross(a, b);
			cross.Normalize();

			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
				return cross;
		}

        /// <summary>
        /// If p.Length % 3 == 0, finds the normal of each triangle in a face and returns the average.
        /// Otherwise return the normal of the first three points.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static Vector3 Normal(Vector3[] p)
		{
			if(p.Length < 3)
				return Vector3.zero;

			if(p.Length % 3 == 0)
			{
				Vector3 nrm = Vector3.zero;

				for(int i = 0; i < p.Length; i+=3)
					nrm += Normal(	p[i+0],
									p[i+1],
									p[i+2]);

				return nrm / (p.Length/3f);
			}
			else
			{
				Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
				if (cross.magnitude < Mathf.Epsilon)
					return new Vector3(0f, 0f, 0f); // bad triangle
				else
				{
					return cross.normalized;
				}
			}
		}
#endregion

#region Algebra
        /// <summary>
        /// Average of a Vector3[].
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        internal static Vector3 Average(Vector3[] array, IEnumerable<int> indices)
		{
            if (array == null || indices == null) return Vector3.zero;

			Vector3 avg = Vector3.zero;
			int count = 0;

			foreach(int i in indices)
			{
				avg.x += array[i].x;
				avg.y += array[i].y;
				avg.z += array[i].z;

				count++;
			}

			return avg / count;
		}

        /// <summary>
        /// Returns a weighted average from values "array", "indices", and a lookup table of index weights.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indices"></param>
        /// <param name="weightLookup"></param>
        /// <returns></returns>
        internal static Vector3 WeightedAverage(Vector3[] array, IList<int> indices, float[] weightLookup)
		{
            if (array == null || indices == null || weightLookup == null) return Vector3.zero;

            float sum = 0f;
			Vector3 avg = Vector3.zero;

			for(int i = 0; i < indices.Count; i++)
			{
				float weight = weightLookup[indices[i]];
				avg.x += array[indices[i]].x * weight;
				avg.y += array[indices[i]].y * weight;
				avg.z += array[indices[i]].z * weight;
				sum += weight;
			}

			return sum > Mathf.Epsilon ? avg /= sum : Vector3.zero;
		}

        /// <summary>
        /// True if all elements of a vector are equal.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        internal static bool VectorIsUniform(Vector3 vec)
		{
			return Mathf.Abs(vec.x - vec.y) < Mathf.Epsilon && Mathf.Abs(vec.x - vec.z) < Mathf.Epsilon;
		}
        #endregion
    }
}
