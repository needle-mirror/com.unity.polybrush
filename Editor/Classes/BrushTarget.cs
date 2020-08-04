using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Stores information about the object a brush is currently hovering.
    /// </summary>
    internal class BrushTarget : IValid
	{
		// List of hit locations on this target mesh.
		internal List<PolyRaycastHit> raycastHits = new List<PolyRaycastHit>();

		private float[] _weights = null;

		// The GameObject the brush is currently hovering.
		[SerializeField] EditableObject _editableObject = null;

		// Getter for editableObject target
		internal EditableObject editableObject { get { return _editableObject; } }

		// Convenience getter for editableObject.gameObject
		internal GameObject gameObject { get { return editableObject == null ? null : editableObject.gameObjectAttached; } }

		// Convenience getter for editableObject.gameObject.transform
		internal Transform transform { get { return editableObject == null ? null : editableObject.gameObjectAttached.transform; } }

		// Convenience getter for gameObject.transform.localToWorldMatrix
		internal Matrix4x4 localToWorldMatrix { get { return editableObject == null ? Matrix4x4.identity : editableObject.gameObjectAttached.transform.localToWorldMatrix; } }

		// Convenience getter for editableObject.editMesh.vertexCount
		internal int vertexCount { get { return _editableObject.editMesh.vertexCount; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editableObject"></param>
        internal BrushTarget(EditableObject editableObject) : this(editableObject, new List<PolyRaycastHit>()) {}

        /// <summary>
        /// Explicit constructor.
        /// </summary>
        /// <param name="editableObject"></param>
        /// <param name="hits"></param>
        internal BrushTarget(EditableObject editableObject, List<PolyRaycastHit> hits)
		{
			this.raycastHits = hits;
			this._editableObject = editableObject;
            if (this.editableObject != null)
                this._weights = new float[this._editableObject.editMesh.vertexCount];
            else
                this._weights = new float[0];
		}

		~BrushTarget()
		{}

        /// <summary>
        /// Clear the Raycasts
        /// </summary>
		internal void ClearRaycasts()
		{
			foreach(PolyRaycastHit hit in raycastHits)
				hit.ReleaseWeights();

			raycastHits.Clear();
		}

        /// <summary>
        /// Returns an array of weights where each index is the max of all raycast hits.
        /// </summary>
        /// <param name="rebuildCache"></param>
        /// <returns></returns>
        internal float[] GetAllWeights(bool rebuildCache = false)
		{
			PolyMesh mesh = editableObject.editMesh;
			int vertexCount = mesh.vertexCount;

			if(mesh == null)
				return null;

            if (vertexCount != _weights.Length)
            {
                _weights = new float[vertexCount];
                rebuildCache = true;
            }

			if(!rebuildCache)
				return _weights;

			for(int i = 0; i < vertexCount; i++)
				_weights[i] = 0f;

			for(int i = 0; i < raycastHits.Count; i++)
			{
				if(raycastHits[i].weights != null)
				{
					float[] w = raycastHits[i].weights;

					for(int n = 0; n < vertexCount; n++)
						if(w[n] > _weights[n])
							_weights[n] = w[n];
				}
			}

			return _weights;
		}

		public bool IsValid { get { return editableObject.IsValid(); } }

		public override string ToString()
		{
			return string.Format("valid: {0}\nvertices: {1}", IsValid, IsValid ? editableObject.vertexCount : 0);
		}
	}
}
