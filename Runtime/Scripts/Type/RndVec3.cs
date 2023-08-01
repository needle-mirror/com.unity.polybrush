using UnityEngine;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Vector3 that is sortable and equatable by a rounded value (resolution).
    /// </summary>
    public struct RndVec3 : System.IEquatable<RndVec3>
	{
		internal float x;
		internal float y;
		internal float z;

		const float resolution = .0001f;

		internal RndVec3(Vector3 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

        /// <summary>
        /// Equality comparer for RndVec3
        /// </summary>
        /// <param name="p"> RndVec3 to compare to this</param>
        /// <returns>true if the 2 RndVec3 are equals</returns>
		public bool Equals(RndVec3 p)
		{
			return  Mathf.Abs(x - p.x) < resolution &&
					Mathf.Abs(y - p.y) < resolution &&
					Mathf.Abs(z - p.z) < resolution;
		}

        /// <summary>
        /// Equality comparer for a RndVec3 with a Vector3
        /// </summary>
        /// <param name="p"> Vector3 to compare to this</param>
        /// <returns>true if this is equal to the Vector3 regarding the RndVec3 resolution</returns>
		public bool Equals(Vector3 p)
		{
			return  Mathf.Abs(x - p.x) < resolution &&
					Mathf.Abs(y - p.y) < resolution &&
					Mathf.Abs(z - p.z) < resolution;
		}

        /// <summary>
        /// Equality comparer for RndVec3
        /// </summary>
        /// <param name="b"> System.Object that should be compare to this</param>
        /// <returns>true if the 2 elements are RndVec3 (or Vector3) and are equals</returns>
		public override bool Equals(System.Object b)
		{
			return 	(b is RndVec3 && ( this.Equals((RndVec3)b) )) ||
					(b is Vector3 && this.Equals((Vector3)b));
		}

        /// <summary>
        /// HashCode Generation
        /// </summary>
        /// <returns>unique hashcode for RndVec3</returns>
		public override int GetHashCode()
		{
			int hash = 27;

			unchecked
			{
				hash = hash * 29 + round(x);
				hash = hash * 29 + round(y);
				hash = hash * 29 + round(z);
			}

			return hash;
		}

        /// <summary>
        /// Stringification of the RndVec3 data
        /// </summary>
        /// <returns>String representing the RndVec3</returns>
		public override string ToString()
		{
			return string.Format("{{{0:F2}, {1:F2}, {2:F2}}}", x, y, z);
		}

		private int round(float v)
		{
			return (int) (v / resolution);
		}

        /// <summary>
        /// Constructor for a Vector3 from a RndVec3
        /// </summary>
        /// <param name="p">input RndVec3</param>
        /// <returns>Vector3 representing p</returns>
		public static implicit operator Vector3(RndVec3 p)
		{
			return new Vector3(p.x, p.y, p.z);
		}


        /// <summary>
        /// Constructor for a RndVec3 from a Vector3
        /// </summary>
        /// <param name="p">input Vector3</param>
        /// <returns>RndVec3 representing p</returns>
		public static implicit operator RndVec3(Vector3 p)
		{
			return new RndVec3(p);
		}
	}
}
