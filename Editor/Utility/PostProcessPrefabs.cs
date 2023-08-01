using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Prefabs Post Process after asset importing
    /// </summary>
    public class PostProcessPrefabs : AssetPostprocessor
    {
        static List<PrefabPalette> s_Palettes = null;
        static List<string> s_PalettePaths = null;

        internal static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if(s_Palettes == null)
            {
                //Creates palettes lists for the first time
                s_PalettePaths = new List<string>();
                s_Palettes = new List<PrefabPalette>();

                var guids = AssetDatabase.FindAssets("t:" + typeof(PrefabPalette));
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    s_PalettePaths.Add(path);
                    s_Palettes.Add(AssetDatabase.LoadAssetAtPath<PrefabPalette>(path));
                }
            }
            else
            {
                //Update lists if palettes are added
                foreach(string assetPath in importedAssets)
                {
                    if(AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(PrefabPalette)
                        && !s_PalettePaths.Contains(assetPath))
                    {
                        s_PalettePaths.Add(assetPath);
                        s_Palettes.Add(AssetDatabase.LoadAssetAtPath<PrefabPalette>(assetPath));
                    }
                }

                //Update lists if palettes are removed
                foreach (string assetPath in deletedAssets)
                {
                    if (s_PalettePaths.Contains(assetPath))
                    {
                        //Remove palettes from the list
                        var index = s_PalettePaths.IndexOf(assetPath);
                        s_Palettes.RemoveAt(index);
                        s_PalettePaths.RemoveAt(index);
                        break;
                    }
                }
            }

            if (s_Palettes.Count == 0 || deletedAssets.Length == 0)
                return;

            RemovedDeletedPrefabFromLoadout();

            // Find out deleted prefabs and put them in a dictionnary to delete
            Dictionary<PrefabPalette, List<PrefabAndSettings>> toDelete = new Dictionary<PrefabPalette, List<PrefabAndSettings>>();
            foreach (PrefabPalette palette in s_Palettes)
            {
                foreach (PrefabAndSettings settings in palette.prefabs)
                {
                    if (settings.gameObject == null)
                    {
                        if (!toDelete.ContainsKey(palette))
                        {
                            toDelete.Add(palette, new List<PrefabAndSettings>() { settings });
                        }
                        else
                        {
                            toDelete[palette].Add(settings);
                        }
                    }
                }
            }

            // Delete the deleted prefabs from all the PrefabPalettes they were contained in
            foreach (PrefabPalette palette in toDelete.Keys)
            {
                foreach (PrefabAndSettings settings in toDelete[palette])
                {
                    palette.prefabs.Remove(settings);
                }
                EditorUtility.SetDirty(palette);
            }

        }

        private static void RemovedDeletedPrefabFromLoadout()
        {
            // If the prefab paint mode is the current one in polybrush,
            // and the prefab that has just been deleted is in the loadout,
            // Need to remove it from there or error spam will occur
            PolybrushEditor editor = PolybrushEditor.instance;
            if (editor == null || editor.tool != BrushTool.Prefab)
            {
                return;
            }
            BrushModePrefab brushMode = (BrushModePrefab)editor.mode;
            PrefabLoadoutEditor loadouteditor = brushMode.prefabLoadoutEditor;
            if (loadouteditor == null)
            {
                return;
            }

            List<LoadoutInfo> toRemove = new List<LoadoutInfo>();
            foreach (LoadoutInfo info in loadouteditor.CurrentLoadout)
            {
                if (info.prefab == null)
                {
                    toRemove.Add(info);
                }
            }

            foreach (LoadoutInfo info in toRemove)
            {
                loadouteditor.RemovePrefabFromLoadout(info);
            }

            // Clear the list of selected items in the current PrefabPalette
            // NOTE: This is not ideal, but it's easier to make it this way for now
            // a solution would be to keep a reference to the deleted items before deleting them
            // then make a comparison with the new list, to keep selected only the ones that were
            // not deleted and refresh the indices of the selected list
            loadouteditor.prefabPaletteEditors[loadouteditor.currentPalette].selected.Clear();
        }
    }
}
