using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Super class for every Polybrush Assets implementing Reset() function
/// so that when you reset them from the inspector they don't loose their name
/// </summary>
public class PolyAsset : ScriptableObject
{
    protected virtual void Reset()
    {
        string path = AssetDatabase.GetAssetPath(this.GetInstanceID());
        this.name = Path.GetFileNameWithoutExtension(path);
    }
}
