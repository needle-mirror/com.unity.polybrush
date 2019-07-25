using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;


namespace UnityEditor.Polybrush
{
    /// <summary>
    /// An intermediary between the BrushModePrefab and multiple PrefabPaletteEditors
    /// to be able to keep a list of loadouts for painting
    /// </summary>
    internal class PrefabLoadoutEditor
    {
        /// <summary>
        /// Storage for user loadouts.
        /// </summary>
        [UserSetting]
        static Pref<PrefabLoadout> s_UserLoadout = new Pref<PrefabLoadout>("ScatteringEditor.userLoadout", new PrefabLoadout(new List<LoadoutInfo>()), SettingsScope.Project);

        static class Styles
        {
            public static readonly GUIContent brushLoadoutLabel = new GUIContent("Brush Loadout", "Currently loaded prefabs for painting");
            public static readonly GUIContent copyPrefabSettingsLabel = new GUIContent("Copy prefab settings", "");
            public static readonly GUIContent pastePrefabSettingsLabel = new GUIContent("Paste prefab settings", "");

            public static GUIStyle deleteButtonStyle = null;

            public static bool initialized = false;

            public static void Initialize()
            {
                deleteButtonStyle = new GUIStyle(GUI.skin.button);
                deleteButtonStyle.normal.background = IconUtility.GetIcon("PaintPrefabs/Delete");
                deleteButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                deleteButtonStyle.fixedHeight = deleteButtonStyle.normal.background.width;
                deleteButtonStyle.fixedWidth = deleteButtonStyle.normal.background.width;
                deleteButtonStyle.hover.background = deleteButtonStyle.active.background = null;

                initialized = true;
            }
        }

        internal Dictionary<PrefabPalette, PrefabPaletteEditor> prefabPaletteEditors;
        internal PrefabPalette currentPalette;
        internal LoadoutInfo copyPastePrefabSettings = null;
        internal LoadoutInfo toUnload;

        List<LoadoutInfo> m_CurrentLoadouts;

        internal List<LoadoutInfo> CurrentLoadout
        {
            get { return m_CurrentLoadouts; }
        }

        public PrefabLoadoutEditor(List<PrefabPalette> palettes, PrefabPalette startingPalette)
        {
            currentPalette = startingPalette;

            prefabPaletteEditors = new Dictionary<PrefabPalette, PrefabPaletteEditor>();
            m_CurrentLoadouts = s_UserLoadout.value.infos;

            RefreshPalettesList(palettes);
        }

        void SaveUserCurrentLoadouts()
        {
            s_UserLoadout.value = new PrefabLoadout(m_CurrentLoadouts);
            PolybrushSettings.Save();
        }

        internal void OnInspectorGUI_Internal(int thumbSize)
        {
            if (!Styles.initialized)
                Styles.Initialize();

            DrawLoadoutList(thumbSize);
            if (prefabPaletteEditors[currentPalette] != null)
                prefabPaletteEditors[currentPalette].OnInspectorGUI_Internal(thumbSize);
        }

        internal void RefreshPalettesList(List<PrefabPalette> palettes)
        {
            prefabPaletteEditors.Clear();
            foreach (PrefabPalette p in palettes)
            {
                var editor = (PrefabPaletteEditor)Editor.CreateEditor(p);
                editor.loadoutEditor = this;
                prefabPaletteEditors.Add(p, editor);
            }

            SyncLoadoutWithPalettes();
        }

        /// <summary>
        /// Show the list of current loadouts
        /// </summary>
        /// <param name="thumbSize">Size of the preview texture</param>
        internal void DrawLoadoutList(int thumbSize)
        {
            SyncLoadoutWithPalettes();

            int count = m_CurrentLoadouts.Count;

            EditorGUILayout.LabelField(Styles.brushLoadoutLabel);

            Rect backGroundRect = EditorGUILayout.BeginVertical(PrefabPaletteEditor.paletteStyle);
            backGroundRect.width = EditorGUIUtility.currentViewWidth;
            if (count == 0)
            {
                var r = EditorGUILayout.BeginVertical(GUILayout.Height(thumbSize+4));
                    EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin ? PolyGUI.k_BoxBackgroundDark : PolyGUI.k_BoxBackgroundLight);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Select items from the Palette below", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                return;
            }
            
