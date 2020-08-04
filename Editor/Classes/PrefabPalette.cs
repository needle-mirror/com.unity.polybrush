using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// A set of Prefabs.
    /// </summary>
    [CreateAssetMenuAttribute(menuName = "Polybrush/Prefab Palette", fileName = "Prefab Palette", order = 802)]
	[System.Serializable]
	internal class PrefabPalette : PolyAsset, IHasDefault, ICustomSettings
	{
        [SerializeField] internal List<PrefabAndSettings> prefabs;

        public string assetsFolder { get { return "Prefab Palette/"; } }

        public void SetDefaultValues()
		{
			prefabs = new List<PrefabAndSettings>() {};
		}

        public bool Contains(GameObject prefab)
        {
            return prefabs.Find(x => x.gameObject == prefab) != null;
        }

        public int FindIndex(GameObject prefab)
        {
            return prefabs.IndexOf(Get(prefab));
        }

        public PrefabAndSettings Get(GameObject prefab)
        {
            return prefabs.Find(x => x.gameObject == prefab);
        }

        public void RemoveRange(IList<int> indexes)
        {
            List<PrefabAndSettings> toRemove = new List<PrefabAndSettings>(indexes.Count);
            foreach (int i in indexes)
                toRemove.Add(prefabs[i]);

            prefabs.RemoveAll(x => toRemove.Contains(x));
        }
    }
}
