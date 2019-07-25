using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnityEditor.Polybrush
{
    [CustomEditor(typeof(PrefabPalette))]
    public class PrefabPaletteEditor : Editor
    {
        internal SerializedProperty prefabs;
        internal HashSet<int> selected = new HashSet<int>();
        internal PrefabLoadoutEditor loadoutEditor;
        internal Action<IEnumerable<int>> onSelectionChanged = null;

        static internal GUIStyle paletteStyle = new GUIStyle();

        GUIContent m_GCCurrentPaletteLabel = new GUIContent("Current Palette", "Currently selected Prefabs Palette");
        GUIContent m_GCPlacementSettingsLabel = new GUIContent("Placement Settings", "Currently selected Prefab(s) settings");

        private void OnEnable()
        {
            paletteStyle.padding = new RectOffset(8, 8, 8, 8);
            prefabs = serializedObject.FindProperty("prefabs");
        }

        /// <summary>
        /// Creates a New PrefabPalette
        /// </summary>
        /// <returns>The Newly created PrefabPalette</returns>
        internal static PrefabPalette AddNew()
        {
            string path = PolyEditorUtility.UserAssetDirectory + "Prefab Palette";

            if (string.IsNullOrEmpty(path))
                path = "Assets";

            path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Prefab Palette.asset");

            if (!string.IsNullOrEmpty(path))
            {
                PrefabPalette palette = ScriptableObject.CreateInstance<PrefabPalette>();
                palette.SetDefaultValues();

                AssetDatabase.CreateAsset(palette, path);
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(palette);

                return palette;
            }

            return null;
        }

        public override void OnInspectorGUI()
        {
            if (loadoutEditor != null)
            {
                OnInspectorGUI_Internal(64);
            }
        }

        private bool IsDeleteKey(Event e)
        {
            return e.keyCode == KeyCode.Backspace;
        }

        /// <summary>
        /// Draw everything concerning a single Prefab Palette in the Polybrush Window
        /// </summary>
        /// <param name="thumbSize">size of the preview textures</param>
        internal void OnInspectorGUI_Internal(int thumbSize)
        {
            EditorGUILayout.LabelField(m_GCCurrentPaletteLabel);

            serializedObject.Update();
            int count = prefabs != null ? prefabs.arraySize : 0;
            Rect dropDownZone = EditorGUILayout.BeginVertical(paletteStyle);
            dropDownZone.width = EditorGUIUtility.currentViewWidth;
            Rect backGroundRect = new Rect(dropDownZone);

            if (count != 0)
            {
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
                        GUILayout.Space(pad);
                        var prefab = prefabs.GetArrayElementAtIndex(currentIndex);
                        var previewRectXPos = pad + j * size + horizontalRect.x;
                        DrawPrefabPreview(prefab, currentIndex, thumbSize, previewRectXPos, horizontalRect.y);
                        currentIndex++;
                        if (currentIndex >= count)
                            break;
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(pad);
                }

                EditorGUILayout.EndVertical();

                if (selected.Count > 0)
                {
                    EditorGUILayout.LabelField(m_GCPlacementSettingsLabel);
                    GUILayout.Space(pad);
                }

                EditorGUILayout.BeginVertical();

                foreach (var i in selected)
                    DrawSinglePrefabPlacementSettings(prefabs.GetArrayElementAtIndex(i), i);

                /// Little Hack to get the Rect for dropping new prefabs
                Rect endDropDownZone = EditorGUILayout.BeginVertical();
                dropDownZone.height = endDropDownZone.y - dropDownZone.y;
                EditorGUILayout.EndVertical();
            }
            else
            {
                dropDownZone.height = thumbSize;
                var r = EditorGUILayout.BeginVertical(GUILayout.Height(thumbSize+4));
                    EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin ? PolyGUI.k_BoxBackgroundDark : PolyGUI.k_BoxBackgroundLight);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Drag Prefabs Here!", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            Event e = Event.current;

            if (dropDownZone.Contains(e.mousePosition) &&
                (e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && DragAndDrop.objectReferences.Length > 0)
            {
                if (PolyEditorUtility.ContainsPrefabAssets(DragAndDrop.objectReferences))
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                else
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    IEnumerable<GameObject> dragAndDropReferences = DragAndDrop.objectReferences
                        .Where(x => x is GameObject && PolyEditorUtility.IsPrefabAsset(x)).Cast<GameObject>();

                    foreach (GameObject go in dragAndDropReferences)
                    {
                        prefabs.InsertArrayElementAtIndex(prefabs.arraySize);
                        SerializedProperty last = prefabs.GetArrayElementAtIndex(prefabs.arraySize - 1);
                        SerializedProperty gameObject = last.FindPropertyRelative("gameObject");
                        gameObject.objectReferenceValue = go;
                        PlacementSettings.PopulateSerializedProperty(last.FindPropertyRelative("settings"));
                        last.FindPropertyRelative("name").stringValue = go.name;
                    }
                }
            }

            if (e.type == EventType.KeyUp)
            {
                if (IsDeleteKey(e) && !GUI.GetNameOfFocusedControl().Contains("cancelbackspace"))
                {
                    PrefabPalette t = target as PrefabPalette;
                    t.RemoveRange(selected.ToArray());

                    selected.Clear();

                    if (onSelectionChanged != null)
                        onSelectionChanged(null);

                    PolyEditor.DoRepaint();
                }
            }

            serializedObject.ApplyModifiedProperties();
            redrawCounter += 1;
        }

        private bool shouldopencontextmenu = false;
        private int rightClickTime = 0;
        private int redrawCounter = 0;
        private int idx = -1;

        /// <summary>
        /// Draws previews for a prefab in the palette.
        /// </summary>
        /// <param name="prefab">Prefab being previewed</param>
        /// <param name="index">index of the prefab in `prefabs`</param>
        /// <param name="thumbSize">Size of the preview texture</param>
        private void DrawPrefabPreview(SerializedProperty prefab, int index, int thumbSize, float x, float y)
        {
            Rect r = new Rect(x, y, thumbSize, thumbSize);
            Rect rightClickZone = new Rect(r);

            // Texture Preview
            UnityEngine.Object o = prefab.FindPropertyRelative("gameObject").objectReferenceValue;

            Texture2D preview = PreviewsDatabase.GetAssetPreview(o);
            if (selected.Contains(index))
            {
                Rect r2 = new Rect(r);
                r2.x -= 1;
                r2.y -= 1;
                r2.width += 2;
                r2.height += 2;
                EditorGUI.DrawRect(r2, Color.blue);
            }

            EditorGUI.DrawPreviewTexture(r, preview);

            // Those numbers were obtained by empirical experimentation
            r.x += thumbSize - 17;
            r.y += thumbSize - 17;
            r.width = 17;
            r.height = 17;
            LoadoutInfo li = new LoadoutInfo(target as PrefabPalette, index);
            bool isLoaded = loadoutEditor.ContainsPrefab(li);
            Event e = Event.current;
            bool rightClick = (e.type == EventType.MouseDown || e.type == EventType.ContextClick) && rightClickZone.Contains(e.mousePosition) && e.button == 1;
            bool b1 = GUI.Toggle(r, isLoaded, "");
            // Reducing the width by 1 to ensure the button is not larger than the thumbnail.
            // Otherwise button is slightly too large and horizontal scrollbar may appear.
            bool b2 = GUILayout.Button("", GUIStyle.none, GUILayout.Width(thumbSize-1), GUILayout.Height(thumbSize));
            // Set the focus to nothing in case the user want to press delete or backspace key
            // I dont know why but If we don't do that the Textfield with the name of prefab settings never looses focus
            if (b2 || rightClick)
            {
                GUI.FocusControl(null);
                e.Use();
            }
            if (rightClick)
            {
                rightClickTime = redrawCounter;
                shouldopencontextmenu = true;
                idx = index;
                if (!selected.Contains(index))
                {
                    selected.Clear();
                    selected.Add(index);
                }
                return;
            }
            else if (shouldopencontextmenu && redrawCounter > rightClickTime)
            {
                loadoutEditor.OpenCopyPasteMenu(new LoadoutInfo(target as PrefabPalette, idx), selected);
                shouldopencontextmenu = false;
                idx = -1;
                // reset  the redraw counter to avoid overflow
                redrawCounter = 0;
            }

            if (b1 && !isLoaded)
            {
                loadoutEditor.AddPrefabInLoadout(li);
                //loadoutEditor.loadouts.Add(li);
            }
            else if (!b1 && isLoaded)
            {
                loadoutEditor.RemovePrefabFromLoadout(li);
                //loadoutEditor.loadouts.Remove(li);
            }
            else if (b2)
            {
                if (Event.current.shift || Event.current.control)
                {
                    if (!selected.Add(index))
                        selected.Remove(index);
                }
                else
                {
                    if (selected.Count == 1 && selected.Contains(index))
                        selected.Remove(index);
                    else
                    {
                        selected.Clear();
                        selected.Add(index);
                    }
                }

                if (onSelectionChanged != null)
                    onSelectionChanged(selected);

                GUI.changed = true;
            }
        }

        /// <summary>
        /// Show the specific placement settings for a prefab in the palette
        /// </summary>
        /// <param name="prefab">The prefab whose settings we're showing</param>
        /// <param name="index">The index of the prefab in `prefabs`</param>
        private void DrawSinglePrefabPlacementSettings(SerializedProperty prefab, int index)
        {
            SerializedProperty go = prefab.FindPropertyRelative("gameObject");
            SerializedProperty settings = prefab.FindPropertyRelative("settings");
            SerializedProperty name = prefab.FindPropertyRelative("name");

            SerializedProperty uniformBool = settings.FindPropertyRelative("m_UniformBool");
            SerializedProperty strength = settings.FindPropertyRelative("m_Strength");
            SerializedProperty minRot = settings.FindPropertyRelative("m_RotationRangeMin");
            SerializedProperty maxRot = settings.FindPropertyRelative("m_RotationRangeMax");
            SerializedProperty minScale = settings.FindPropertyRelative("m_ScaleRangeMin");
            SerializedProperty maxScale = settings.FindPropertyRelative("m_ScaleRangeMax");
            SerializedProperty xScaleBool = settings.FindPropertyRelative("m_XScaleBool");
            SerializedProperty yScaleBool = settings.FindPropertyRelative("m_YScaleBool");
            SerializedProperty zScaleBool = settings.FindPropertyRelative("m_ZScaleBool");
            SerializedProperty xRotationBool = settings.FindPropertyRelative("m_XRotationBool");
            SerializedProperty yRotationBool = settings.FindPropertyRelative("m_YRotationBool");
            SerializedProperty zRotationBool = settings.FindPropertyRelative("m_ZRotationBool");

            const int pad = 4;

            var settingsBackgroundStyle = new GUIStyle();
            settingsBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;
            settingsBackgroundStyle.padding = new RectOffset(0, 0, 0, 0);
            PolyGUI.PushBackgroundColor(EditorGUIUtility.isProSkin ? PolyGUI.k_BoxBackgroundDark : PolyGUI.k_BoxBackgroundLight);

            EditorGUILayout.BeginVertical(settingsBackgroundStyle);

            GUILayout.Space(pad);

            PolyGUI.PopBackgroundColor();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(pad);

                /// Name field And Strenght Slider
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    GUI.SetNextControlName("cancelbackspace1" + index);
                    name.stringValue = GUILayout.TextField(name.stringValue, GUILayout.ExpandWidth(true));
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUI.SetNextControlName("cancelbackspace2" + index);
                        strength.floatValue = EditorGUILayout.Slider("Frequency (%)", strength.floatValue,
                            BrushModePrefab.k_PrefabOccurrenceMin, BrushModePrefab.k_PrefabOccurrenceMax, GUILayout.ExpandWidth(true));
                    }
                }
                GUILayout.Space(pad);
            }

            GUILayout.BeginVertical();

            float floatfieldWidth = EditorGUIUtility.currentViewWidth / 10;
            GUILayoutOption[] floatFieldsOptions = new GUILayoutOption[] { GUILayout.Width(floatfieldWidth), GUILayout.MinWidth(40) };
            GUILayoutOption widthConstraint = GUILayout.Width(15);

            int slidersMargin = 15;

            EditorGUILayout.LabelField("Randomize Scale");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(slidersMargin);
            EditorGUILayout.LabelField("Uniform Scale", GUILayout.Width(90));
            uniformBool.boolValue = EditorGUILayout.Toggle(uniformBool.boolValue, widthConstraint);
            GUILayout.FlexibleSpace();
            if (uniformBool.boolValue)
            {
                SerializedProperty uScale = settings.FindPropertyRelative("m_UniformScale");
                Vector2 uniformScale = uScale.vector2Value;
                GUILayout.Space(slidersMargin);
                GUI.SetNextControlName("cancelbackspace3" + index);
                uniformScale.x = EditorGUILayout.FloatField(uniformScale.x, floatFieldsOptions);
                EditorGUILayout.MinMaxSlider(ref uniformScale.x, ref uniformScale.y, 0.0f, 10f);
                GUI.SetNextControlName("cancelbackspace4" + index);
                uniformScale.y = EditorGUILayout.FloatField(uniformScale.y, floatFieldsOptions);
                GUILayout.FlexibleSpace();
                uScale.vector2Value = uniformScale;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndHorizontal();
                //Random Scale
                Vector3 scaleRangeMin = minScale.vector3Value;
                Vector3 scaleRangeMax = maxScale.vector3Value;

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(slidersMargin);
                    xScaleBool.boolValue = EditorGUILayout.Toggle(xScaleBool.boolValue, widthConstraint);
                    EditorGUILayout.LabelField("X", widthConstraint);
                    GUI.SetNextControlName("cancelbackspace5" + index);
                    scaleRangeMin.x = EditorGUILayout.FloatField(scaleRangeMin.x, floatFieldsOptions);
                    EditorGUILayout.MinMaxSlider(ref scaleRangeMin.x, ref scaleRangeMax.x, 0.0f, 10f);
                    GUI.SetNextControlName("cancelbackspace6" + index);
                    scaleRangeMax.x = EditorGUILayout.FloatField(scaleRangeMax.x, floatFieldsOptions);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(slidersMargin);
                    yScaleBool.boolValue = EditorGUILayout.Toggle(yScaleBool.boolValue, widthConstraint);
                    EditorGUILayout.LabelField("Y", widthConstraint);
                    GUI.SetNextControlName("cancelbackspace7" + index);
                    scaleRangeMin.y = EditorGUILayout.FloatField(scaleRangeMin.y, floatFieldsOptions);
                    EditorGUILayout.MinMaxSlider(ref scaleRangeMin.y, ref scaleRangeMax.y, 0.0f, 10f);
                    GUI.SetNextControlName("cancelbackspace8" + index);
                    scaleRangeMax.y = EditorGUILayout.FloatField(scaleRangeMax.y, floatFieldsOptions);
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(slidersMargin);
                    zScaleBool.boolValue = EditorGUILayout.Toggle(zScaleBool.boolValue, widthConstraint);
                    EditorGUILayout.LabelField("Z", widthConstraint);
                    GUI.SetNextControlName("cancelbackspace9" + index);
                    scaleRangeMin.z = EditorGUILayout.FloatField(scaleRangeMin.z, floatFieldsOptions);
                    EditorGUILayout.MinMaxSlider(ref scaleRangeMin.z, ref scaleRangeMax.z, 0.0f, 10f);
                    GUI.SetNextControlName("cancelbackspace10" + index);
                    scaleRangeMax.z = EditorGUILayout.FloatField(scaleRangeMax.z, floatFieldsOptions);
                }
                minScale.vector3Value = scaleRangeMin;
                maxScale.vector3Value = scaleRangeMax;
            }

            // Random Rotation
            Vector3 rotationRangeMin = minRot.vector3Value;
            Vector3 rotationRangeMax = maxRot.vector3Value;
            EditorGUILayout.LabelField("Randomize Rotation");

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(slidersMargin);
                xRotationBool.boolValue = EditorGUILayout.Toggle(xRotationBool.boolValue, widthConstraint);
                EditorGUILayout.LabelField("X", widthConstraint);
                GUI.SetNextControlName("cancelbackspace11" + index);
                rotationRangeMin.x = EditorGUILayout.FloatField(rotationRangeMin.x, floatFieldsOptions);
                EditorGUILayout.MinMaxSlider(ref rotationRangeMin.x, ref rotationRangeMax.x, 0.0f, 360f);
                GUI.SetNextControlName("cancelbackspace12" + index);
                rotationRangeMax.x = EditorGUILayout.FloatField(rotationRangeMax.x, floatFieldsOptions);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(slidersMargin);
                yRotationBool.boolValue = EditorGUILayout.Toggle(yRotationBool.boolValue, widthConstraint);
                EditorGUILayout.LabelField("Y", widthConstraint);
                GUI.SetNextControlName("cancelbackspace13" + index);
                rotationRangeMin.y = EditorGUILayout.FloatField(rotationRangeMin.y, floatFieldsOptions);
                EditorGUILayout.MinMaxSlider(ref rotationRangeMin.y, ref rotationRangeMax.y, 0.0f, 360f);
                GUI.SetNextControlName("cancelbackspace14" + index);
                rotationRangeMax.y = EditorGUILayout.FloatField(rotationRangeMax.y, floatFieldsOptions);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(slidersMargin);
                zRotationBool.boolValue = EditorGUILayout.Toggle(zRotationBool.boolValue, widthConstraint);
                EditorGUILayout.LabelField("Z", widthConstraint);
                GUI.SetNextControlName("cancelbackspace15" + index);
                rotationRangeMin.z = EditorGUILayout.FloatField(rotationRangeMin.z, floatFieldsOptions);
                EditorGUILayout.MinMaxSlider(ref rotationRangeMin.z, ref rotationRangeMax.z, 0.0f, 360f);
                GUI.SetNextControlName("cancelbackspace16" + index);
                rotationRangeMax.z = EditorGUILayout.FloatField(rotationRangeMax.z, floatFieldsOptions);
            }
            minRot.vector3Value = rotationRangeMin;
            maxRot.vector3Value = rotationRangeMax;
            GUILayout.EndVertical();

            GUILayout.Space(pad);
            GUILayout.EndVertical();
            GUILayout.Space(pad);
		}
	}
}
