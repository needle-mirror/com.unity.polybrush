using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Polybrush
{
    [System.Serializable]
    internal class PrefabLoadout
    {
        [SerializeField]
        internal List<LoadoutInfo> infos = new List<LoadoutInfo>();

        public PrefabLoadout(List<LoadoutInfo> infos)
        {
            this.infos = infos;
        }

        public bool IsValid()
        {
            if (infos == null)
                return false;

            foreach (LoadoutInfo info in infos)
            {
                if (info.palette == null)
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Utility to identify a specific PrefabAndSettings in a PrefabPalette
    /// </summary>
    [System.Serializable]
    internal class LoadoutInfo
    {
        [SerializeField]
        internal PrefabPalette palette;
        [SerializeField]
        internal GameObject prefab;

        public LoadoutInfo(PrefabPalette p, int index)
        {
            palette = p;
            prefab = p.prefabs[index].gameObject;
        }

        public override bool Equals(object obj)
        {
            var other = obj as LoadoutInfo;
            if (other != null)
            {
                if (palette != other.palette) return false;
                if (prefab != other.prefab) return false;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool IsValid()
        {
            return (palette != null && prefab != null && palette.Contains(prefab));
        }

        public override int GetHashCode()
        {
            var hashCode = 2049644202;
            hashCode = hashCode * -1521134295 + EqualityComparer<PrefabPalette>.Default.GetHashCode(palette);
            hashCode = hashCode * -1521134295 + palette.Get(prefab).GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<PrefabPalette>.Default.GetHashCode(palette);
            return hashCode;
        }
    }
}
