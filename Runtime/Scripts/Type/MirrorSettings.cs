namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Brush mirror settings.
    /// Used to define brush areas and placement when Axes is defined.
    /// </summary>
    [System.Serializable]
    internal struct MirrorSettings
    {
        /// <summary>
        /// Mask of active axes. Set value to None for no mirroring.
        /// </summary>
        public BrushMirror Axes;

        /// <summary>
        /// Space coordinate in which the brush ray will be flipped.
        /// </summary>
        public MirrorCoordinateSpace Space;
    }
}
