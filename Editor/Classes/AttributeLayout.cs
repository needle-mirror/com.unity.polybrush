using System;
using UnityEditor;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// JsonUtility cannot serialize arrays, but can handle classes with arrays.
    /// </summary>
    [System.Serializable]
	internal class AttributeLayoutContainer : ScriptableObject, IEquatable<AttributeLayoutContainer>
	{
	    [SerializeField] internal string shaderPath;
        [SerializeField] internal AttributeLayout[] attributes;

        [Obsolete("Field is deprecated.")]
	    internal Shader shader;

        internal static AttributeLayoutContainer Create(Shader shader, AttributeLayout[] attributes)
		{
			AttributeLayoutContainer container = ScriptableObject.CreateInstance<AttributeLayoutContainer>();
			container.attributes = attributes;
            container.shaderPath = shader.name;
			return container;
		}

		public bool Equals(AttributeLayoutContainer other)
		{
			if(shaderPath != other.shaderPath)
				return false;

			int a = attributes == null ? 0 : attributes.Length;
			int b = other.attributes == null ? 0 : other.attributes.Length;

			if(a != b)
				return false;

			for(int i = 0; i < a; ++i)
				if(!attributes[i].Equals(other.attributes[b]))
					return false;

			return true;
		}

        internal bool HasAttributes(string textureProperty)
        {
            return GetAttributes(textureProperty) != null;
        }

        internal AttributeLayout GetAttributes(string textureProperty)
        {
            if (attributes == null)
                return null;
            
            return Array.Find(attributes, attr => attr.propertyTarget == textureProperty);
        }

        internal void AddAttribute(AttributeLayout layout)
        {
            if (attributes == null)
                attributes = new AttributeLayout[0];
            
            ArrayUtility.Add(ref attributes, layout);
        }

        internal void RemoveAttribute(string propertyName)
        {
            RemoveAttribute(GetAttributes(propertyName));
        }
        
        internal void RemoveAttribute(AttributeLayout layout)
        {
            if (layout == null)
                throw new ArgumentNullException("layout");
            
            ArrayUtility.Remove(ref attributes, layout);
        }
	}

    /// <summary>
    /// AttributeLayout defines how Polybrush applies a value to a mesh.
    /// </summary>
    [System.Serializable]
	internal class AttributeLayout : IEquatable<AttributeLayout>
	{
		internal const int NoMask = -1;
		internal const int DefaultMask = 0;

		internal static readonly int[] DefaultMaskValues = new int[] {
			-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
		};

		internal static readonly GUIContent[] DefaultMaskDescriptions = new GUIContent[] {
			new GUIContent("No Mask"), new GUIContent("0"), new GUIContent("1"), new GUIContent("2"),
			new GUIContent("3"), new GUIContent("4"), new GUIContent("5"), new GUIContent("6"),
			new GUIContent("7"), new GUIContent("8"), new GUIContent("9"), new GUIContent("10"),
			new GUIContent("11"), new GUIContent("12"), new GUIContent("13"), new GUIContent("14"),
			new GUIContent("15"), new GUIContent("16"), new GUIContent("17"), new GUIContent("18"),
			new GUIContent("19"), new GUIContent("20"), new GUIContent("21"), new GUIContent("22"),
			new GUIContent("23"), new GUIContent("24"), new GUIContent("25"), new GUIContent("26"),
			new GUIContent("27"), new GUIContent("28"), new GUIContent("29"), new GUIContent("30"),
			new GUIContent("31")
		};

		internal static readonly Vector2 NormalizedRange = new Vector2(0f, 1f);

        /// <summary>
        /// The texture to place for vertices that have no other texture set.
        /// Can have max one per group/mask.
        /// </summary>
        [SerializeField] internal bool isBaseTexture = false;

        // Which mesh attribute to apply values to (color, tangent, uv, etc)
        [SerializeField] internal MeshChannel channel;

        // Which field (r,g,b,a / x,y,z,w)
        [SerializeField] internal ComponentIndex index;

        // How to scale the value when it's applied to the mesh
        [SerializeField] internal Vector2 range = new Vector2(0f, 1f);

        [SerializeField] internal float min { get { return range.x; } set { range.x = value; } }
        [SerializeField] internal float max { get { return range.y; } set { range.y = value; } }

        // The shader property that is controlled by this attribute.  If set to a valid
        // Texture2D property, that texture will be loaded and shown in the Texture Blend
        // Palette.
        // If propertyTarget is not a loadable texture property this string will be displayed
        // in place of an image.
        [SerializeField] internal string propertyTarget;

        // What masking group this value should normalize to.
        // -1 is reserved as no mask, 0 is reserved as default mask.
        [SerializeField] internal int mask = DefaultMask;

		// If this value controls the strength of a texture, this can be set to display
		// a preview texture in the splatweight editor.
		[System.NonSerialized] internal Texture2D previewTexture = null;

        internal AttributeLayout()
        {
            
        }
		
		internal AttributeLayout(MeshChannel channel, ComponentIndex index) : this(channel, index, Vector2.up, DefaultMask)
		{}

		internal AttributeLayout(MeshChannel channel, ComponentIndex index, Vector2 range, int mask, bool isBaseTexture = false)
		{
			this.channel = channel;
			this.index = index;
			this.range = range;
			this.mask = mask;
            this.isBaseTexture = isBaseTexture;
		}

		internal AttributeLayout(MeshChannel channel, ComponentIndex index, Vector2 range, int mask, string targetProperty, Texture2D texture = null)
			: this(channel, index, range, mask)
		{
			this.propertyTarget = targetProperty;
			this.previewTexture = texture;
		}

		public bool Equals(AttributeLayout other)
		{
			return 	channel == other.channel &&
					propertyTarget.Equals(other.propertyTarget) &&
					index == other.index &&
					range == other.range &&
					mask == other.mask &&
                    isBaseTexture == other.isBaseTexture;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}.{2} ({3:f2}, {4:f2})  {5}", propertyTarget, channel.ToString(), index.GetString(), min, max, mask);
		}
	}
}
