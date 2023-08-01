#if UNITY_EDITOR

using UnityEngine;

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
            this.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

			Component[] other = GetComponents<ZoomOverride>();

			foreach(Component c in other)
				if(c != this)
					GameObject.DestroyImmediate(c);
		}

        internal bool HasFrameBounds()
        {
            return 	Mesh != null && weights.Length == Mesh.vertexCount;
        }

        internal Bounds OnGetFrameBounds()
        {
            Vector3[] vertices = Mesh.vertices;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            int appliedWeights = 0;

            for(int i = 0; i < Mesh.vertexCount; i++)
            {
                if(weights[i] > 0.0001f)
                {
                    if(appliedWeights > 0)
                        bounds.Encapsulate( transform.TransformPoint(vertices[i]));
                    else
                        bounds.center = transform.TransformPoint(vertices[i]);

                    appliedWeights++;
                }
            }

            if(appliedWeights < 1)
                bounds = transform.GetComponent<MeshRenderer>().bounds;
            else if(appliedWeights == 1 || bounds.size.magnitude < .1f)
                bounds.size = Vector3.one * .5f;

            return bounds;
        }
	}
}
#endif
