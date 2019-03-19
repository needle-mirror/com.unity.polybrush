using UnityEngine;
using UnityEngine.Polybrush;

/// <summary>
/// Class only use for unit test the function PolyEditorUtility.GetFirstOrNew<T>()
/// </summary>
public class UnitTestGenericType : ScriptableObject, IHasDefault, ICustomSettings
{
    public string assetsFolder
    {
        get
        {
            return "UnitTestGenericType/";
        }
    }

    public void SetDefaultValues()
    {
        
    }
}
