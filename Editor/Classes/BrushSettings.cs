using UnityEngine;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Collection of settings for a sculpting brush.
    /// </summary>
    [CreateAssetMenuAttribute(menuName = "Polybrush/Brush Settings Preset", fileName = "Brush Settings", order = 800)]
    [System.Serializable]
    internal class BrushSettings : PolyAsset, IHasDefault, ICustomSettings
    {

        [SerializeField] internal float brushRadiusMin = 0.001f;
        [SerializeField] internal float brushRadiusMax = 5f;

        /// The total affected radius of this brush.
        [SerializeField] private float _radius = 1f;

        /// At what point the strength of the brush begins to taper off.
        [SerializeField] float _falloff = .5f;

        /// How may times per-second a mouse click will apply a brush stroke.
        [SerializeField] float _strength = 1f;

        [SerializeField] AnimationCurve _curve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f, -3f, -3f)
            );

        public string assetsFolder { get { return "Brush Settings/"; } }

        internal AnimationCurve falloffCurve
        {
            get
            {
                return _curve;
            }
            set
            {
                _curve = allowNonNormalizedFalloff ? value : Util.ClampAnimationKeys(value, 0f, 1f, 1f, 0f);
            }
        }

        /// If true, the falloff curve won't be clamped to keyframes at 0,0 and 1,1.
        public bool allowNonNormalizedFalloff = false;

        /// The total affected radius of this brush.
        internal float radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = Mathf.Clamp(value, brushRadiusMin, brushRadiusMax);
                _radius = Mathf.Round(_radius * 1000f) / 1000f;
            }
        }

        /// <summary>
        /// At what point the strength of the brush begins to taper off.
        /// 0 means the strength tapers from the center of the brush to the edge.
        /// 1 means the strength is 100% all the way through the brush.
        /// .5 means the strength is 100% through 1/2 the radius then tapers to the edge.
        /// </summary>
        internal float falloff
        {
            get
            {
                return _falloff;
            }

            set
            {
                _falloff = Mathf.Clamp(value, 0f, 1f);
                _falloff = Mathf.Round(_falloff * 1000f) / 1000f;
            }
        }

        internal float strength
        {
            get
            {
                return _strength;
            }

            set
            {
                _strength = Mathf.Clamp(value, 0f, 1f);
                _strength = Mathf.Round(_strength * 1000f) / 1000f;
            }
        }

        /// <summary>
        /// Radius value scaled to 0-1.
        /// </summary>
        internal float normalizedRadius
        {
            get
            {
                return (_radius - brushRadiusMin) / (brushRadiusMax - brushRadiusMin);
            }
        }

        ///whether the user is holding control or not
        internal bool isUserHoldingControl { get; set; }
        ///whether the user is holding shift or not
        internal bool isUserHoldingShift { get; set; }

        /// <summary>
        /// Set the object's default values.
        /// </summary>
        public void SetDefaultValues()
        {
            brushRadiusMin = 0.001f;
            brushRadiusMax = 5f;

            radius = 1f;
            falloff = .5f;
            strength = 1f;
        }

        /// <summary>
        /// Deep copy this
        /// </summary>
        /// <returns>A new object with the same values as this</returns>
		internal BrushSettings DeepCopy()
		{
			BrushSettings copy = ScriptableObject.CreateInstance<BrushSettings>();
			this.CopyTo(copy);
			return copy;
		}

        /// <summary>
        /// Copy all properties to target
        /// </summary>
        /// <param name="target"></param>
        internal void CopyTo(BrushSettings target)
		{
			target.name 							= this.name;
			target.brushRadiusMin					= this.brushRadiusMin;
			target.brushRadiusMax					= this.brushRadiusMax;
			target._radius							= this._radius;
			target._falloff							= this._falloff;
			target._strength						= this._strength;
			target._curve							= new AnimationCurve(this._curve.keys);
			target.allowNonNormalizedFalloff		= this.allowNonNormalizedFalloff;
		}
    }
}
