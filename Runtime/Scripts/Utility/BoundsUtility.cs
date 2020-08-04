namespace UnityEngine.Polybrush
{
    internal static class BoundsUtility
    {
        internal struct SphereBounds
        {
            internal Vector3 position;
            internal float radius;

            /// <summary>
            /// Create a sphere bound based on a position and a radius.
            /// </summary>
            /// <param name="p">Position</param>
            /// <param name="r">Radius</param>
            internal SphereBounds(Vector3 p, float r)
            {
                position = p;
                radius = r;
            }

            /// <summary>
            /// Return true if the other sphere is in the radius of this sphere bound.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            internal bool Intersects(SphereBounds other)
            {
                return Vector3.Distance(position, other.position) < (radius + other.radius);
            }
        }

        /// <summary>
        /// Create a spherical bounds inside of a Renderer bounds.
        /// It'll take every meshes found in the hierarchy into account.
        /// </summary>
        /// <param name="go">GameObject having Renderer component(s).</param>
        /// <param name="bounds">Sphere bounds instance in which sphere information will be store.</param>
        /// <returns>True if bounds data has been set.</returns>
        internal static bool GetSphereBounds(GameObject go, out SphereBounds bounds)
        {
            Bounds entireObjectBounds = GetHierarchyBounds(go);

            bounds = default(SphereBounds);

            if (entireObjectBounds.size == Vector3.zero)
                return false;

            bounds.position = entireObjectBounds.center;
            bounds.radius = Mathf.Max(entireObjectBounds.extents.x, entireObjectBounds.extents.z);

            return true;
        }

        /// <summary>
        /// Create bounds based on meshes found in the given GameObject hierarchy.
        /// </summary>
        /// <param name="parent">Root object.</param>
        /// <returns>New bounds around the given GameObject.
        /// If GameObject has no Renderer, bounds will have a size of zero.</returns>
        internal static Bounds GetHierarchyBounds(GameObject parent)
        {
            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

            Bounds bounds = default(Bounds);

            if (renderers.Length == 0)
                return bounds;

            for (int i = 0; i < renderers.Length; ++i)
            {
                Bounds it = renderers[i].bounds;

                if (i == 0)
                {
                    // World position
                    bounds.center = it.center;
                }

                bounds.Encapsulate(it.max);
                bounds.Encapsulate(it.min);
            }

            return bounds;
        }
    }
}
