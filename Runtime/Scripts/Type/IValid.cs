namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Interface for objects that may be null or otherwise invalid for use.
    /// \sa EditableObject, BrushTarget, Util.IsValid
    /// </summary>
    internal interface IValid
	{
		bool IsValid { get; }
	}
}
