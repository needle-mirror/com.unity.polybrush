using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// A simplified version of UnityEngine.RaycastHit that only contains information we're interested in.
    /// </summary>
    internal class PolyRaycastHit
	{
		const int MAX_POOL_SIZE = 16;
		private static Queue<float[]> weightPool = new Queue<float[]>();

		/// Distance from the Raycast origin to the point of impact.
		internal float distance;
		/// The position in model space where a raycast intercepted a triangle.
		internal Vector3 position;
		/// The normal in model space of the triangle that this raycast hit.
		internal Vector3 normal;
		/// The triangle index of the hit face.
		internal int triangle;
		/// The vertices affected by this ray will have their weights stored here.
		internal float[] weights;

        internal PolyRaycastHit(float InDistance, Vector3 InPosition, Vector3 InNormal, int InTriangle)
		{
			this.distance 	= InDistance;
			this.position 	= InPosition;
			this.normal 	= InNormal;
			this.triangle 	= InTriangle;
			this.weights 	= null;
		}

		~PolyRaycastHit()
		{
			if(weights != null && weightPool.Count < MAX_POOL_SIZE)
				weightPool.Enqueue(weights);
		}

		internal void ReleaseWeights()
		{
			if(weights != null)
			{
				weightPool.Enqueue(weights);
				weights = null;
			}
		}

		internal void SetVertexCount(int vertexCount)
		{
			if(weightPool.Count > 0)
				weights = weightPool.Dequeue();

			if(weights == null || weights.Length < vertexCount)
			{
				PolyDebug.Log(string.Format("new alloc  	float[{0}]kb  pool.size = {1}", (sizeof(float) * vertexCount) / 1024, weightPool.Count), "#FF0000FF");
				weights = new float[vertexCount];
			}
			else
			{
				PolyDebug.Log(string.Format("re-use  	float[{0}]kb  pool.size = {1}", (sizeof(float) * vertexCount) / 1024, weightPool.Count),  "green");
			}
		}

        /// <summary>
        /// Summary of this struct.
        /// </summary>
        /// <returns>A string representing the summary of this struct</returns>
        public override string ToString()
		{
			return string.Format("p{0}, n{1}, w[{2}]", position, normal, weights == null ? "null" : weights.Length.ToString());
		}
	}
}
