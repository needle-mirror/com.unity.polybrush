using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Polybrush
{
    public class PostProcessPrefabs : AssetPostprocessor
    {

        internal static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            List<PrefabPalette> palettes = PolyEditorUtility.GetAll<PrefabPalette>();

            if (palettes.Count == 0 || deletedAssets.Length == 0)
            {
                return;
            }

            RemovedDeletedPrefabFromloadout();

            // Find out deleted prefabs and put them in a dictionnary to delete
            Dictionary<PrefabPalette, List<PrefabAndSettings>> toDelete = new Dictionary<PrefabPalette, List<PrefabAndSettings>>();
            foreach (PrefabPalette palette in palettes)
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

        private static void RemovedDeletedPrefabFromloadout()
        {
            // If the prefab paint mode is the current one in polybrush, 
            // and the prefab that has just been deleted is in the loadout,
            // Need to remove it from there or error spam will occur
            PolyEditor editor = PolyEditor.instance;
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
