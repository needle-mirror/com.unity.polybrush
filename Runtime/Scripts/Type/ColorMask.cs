namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Mask Used for Vertex Color Painting mode 
    /// </summary>
	internal struct ColorMask
	{
		internal bool r, g, b, a; 

		internal ColorMask(bool r, bool g, bool b, bool a)
		{
			this.r = r;
			this.b = b;
			this.g = g;
			this.a = a;
		}
	}
}
	
