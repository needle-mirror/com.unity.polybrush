using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Store previews in an internal cache for quick access.
    /// </summary>
    internal static class PreviewsDatabase
	{
        /// <summary>
        /// Cache size. Also used to set the cache size of AssetPreview.
        /// </summary>
        static readonly int k_CacheSize = 128;

        /// <summary>
        /// Storage for previews.
        /// </summary>
        static Dictionary<Object, MeshPreview> s_Cache = null;

        /// <summary>
        /// Clear and unload cache.
        /// </summary>
        internal static void UnloadCache()
        {
            if (s_Cache == null)
                return;

            s_Cache.Clear();
            s_Cache = null;
        }

        /// <summary>
        /// Fetch preview from cache. Creates one if no cached preview is found
        /// for a given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static Texture2D GetAssetPreview(Object obj)
        {
            if (s_Cache == null)
            {
                s_Cache = new Dictionary<Object, MeshPreview>(k_CacheSize);
                AssetPreview.SetPreviewTextureCacheSize(k_CacheSize);
            }

            if (s_Cache.ContainsKey(obj) == false)
                s_Cache.Add(obj, new MeshPreview(obj));

            MeshPreview preview = s_Cache[obj];
            preview.UpdatePreview();

            return preview.previewTexture;
        }
	}
}
