// #define Z_DEBUG

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Base class for brush modes.
    /// </summary>
    [System.Serializable]
	internal abstract class BrushMode : ScriptableObject
	{
        static readonly string k_DefaultBrushColorGradient = "RGBA(0.227, 1.000, 0.227, 255.000)&0.000|RGBA(1.000, 1.000, 1.000, 255.000)&1.000|\n1.0000&0.000|0.2588&1.000|";
        static readonly Color k_DefaultBrushColor = new Color(0f, .8f, 1f, 1f);

        [UserSetting("General Settings", "Hide Wireframe", "Hides the object wireframe when a brush is hovering.")]
        internal static Pref<bool> s_HideWireframe = new Pref<bool>("Brush.HideWireframe", true, SettingsScope.Project);

        [UserSetting("General Settings", "Inner Brush Color", "Vertices inside the brush's Inner Radius will be highlighted with this color")]
        internal static Pref<Color> s_FullStrengthColor = new Pref<Color>("Brush.BrushColor", k_DefaultBrushColor, SettingsScope.Project);

        [UserSetting("General Settings", "Brush Gradient", "Vertices outside the Inner Radius will be highlighted with this gradient, matching the brush's Falloff curve")]
        internal static Pref<Gradient> s_BrushGradientColor = new Pref<Gradient>("Brush.BrushColorGradient", GradientSerializer.Deserialize(k_DefaultBrushColorGradient), SettingsScope.Project);

        // The message that will accompany Undo commands for this brush.  Undo/Redo is handled by PolyEditor.
        internal virtual string UndoMessage { get { return "Apply Brush"; } }

		// A temporary component attached to the currently editing object.  Use this to (by default) override the
		// scene zoom functionality, or optionally extend (see OverlayRenderer).
		[SerializeField] protected ZoomOverride tempComponent;

		// The title to be displayed in the settings header.
		protected abstract string ModeSettingsHeader { get; }

		// The link to the documentation page for this mode.
		protected abstract string DocsLink { get; }

		protected Color innerColor, outerColor;

        /// <summary>
        /// Create the temporary component
        /// </summary>
        /// <param name="target">The object to attach the temporary component on</param>
		protected virtual void CreateTempComponent(EditableObject target)
		{
			if(!Util.IsValid(target))
				return;

            if(tempComponent == null)
                tempComponent = target.gameObjectAttached.AddComponent<ZoomOverride>();

            tempComponent.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
			tempComponent.SetWeights(null, 0f);
		}


        /// <summary>
        /// Update the temporary component with new weigths and strengh
        /// </summary>
        /// <param name="target">Target object containing the temporary component</param>
        /// <param name="settings">Current brush settings</param>
		protected virtual void UpdateTempComponent(BrushTarget target, BrushSettings settings)
		{
			if(!Util.IsValid(target))
				return;

            tempComponent.SetWeights(target.GetAllWeights(), settings.strength);
		}

		protected virtual void DestroyTempComponent()
		{
			if(tempComponent != null)
				GameObject.DestroyImmediate(tempComponent);
		}

        /// <summary>
		/// Called on instantiation.  Base implementation sets HideFlags.
        /// </summary>
        internal virtual void OnEnable()
		{
            this.hideFlags = HideFlags.HideAndDontSave;
		}

        /// <summary>
        /// Called when mode is disabled.
        /// </summary>
        internal virtual void OnDisable()
		{
			DestroyTempComponent();
		}

        /// <summary>
        /// Will be called by the unit test to ensure that all the settings are reset to default values, with basic settings or palettes
        /// </summary>
        /// <returns></returns>
        internal virtual bool SetDefaultSettings()
        {
            return true;
        }

        /// <summary>
        /// Called by PolyEditor when brush settings have been modified.
        /// Updates the temporary component
        /// </summary>
        /// <param name="target">Target object containing the temporary component</param>
        /// <param name="settings">Modified brush settings</param>
        internal virtual void OnBrushSettingsChanged(BrushTarget target, BrushSettings settings)
		{
			UpdateTempComponent(target, settings);
		}

        /// <summary>
        /// Inspector GUI shown in the Editor window.
        /// </summary>
        /// <param name="brushSettings"></param>
        internal virtual void DrawGUI(BrushSettings brushSettings)
		{
			if( PolyGUILayout.HeaderWithDocsLink( PolyGUI.TempContent(ModeSettingsHeader, "")) )
				Application.OpenURL(DocsLink);
		}

        /// <summary>
        /// Called when the mouse begins hovering an editable object.
        /// </summary>
        /// <param name="target">Object being hovered</param>
        /// <param name="settings">Current brush settings</param>
        internal virtual void OnBrushEnter(EditableObject target, BrushSettings settings)
		{
			if(s_HideWireframe.value && target.renderer != null)
			{
				// disable wirefame
				PolyEditorUtility.SetSelectionRenderState(target.renderer, PolyEditorUtility.GetSelectionRenderState() & SelectionRenderState.Outline);
			}

			CreateTempComponent(target);
		}

        /// <summary>
        /// Called whenever the brush is moved. Note that @target may have a null editableObject.
        /// </summary>
        /// <param name="target">Current target of the brush</param>
        /// <param name="settings">Current brush settings</param>
        internal virtual void OnBrushMove(BrushTarget target, BrushSettings settings)
		{
			UpdateTempComponent(target, settings);
		}

        /// <summary>
        /// Called when the mouse exits hovering an editable object.
        /// </summary>
        /// <param name="target">Previously hovered object</param>
        internal virtual void OnBrushExit(EditableObject target)
		{
			if(target.renderer != null)
				PolyEditorUtility.SetSelectionRenderState(target.renderer, PolyEditorUtility.GetSelectionRenderState());

			DestroyTempComponent();
		}

        /// <summary>
        /// Called when the mouse begins a drag across a valid target.
        /// </summary>
        /// <param name="target"> The object the mouse is dragging on</param>
        /// <param name="settings">Current brush settings</param>
        internal virtual void OnBrushBeginApply(BrushTarget target, BrushSettings settings) {}

        /// <summary>
        /// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
        /// </summary>
        /// <param name="target">Object on which to apply the brush</param>
        /// <param name="settings">Current brush settings</param>
        internal abstract void OnBrushApply(BrushTarget target, BrushSettings settings);


        /// <summary>
        /// Called when a brush application has finished.  Use this to clean up temporary resources or apply
		/// deferred actions to a mesh (rebuild UV2, tangents, whatever).
        /// </summary>
        /// <param name="target">The Object the brush was being applied on</param>
        /// <param name="settings">Current brush settings</param>
		internal virtual void OnBrushFinishApply(BrushTarget target, BrushSettings settings)
		{
			DestroyTempComponent();
		}

        /// <summary>
        /// Update gizmos drawing color based on settings.
        /// </summary>
        internal void UpdateBrushGizmosColor()
        {
            innerColor = s_FullStrengthColor;
            outerColor = s_BrushGradientColor.value.Evaluate(1f);

            innerColor.a = .9f;
            outerColor.a = .35f;
        }

        /// <summary>
        /// Draw scene gizmos. Base implementation draws the brush preview.
        /// </summary>
        /// <param name="target">Current target Object</param>
        /// <param name="settings">Current brush settings</param>
        internal virtual void DrawGizmos(BrushTarget target, BrushSettings settings)
		{
            UpdateBrushGizmosColor();
            foreach (PolyRaycastHit hit in target.raycastHits)
                PolyHandles.DrawBrush(hit.position, hit.normal, settings, target.localToWorldMatrix, innerColor, outerColor);

#if Z_DEBUG

#if Z_DRAW_WEIGHTS || DRAW_PER_VERTEX_ATTRIBUTES
			float[] w = target.GetAllWeights();
#endif

#if Z_DRAW_WEIGHTS
			Mesh m = target.mesh;
			Vector3[] v = m.vertices;
			GUIContent content = new GUIContent("","");

			Handles.BeginGUI();
			for(int i = 0; i < v.Length; i++)
			{
				if(w[i] < .0001f)
					continue;

				content.text = w[i].ToString("F2");
				GUI.Label(HandleUtility.WorldPointToSizedRect(target.transform.TransformPoint(v[i]), content, EditorStyles.label), content);
			}
			Handles.EndGUI();
#endif

#if DRAW_PER_VERTEX_ATTRIBUTES

			Mesh m = target.editableObject.editMesh;
			Color32[] colors = m.colors;
			Vector4[] tangents = m.tangents;
			List<Vector4> uv0 = m.uv0;
			List<Vector4> uv1 = m.uv1;
			List<Vector4> uv2 = m.uv2;
			List<Vector4> uv3 = m.uv3;

			int vertexCount = m.vertexCount;

			Vector3[] verts = m.vertices;
			GUIContent gc = new GUIContent("");

			List<List<int>> common = MeshUtility.GetCommonVertices(m);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			Handles.BeginGUI();
			foreach(List<int> l in common)
			{
				if( w[l[0]] < .001 )
					continue;

				Vector3 v = target.transform.TransformPoint(verts[l[0]]);

				if(colors != null) sb.AppendLine("color: " + colors[l[0]].ToString("F2"));
				if(tangents != null) sb.AppendLine("tangent: " + tangents[l[0]].ToString("F2"));
				if(uv0 != null && uv0.Count == vertexCount) sb.AppendLine("uv0: " + uv0[l[0]].ToString("F2"));
				if(uv1 != null && uv1.Count == vertexCount) sb.AppendLine("uv1: " + uv1[l[0]].ToString("F2"));
				if(uv2 != null && uv2.Count == vertexCount) sb.AppendLine("uv2: " + uv2[l[0]].ToString("F2"));
				if(uv3 != null && uv3.Count == vertexCount) sb.AppendLine("uv3: " + uv3[l[0]].ToString("F2"));

				gc.text = sb.ToString();
				sb.Remove(0, sb.Length);	// @todo .NET 4.0
				GUI.Label(HandleUtility.WorldPointToSizedRect(v, gc, EditorStyles.label), gc);
			}
			Handles.EndGUI();
#endif

#endif
		}

		internal abstract void RegisterUndo(BrushTarget brushTarget);

		internal virtual void UndoRedoPerformed(List<GameObject> modified)
		{
			DestroyTempComponent();
		}
	}
}