            const int pad = 4;
            int size = thumbSize + pad;
            backGroundRect.x += 8;
            backGroundRect.y += 4;
            // The backgroundRect is currently as wide as the current view.
            // Adjust it to take the size of the vertical scrollbar and padding into account.
            backGroundRect.width -= (20 + (int)GUI.skin.verticalScrollbar.fixedWidth);
            // size variable will not take into account the padding to the right of all the thumbnails,
            // therefore it needs to be substracted from the width.
            int container_width = ((int)Mathf.Floor(backGroundRect.width) - (pad + 1));
            int columns = (int)Mathf.Floor(container_width / size);
            if (columns == 0) columns = 1;
            int rows = count / columns + (count % columns == 0 ? 0 : 1);
            if (rows < 1) rows = 1;

            backGroundRect.height = 8 + rows * thumbSize + (rows - 1) * pad;
            EditorGUI.DrawRect(backGroundRect, EditorGUIUtility.isProSkin ? PolyGUI.k_BoxBackgroundDark : PolyGUI.k_BoxBackgroundLight);

            int currentIndex = 0;

            for (int i = 0; i < rows; i++)
            {
                var horizontalRect = EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < columns; j++)
                {
                    LoadoutInfo loadoutInfo = m_CurrentLoadouts[currentIndex];
                    PrefabPaletteEditor prefabEditor = prefabPaletteEditors[loadoutInfo.palette];
                    SerializedProperty prefabs = prefabEditor.prefabs;
                    SerializedProperty prefab = prefabs.GetArrayElementAtIndex(loadoutInfo.palette.FindIndex(loadoutInfo.prefab));

                    var previewRectXPos = pad + j * size + horizontalRect.x;
                    DrawSingleLoadout(prefab, thumbSize, loadoutInfo, previewRectXPos, horizontalRect.y);
                    if (j != columns - 1)
                        GUILayout.Space(pad);
                    currentIndex++;
                    if (currentIndex >= count)
                        break;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
            EditorGUILayout.EndVertical();

            if (toUnload != null)
            {
                RemovePrefabFromLoadout(toUnload);
                toUnload = null;
                SaveUserCurrentLoadouts();
            }
        }

        /// <summary>
        /// Draw a single loadout
        /// </summary>
        /// <param name="loadout">The loadout being drawn</param>
        /// <param name="thumbSize">Size of the preview</param>
        /// <param name="infos">additionnal infos about the loadout being drawn (parent prefab palette and index in it)</param>
        void DrawSingleLoadout(SerializedProperty loadout, int thumbSize, LoadoutInfo infos, float x, float y)
        {
            var editor = prefabPaletteEditors[infos.palette];
            editor.serializedObject.Update();
            Rect r = new Rect(x, y, thumbSize, thumbSize);

            // Texture Preview
            Texture2D preview = PreviewsDatabase.GetAssetPreview(loadout.FindPropertyRelative("gameObject").objectReferenceValue);
            EditorGUI.DrawPreviewTexture(r, preview);
            float pad = thumbSize * 0.05f;

            GUILayoutUtility.GetRect(thumbSize, thumbSize);

            Rect r3 = new Rect(r);
            r3.width = Styles.deleteButtonStyle.fixedWidth;
            r3.height = Styles.deleteButtonStyle.fixedHeight;
            r3.x += thumbSize - pad - r3.width;
            r3.y += pad;
            if (GUI.Button(r3, "", Styles.deleteButtonStyle))
            {
                toUnload = infos;
                GUI.changed = true;
            }
            r.y += thumbSize - pad - 10;
            r.x += pad;
            r.width = thumbSize - (2 * pad);
            var prefabOccurence = loadout.FindPropertyRelative("settings").FindPropertyRelative("m_Strength");
            prefabOccurence.floatValue = GUI.HorizontalSlider(r, prefabOccurence.floatValue, BrushModePrefab.k_PrefabOccurrenceMin, BrushModePrefab.k_PrefabOccurrenceMax);

            editor.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Switch to a new Palette Edition, which means switching paletteEditor too
        /// </summary>
        /// <param name="palette">the target palette</param>
        internal void ChangePalette(PrefabPalette palette)
        {
            if (!prefabPaletteEditors.ContainsKey(palette))
            {
                var editor = (PrefabPaletteEditor)Editor.CreateEditor(palette);
                editor.loadoutEditor = this;
                prefabPaletteEditors.Add(palette, editor);
            }
            currentPalette = palette;
        }

        /// <summary>
        /// Returns a random PrefabAndSettings from the loadout list
        /// </summary>
        /// <returns></returns>
        internal PrefabAndSettings GetRandomLoadout()
        {
            if (m_CurrentLoadouts.Count < 1)
                return null;

            // Weighted random implementation
            List<float> weights = new List<float>() { 0.0f };
            float totalWeights = 0.0f;

            foreach (LoadoutInfo info in m_CurrentLoadouts)
            {
                float strength = info.palette.Get(info.prefab).settings.strength;
                weights.Add(totalWeights + strength);
                totalWeights += strength;
            }

            float random = UnityEngine.Random.Range(0.0f + Mathf.Epsilon, totalWeights);

            int resultIndex = -1;
            for(int i = 0; i < weights.Count - 1; i++)
            {
                if(weights[i] < random  && random < weights[i + 1])
                {
                    resultIndex = i;
                    break;
                }
            }

            if (resultIndex == -1)
                return null;

            LoadoutInfo loadout = m_CurrentLoadouts[resultIndex];
            return loadout.palette.Get(loadout.prefab);
        }

        internal bool ContainsPrefabInstance(GameObject instance)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(instance);

            if (prefab == null)
                return false;

            return ContainsPrefab(prefab);
        }

