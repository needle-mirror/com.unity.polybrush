#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Overrides the default scene zoom with the current values.
    /// </summary>
    internal class ZoomOverride : MonoBehaviour
	{
		// The current weights applied to this mesh
		protected float[] weights;

		// Normalized brush strength
		protected float normalizedStrength;

		internal virtual void SetWeights(float[] weights, float normalizedStrength)
		{
			this.weights = weights;
			this.normalizedStrength = normalizedStrength;
		}

		internal virtual float[] GetWeights()
		{
			return weights;
		}

		internal Mesh Mesh
		{
			get
			{
                return gameObject.GetMesh();
			}
		}

        /// <summary>
        /// Let the temp mesh know that vertex positions have changed.
        /// </summary>
        /// <param name="mesh"></param>
        internal virtual void OnVerticesMoved(PolyMesh mesh) {}

		protected virtual void OnEnable()
		{
			this.hideFlags = HideFlags.HideAndDontSave;

			Component[] other = GetComponents<ZoomOverride>();

			foreach(Component c in other)
				if(c != this)
					GameObject.DestroyImmediate(c);
		}
	}
}
#endif
