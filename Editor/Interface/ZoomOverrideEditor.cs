using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Custom Editor for the ZoomOverride
    /// </summary>
	[CustomEditor(typeof(ZoomOverride), true)]
	internal class ZoomOverrideEditor : Editor
	{
		void OnEnable()
		{
			if(PolybrushEditor.instance == null)
				GameObject.DestroyImmediate(this.target);
		}

		public override void OnInspectorGUI() {}

		bool HasFrameBounds()
		{
			ZoomOverride ren = (ZoomOverride) target;
			return 	ren.Mesh != null && ren.GetWeights().Length == ren.Mesh.vertexCount;
		}

		Bounds OnGetFrameBounds()
		{
			ZoomOverride ren = (ZoomOverride) target;

			Mesh m = ren.Mesh;

			Vector3[] vertices = m.vertices;
			float[] weights = ren.GetWeights();

			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			int appliedWeights = 0;

			Transform transform = ((ZoomOverride)target).transform;

			for(int i = 0; i < m.vertexCount; i++)
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
				bounds = ren.transform.GetComponent<MeshRenderer>().bounds;
			else if(appliedWeights == 1 || bounds.size.magnitude < .1f)
				bounds.size = Vector3.one * .5f;

			return bounds;
		}
	}
}
