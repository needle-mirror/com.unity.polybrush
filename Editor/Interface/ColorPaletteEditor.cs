using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Custom Editor for Color Palettes
    /// </summary>
	[CustomEditor(typeof(ColorPalette))]
	internal class ColorPaletteEditor : Editor
	{
        /// <summary>
        /// Utility class to handle dragging colors in the Color Palette
        /// </summary>
		internal class DragState
		{
			internal enum Status
			{
				Ready,
				Dragging,
				DragInvalid
			}

			Status m_Status = Status.Ready;
		    Status m_QueuedStatus = Status.Ready;

			int m_SourceIndex;
			int m_DestinationIndex;
            int m_QueuedDestinationIndex;
            int m_QueuedSourceIndex;

            internal SerializedProperty swatch;
            internal Vector2 offset;

			internal Status status {
				get {
					return m_Status;
				}

				set {
					m_QueuedStatus = value;
					m_WantsUpdate = true;
				}
			}

			internal int sourceIndex {
				get {
					return m_SourceIndex;
				}

				set {
					m_QueuedSourceIndex = value;
					m_WantsUpdate = true;
				}
			}

			internal int destinationIndex {
				get {
					return m_DestinationIndex;
				}

				set {
					m_QueuedDestinationIndex = value;
					m_WantsUpdate = true;
				}
			}

			bool m_WantsUpdate = false;
		    bool m_IsBetweenRepaint = true;

			internal void Init(int index, SerializedProperty swatch, Vector2 mouseOffset)
			{
				this.sourceIndex = index;
				this.destinationIndex = index;
				this.swatch = swatch;
				this.status = Status.Dragging;
				this.offset = mouseOffset;
				this.m_IsBetweenRepaint = true;
				this.m_WantsUpdate = true;
			}

			internal void Reset()
			{
				this.status = DragState.Status.Ready;
				this.swatch = null;
				this.sourceIndex = -1;
				this.destinationIndex = -1;
			}

			internal void Update(Event e)
			{
				if(e.type == EventType.Layout)
					m_IsBetweenRepaint = true;
				else if(e.type == EventType.Repaint)
					m_IsBetweenRepaint = false;
                else if(e.type == EventType.Ignore && status == Status.Dragging)
                    Reset();

				if(!m_WantsUpdate || m_IsBetweenRepaint)
					return;

				m_WantsUpdate = false;

				m_Status = m_QueuedStatus;
				m_SourceIndex = m_QueuedSourceIndex;
				m_DestinationIndex = m_QueuedDestinationIndex;

				PolybrushEditor.DoRepaint();
			}

			public override string ToString()
			{
				return string.Format("{0}: {1} -> {2}", status, sourceIndex, destinationIndex);
			}
		}

        static GUIStyle s_StyleForColorSwatch;

        SerializedProperty m_CurrentProperty;
		SerializedProperty m_ColorsProperty;

		internal Action<Color> onSelectIndex = null;
		internal Action<ColorPalette> onSaveAs = null;

		DragState m_Drag = new DragState();
		const int k_DragOverNull = -1;
		const int k_DragOverTrash = -42;

        GUIContent m_GCRemoveSwatch;
        GUIContent m_GCAddColorSwatch;

		private void OnEnable()
		{
			m_CurrentProperty = serializedObject.FindProperty("current");
			m_ColorsProperty = serializedObject.FindProperty("colors");

			m_GCAddColorSwatch = new GUIContent(IconUtility.GetIcon("PaintVertexColors/AddColor"), "Add Selected Color to Palette");
            m_GCRemoveSwatch = new GUIContent(IconUtility.GetIcon("PaintVertexColors/Trashcan"));
        }

		private void SetCurrent(Color color)
		{
			if(onSelectIndex != null)
				onSelectIndex(color);

			m_CurrentProperty.colorValue = color;
		}

		int IncrementIndex(int index, int rowSize)
		{
			index++;

			if(index % rowSize == 0)
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}

			return index;
		}

        void GenerateStaticGUIStyle()
        {
            s_StyleForColorSwatch = new GUIStyle(GUI.skin.FindStyle("IconButton"));
            s_StyleForColorSwatch.border = new RectOffset(0, 0, 0, 0);
            s_StyleForColorSwatch.padding = new RectOffset(0, 0, 0, 0);
            s_StyleForColorSwatch.margin = new RectOffset(-1, -1, 0, 0);
            s_StyleForColorSwatch.richText = false;
            s_StyleForColorSwatch.imagePosition = ImagePosition.ImageOnly;
            s_StyleForColorSwatch.fixedHeight = s_StyleForColorSwatch.fixedWidth = 0;
            s_StyleForColorSwatch.stretchHeight = s_StyleForColorSwatch.stretchWidth = true;
            s_StyleForColorSwatch.alignment = TextAnchor.MiddleCenter;
            s_StyleForColorSwatch.normal.background = IconUtility.GetIcon("PaintVertexColors/ColorSwatch");
        }

		public override void OnInspectorGUI()
		{
            if (s_StyleForColorSwatch == null)
                GenerateStaticGUIStyle();

			Event e = Event.current;

			serializedObject.Update();

			Color current = m_CurrentProperty.colorValue;

			EditorGUI.BeginChangeCheck();
			current = EditorGUILayout.ColorField(current);
			if(EditorGUI.EndChangeCheck())
				SetCurrent(current);

            int swatchSize = 12;
			int viewWidth = (int) EditorGUIUtility.currentViewWidth - 12;
			int swatchesPerRow = viewWidth / swatchSize;
			swatchSize += (viewWidth % swatchSize) / swatchesPerRow;

            int mouseOverIndex = k_DragOverNull;
            int index = 0;
            int arraySize = m_ColorsProperty.arraySize;
            int arraySizeWithAdd = m_ColorsProperty.arraySize + 1;

            using (new GUILayout.HorizontalScope())
            {
                for (int i = 0; i < arraySizeWithAdd; i++)
                {
                    bool isColorSwatch = i < arraySize;
                    bool isActiveDrag = m_Drag.status == DragState.Status.Dragging && m_Drag.destinationIndex == i && i != arraySizeWithAdd - 1;
                    SerializedProperty swatch = isColorSwatch ? m_ColorsProperty.GetArrayElementAtIndex(i) : null;
                    Rect swatchRect = new Rect(-1f, -1f, 0f, 0f);

                    if (isActiveDrag)
                    {
                        GUILayout.Space(swatchSize + 4);
                        index = IncrementIndex(index, swatchesPerRow);
                    }

                    if (isColorSwatch)
                    {
                        GUI.backgroundColor = swatch.colorValue;

                        if (m_Drag.status != DragState.Status.Dragging || i != m_Drag.sourceIndex)
                        {
                            GUILayout.Label("", s_StyleForColorSwatch,
                                    GUILayout.MinWidth(swatchSize),
                                    GUILayout.MaxWidth(swatchSize),
                                    GUILayout.MinHeight(swatchSize),
                                    GUILayout.MaxHeight(swatchSize)); ;

                            swatchRect = GUILayoutUtility.GetLastRect();
                            index = IncrementIndex(index, swatchesPerRow);
                        }
                    }
                    else
                    {
                        if (m_Drag.status != DragState.Status.Dragging)
                        {
                            GUI.backgroundColor = current;

                            EditorGUIUtility.SetIconSize(new Vector2(10, 11));

                            if (GUILayout.Button(m_GCAddColorSwatch, s_StyleForColorSwatch,
                                    GUILayout.MinWidth(swatchSize),
                                    GUILayout.MaxWidth(swatchSize),
                                    GUILayout.Height(swatchSize),
                                    GUILayout.MaxHeight(swatchSize)))
                            {
                                m_ColorsProperty.arraySize++;
                                SerializedProperty added = m_ColorsProperty.GetArrayElementAtIndex(m_ColorsProperty.arraySize - 1);
                                added.colorValue = current;
                            }
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(m_GCRemoveSwatch);
                        }

                        swatchRect = GUILayoutUtility.GetLastRect();
                        index = IncrementIndex(index, swatchesPerRow);
                    }

                    GUI.backgroundColor = Color.white;

                    if (swatchRect.Contains(e.mousePosition))
                    {
                        if (m_Drag.status == DragState.Status.Dragging)
                            mouseOverIndex = i >= m_Drag.destinationIndex ? i + 1 : i;
                        else
                            mouseOverIndex = i;

                        if (i == arraySize)
                            mouseOverIndex = k_DragOverTrash;

                        if (e.type == EventType.MouseDrag)
                        {
                            if (m_Drag.status == DragState.Status.Ready && isColorSwatch)
                            {
                                e.Use();
                                m_Drag.Init(mouseOverIndex, m_ColorsProperty.GetArrayElementAtIndex(mouseOverIndex), swatchRect.position - e.mousePosition);
                            }
                            else if (m_Drag.status == DragState.Status.Dragging)
                            {
                                m_Drag.destinationIndex = mouseOverIndex;
                            }
                        }
                        else if (e.type == EventType.MouseUp && m_Drag.status != DragState.Status.Dragging && isColorSwatch)
                        {
                            if (onSelectIndex != null)
                            {
                                SetCurrent(swatch.colorValue);
                            }
                        }
                    }
                }

            }

			// If drag was previously over the trash bin but has moved, reset the index to be over the last array entry
			// instead.
			if( e.type == EventType.MouseDrag &&
				m_Drag.status == DragState.Status.Dragging &&
				mouseOverIndex == k_DragOverNull &&
				m_Drag.destinationIndex == k_DragOverTrash)
			{
				m_Drag.destinationIndex = arraySize;
			}

			bool dragIsOverTrash = m_Drag.destinationIndex == k_DragOverTrash;

			if(m_Drag.status == DragState.Status.Dragging && m_Drag.swatch != null)
			{
				Rect r = new Rect(e.mousePosition.x + m_Drag.offset.x, e.mousePosition.y + m_Drag.offset.y, swatchSize, swatchSize);
				GUI.backgroundColor = m_Drag.swatch.colorValue;
                GUI.Label(r, "", s_StyleForColorSwatch);
				GUI.backgroundColor = Color.white;

				PolybrushEditor.DoRepaint();
				Repaint();
			}

			switch( e.type )
			{
				case EventType.MouseUp:
				{
					if(m_Drag.status == DragState.Status.Dragging)
					{
						if(m_Drag.destinationIndex != k_DragOverNull)
						{
							if(dragIsOverTrash)
								m_ColorsProperty.DeleteArrayElementAtIndex(m_Drag.sourceIndex);
							else
								m_ColorsProperty.MoveArrayElement(m_Drag.sourceIndex, m_Drag.destinationIndex > m_Drag.sourceIndex ? m_Drag.destinationIndex - 1 : m_Drag.destinationIndex);
						}
					}

					m_Drag.Reset();

					PolybrushEditor.DoRepaint();
					Repaint();
				}
				break;
			}
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

			serializedObject.ApplyModifiedProperties();
			m_Drag.Update(e);
		}

        /// <summary>
        /// Create a New Color Palette assets
        /// </summary>
        /// <returns>The newly created Color Palette</returns>
        internal static ColorPalette AddNew()
        {
            string path = PolyEditorUtility.UserAssetDirectory + "Color Palette";

            if (string.IsNullOrEmpty(path))
                path = "Assets";

            path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Color Palette.asset");

            if (!string.IsNullOrEmpty(path))
            {
                ColorPalette palette = ScriptableObject.CreateInstance<ColorPalette>();
                palette.SetDefaultValues();

                AssetDatabase.CreateAsset(palette, path);
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(palette);

                return palette;
            }

            return null;
        }

        void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, serializedObject.targetObject.name);
		}

	    void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty col = m_ColorsProperty.GetArrayElementAtIndex(index);
			Rect r = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 5);
			EditorGUI.PropertyField(r, col);
		}

		void OnAddItem(ReorderableList list)
		{
			ReorderableList.defaultBehaviours.DoAddButton(list);

			SerializedProperty col = m_ColorsProperty.GetArrayElementAtIndex(list.index);
			col.colorValue = Color.white;
		}
	}
}