        internal bool ContainsPrefab(GameObject prefab)
        {
            foreach (var loadoutInfo in m_CurrentLoadouts)
                if (loadoutInfo.palette.Contains(prefab))
                    return true;
            return false;
        }

        /// <summary>
        /// Show the Menu to copu/paste prefab placement settings
        /// </summary>
        /// <param name="info">The PrefabAndSettings which was right clicked</param>
        /// <param name="selected">The list of selected PlacementSettings in the current PrefabPalette</param>
        internal void OpenCopyPasteMenu(LoadoutInfo info, HashSet<int> selected)
        {
            GenericMenu menu = new GenericMenu();
            if(selected.Count > 1)
                menu.AddDisabledItem(Styles.copyPrefabSettingsLabel);
            else
                menu.AddItem(Styles.copyPrefabSettingsLabel, false, () => { CopyPasteSettings(info, true, selected); });

            menu.AddItem(Styles.pastePrefabSettingsLabel, false, () => { CopyPasteSettings(info, false, selected); });

            menu.ShowAsContext();
        }

        /// <summary>
        /// Copy or paste settings to selected PlacementSettings
        /// </summary>
        /// <param name="loadout">The loadout that got clicked</param>
        /// <param name="copy">do we copy or paste ?</param>
        /// <param name="selected">the list of currently selected PlacementSettings for edition in the current PrefabPalette</param>
        private void CopyPasteSettings(LoadoutInfo loadout, bool copy, HashSet<int> selected)
        {
            if (copy)
            {
                copyPastePrefabSettings = loadout;
            }else
            {
                if (copyPastePrefabSettings == null)
                    return;

                PrefabPaletteEditor sourceEditor = prefabPaletteEditors[copyPastePrefabSettings.palette];
                PrefabPaletteEditor destEditor = prefabPaletteEditors[loadout.palette];
                SerializedProperty srcPAS = sourceEditor.prefabs.GetArrayElementAtIndex(loadout.palette.FindIndex(loadout.prefab));
                SerializedProperty srcPS = srcPAS.FindPropertyRelative("settings");
                destEditor.serializedObject.Update();
                foreach (int i in selected)
                {
                    SerializedProperty destPAS = destEditor.prefabs.GetArrayElementAtIndex(i);
                    SerializedProperty destPS = destPAS.FindPropertyRelative("settings");
                    PlacementSettings.CopySerializedProperty(srcPS, destPS);
                }
                destEditor.serializedObject.ApplyModifiedProperties();
            }
        }

        internal void AddPrefabInLoadout(LoadoutInfo loadoutInfo)
        {
            if (!m_CurrentLoadouts.Contains(loadoutInfo))
                m_CurrentLoadouts.Add(loadoutInfo);
            SaveUserCurrentLoadouts();
        }

        internal bool ContainsPrefab(LoadoutInfo loadoutInfo)
        {
            return m_CurrentLoadouts.Contains(loadoutInfo);
        }

        internal void RemovePrefabFromLoadout(LoadoutInfo loadoutInfo)
        {
            if (m_CurrentLoadouts.Contains(loadoutInfo))
                m_CurrentLoadouts.Remove(loadoutInfo);
            SaveUserCurrentLoadouts();
        }

        /// <summary>
        /// Remove null references if palettes have been modified.
        /// </summary>
        private void SyncLoadoutWithPalettes()
        {
            if (m_CurrentLoadouts != null)
            {
                m_CurrentLoadouts.RemoveAll(x => x.palette == null || !x.IsValid());

                SaveUserCurrentLoadouts();
            }
        }
    }
}
