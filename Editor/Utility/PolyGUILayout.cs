using UnityEngine;
using System.Reflection;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// GUI field extensions.
    /// </summary>
    internal static class PolyGUILayout
	{
        static GUIStyle m_HelpStyle;

        static GUIStyle HelpStyle
        {
            get
            {
                if (m_HelpStyle == null)
                {
					m_HelpStyle = GUI.skin.FindStyle("IconButton");
                }
                return m_HelpStyle;
            }
        }

        /// <summary>
        /// Color field control
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        internal static ColorMask ColorMaskField(string text, ColorMask mask)
		{
			return ColorMaskField(string.IsNullOrEmpty(text) ? null : PolyGUI.TempContent(text), mask);
		}

		internal static ColorMask ColorMaskField(GUIContent gc, ColorMask mask)
		{
            using (new GUILayout.HorizontalScope())
            {
                if (gc != null)
                {
                    EditorGUILayout.LabelField(gc, GUILayout.Width(EditorGUIUtility.currentViewWidth - 160));
                }

                bool newValue;
                newValue = GUILayout.Toggle(mask.r, "R", "Button");
                if (newValue != mask.r)
                    mask.r = !mask.r;

                newValue = GUILayout.Toggle(mask.g, "G", "Button");
                if (newValue != mask.g)
                    mask.g = !mask.g;

                newValue = GUILayout.Toggle(mask.b, "B", "Button");
                if (newValue != mask.b)
                    mask.b = !mask.b;

                newValue = GUILayout.Toggle(mask.a, "A", "Button");
                if (newValue != mask.a)
                    mask.a = !mask.a;
            }
            EditorGUILayout.Space();
			return mask;
		}

		internal static uint BitMaskField(uint value, string[] descriptions, string tooltip)
		{
            GUIContent gc =  PolyGUI.TempContent("", tooltip);

            using (new GUILayout.HorizontalScope())
            {
                int l = descriptions.Length;

                for (int i = 1; i < l; i++)
                {
                    int s = i - 1;

                    bool toggled = (value & (1u << s)) > 0;

                    gc.text = descriptions[i];

                    bool newValue = GUILayout.Toggle(toggled, gc, "Button", GUILayout.Width(30));

                    if (toggled != newValue)
                        value ^= (1u << s);
                }
            }

			return value;
		}

		internal static int CycleButton(int index, GUIContent[] content, GUIStyle style = null)
		{
			if(style != null)
			{
				if( GUILayout.Button(content[index], style) )
					return (index + 1) % content.Length;
				else
					return index;
			}
			else
			{
				if( GUILayout.Button(content[index]) )
					return (index + 1) % content.Length;
				else
					return index;
			}
		}

        /// <summary>
        /// Similar to EditorGUILayoutUtility.Slider, except this allows for values outside of the min/max bounds via the float field.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static float FreeSlider(string content, float value, float min, float max)
		{
			return FreeSlider(PolyGUI.TempContent(content), value, min, max);
		}

        /// <summary>
        /// Similar to EditorGUILayoutUtility.Slider, except this allows for values outside of the min/max bounds via the float field.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static float FreeSlider(GUIContent content, float value, float min, float max)
		{
			const float PAD = 4f;
			const float SLIDER_HEIGHT = 16f;
			const float MIN_LABEL_WIDTH = 0f;
			const float MAX_LABEL_WIDTH = 128f;
			const float MIN_FIELD_WIDTH = 48f;

			GUILayoutUtility.GetRect(Screen.width, 18);

			Rect previousRect = GUILayoutUtility.GetLastRect();
			float y = previousRect.y;

			float labelWidth = content != null ? Mathf.Max(MIN_LABEL_WIDTH, Mathf.Min(GUI.skin.label.CalcSize(content).x + PAD, MAX_LABEL_WIDTH)) : 0f;
			float remaining = (Screen.width - (PAD * 2f)) - labelWidth;
			float sliderWidth = remaining - (MIN_FIELD_WIDTH + PAD);
			float floatWidth = MIN_FIELD_WIDTH;

			Rect labelRect = new Rect(PAD, y + 2f, labelWidth, SLIDER_HEIGHT);
			Rect sliderRect = new Rect(labelRect.x + labelWidth, y + 1f, sliderWidth, SLIDER_HEIGHT);
			Rect floatRect = new Rect(sliderRect.x + sliderRect.width + PAD, y + 1f, floatWidth, SLIDER_HEIGHT);

			if(content != null)
				GUI.Label(labelRect, content);

			EditorGUI.BeginChangeCheck();

				int controlID = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
				float tmp = value;
				tmp = GUI.Slider(sliderRect, tmp, 0f, min, max, GUI.skin.horizontalSlider, (!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, controlID);

			if(EditorGUI.EndChangeCheck())
				value = Event.current.control ? 1f * Mathf.Round(tmp / 1f) : tmp;

			value = EditorGUI.FloatField(floatRect, value);

			return value;
		}

		internal static int ChannelField(int index, AttributeLayout[] channels, int thumbSize, int yPos)
		{
			int mIndex = index;
			int attribsLength = channels != null ? channels.Length : 0;

			const int margin = 4; 					// group pad
			const int pad = 2; 						// texture pad
			const int selected_rect_height = 10;	// the little green bar and height padding

			int actual_width = thumbSize + pad;
			int container_width = (int) Mathf.Floor(EditorGUIUtility.currentViewWidth) - ((margin + pad) * 2);
            // The container_width is currently as wide as the current view - margins and padding.
            // Adjust it to take the size of the vertical scrollbar.
            container_width -= (int)GUI.skin.verticalScrollbar.fixedWidth;
            int columns = (int) Mathf.Floor(container_width / actual_width);
			int rows = attribsLength / columns + (attribsLength % columns == 0 ? 0 : 1);
			int height = rows * (actual_width + selected_rect_height);// + margin * 2;

			Rect r = new Rect(margin + pad, yPos, actual_width, actual_width);
            Rect border = new Rect( margin + pad , yPos, container_width, height);
            
            // Draw background
            EditorGUI.DrawRect(border, EditorGUIUtility.isProSkin ? PolyGUI.k_BoxOutlineDark : PolyGUI.k_BoxOutlineLight);
            
            for (int i = 0; i < attribsLength; i++)
			{
				if(i > 0 && i % columns == 0)
				{
					r.x = pad + margin;
					r.y += r.height + selected_rect_height;
				}

				string summary = channels[i].propertyTarget;

				if(string.IsNullOrEmpty(summary))
					summary = "channel\n" + (i+1);

				if( AttributeComponentButton(r, summary, channels[i].previewTexture, i == mIndex) )
				{
					mIndex = i;
					GUI.changed = true;
				}

				r.x += r.width;
			}

			GUILayoutUtility.GetRect(container_width - 8, height);

			return mIndex;
		}

		static readonly Color texture_button_border = new Color(.1f, .1f, .1f, 1f);
		static readonly Color texture_button_fill = new Color(.18f, .18f, .18f, 1f);

		static bool AttributeComponentButton(Rect rect, string text, Texture2D img, bool selected)
		{
			bool clicked = false;

			Rect r = rect;

			Rect border = new Rect(r.x + 2, r.y + 6, r.width - 4, r.height - 4);

		    EditorGUI.DrawRect(border, texture_button_border);

			border.x += 2;
			border.y += 2;
			border.width -= 4;
			border.height -= 4;

			if(img != null)
			{
				EditorGUI.DrawPreviewTexture(border, img, null, ScaleMode.ScaleToFit, 0f);
			}
			else
			{
			    EditorGUI.DrawRect(border, texture_button_fill);
				GUI.Label(border, text, PolyGUI.CenteredStyle);
			}

			if(selected)
			{
				r.y += r.height + 4;
				r.x += 2;
				r.width -= 5;
				r.height = 6;
			    EditorGUI.DrawRect(r, Color.green);
			}

			clicked = GUI.Button(border, "", GUIStyle.none);

			return clicked;
		}

		internal static bool AssetPreviewButton(Rect rect, Object obj, bool selected)
		{
			bool clicked = false;
			Rect r = rect;
			Rect border = new Rect(r.x + 2, r.y + 6, r.width - 4, r.height - 4);

		    EditorGUI.DrawRect(border, texture_button_border);

			border.x += 2;
			border.y += 2;
			border.width -= 4;
			border.height -= 4;

			Texture2D preview = PreviewsDatabase.GetAssetPreview(obj);

			if(preview != null)
			{
				EditorGUI.DrawPreviewTexture(border, preview, null, ScaleMode.ScaleToFit, 0f);
			}
			else
			{
				string text = obj != null ? obj.name : "null";
			    EditorGUI.DrawRect(border, texture_button_fill);
				GUI.Label(border, text, PolyGUI.CenteredStyle);
			}

			if(selected)
			{
				r.y += r.height + 4;
				r.x += 2;
				r.width -= 5;
				r.height = 6;

			    EditorGUI.DrawRect(r, Color.green);
			}

			clicked = GUI.Button(border, "", GUIStyle.none);

			return clicked;
		}

		internal static bool Foldout(bool state, GUIContent content)
		{
            return EditorGUILayout.Foldout(state, content, true);
		}

        public static bool Foldout(bool state, GUIContent content, GUILayoutOption[] options)
        {
            if (GUILayout.Button(PolyGUI.TempContent(""), state ? "FoldoutOpen" : "FoldoutClosed", options))
                state = !state;

            GUILayout.Label(content, options);
            return state;
        }

		internal static bool Toggle(GUIContent gc, bool isToggled)
		{
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(gc);
                isToggled = EditorGUILayout.Toggle(isToggled, GUILayout.Width(15*(EditorGUI.indentLevel+1)));
            }

            return isToggled;
		}

        internal static int PopupFieldWithTitle(GUIContent title, int selectionIndex, string[] values)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(title, GUILayout.Width(140));
                int selection = EditorGUILayout.Popup(selectionIndex, values);
                return selection;
            }
        }

		internal static float FloatField(GUIContent gc, float value, params GUILayoutOption[] options)
		{
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(gc, GUILayout.Width(140));
                float ret = EditorGUILayout.FloatField(value, "textfield");

                return ret;
            }
		}

        internal static float FloatFieldWithSlider(GUIContent gc, float value, float minValue, float maxValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                float returnValue = value;

                EditorGUILayout.LabelField(gc, GUILayout.Width(100));
                returnValue = EditorGUILayout.Slider(returnValue, minValue, maxValue);
                returnValue = Mathf.Clamp(returnValue, minValue, maxValue);

                return returnValue;
            }
        }

		internal static Color ColorField(GUIContent gc, Color color)
		{
			if(gc != null && !string.IsNullOrEmpty(gc.text))
				GUILayout.Label(gc);

			var ret = EditorGUILayout.ColorField(PolyGUI.TempContent("", gc.tooltip), color);
			return ret;
		}

		internal static Gradient GradientField(GUIContent gc, Gradient value)
		{
			GUILayout.Label(gc);

		    object out_gradient = EditorGUILayout.GradientField(PolyGUI.TempContent("", gc.tooltip), value, null);

			return (Gradient) out_gradient;
		}

		internal static bool HeaderWithDocsLink(GUIContent gc)
		{          
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(gc, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");
                Vector2 iconSize = HelpStyle.CalcSize(helpIcon);
                Rect rect = GUILayoutUtility.GetRect(iconSize.x, iconSize.y);

                return GUI.Button(rect, helpIcon, HelpStyle);
            }
        }
	}
}
