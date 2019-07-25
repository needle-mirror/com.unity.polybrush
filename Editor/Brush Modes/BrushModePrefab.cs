using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SettingsManagement;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Prefab painter brush mode.
    /// </summary>
    internal class BrushModePrefab : BrushMode
    {
        const string k_PrefabLoadouts = "Polybrush::Editor.PrefabLoadout";
        internal readonly static float k_PrefabOccurrenceMin = 1;
        internal readonly static float k_PrefabOccurrenceMax = 100;

        // The current prefab palette
        [SerializeField]
        PrefabPalette m_PrefabPalette = null;

        /// <summary>
        /// Set true to assign the object position based on its pivot.
        /// Set false to use the bottom facing face of the mesh bound to position the object.
        /// </summary>
        [UserSetting]
        static Pref<bool> s_UsePivotForPlacement = new Pref<bool>("Scattering.UsePivotForPlacement", false, SettingsScope.Project);
        /// <summary>
        /// Set to true to parent the object with the painted surface.
        /// Otherwise, objects will be created at the root of the scene.
        /// </summary>
        [UserSetting]
        static Pref<bool> s_ParentObjectWithSurface = new Pref<bool>("Scattering.ParentObjectsWithSurface", true, SettingsScope.Project);
        /// <summary>
        /// Set to true to avoid overlapping between objects.
        /// </summary>
        [UserSetting]
        static Pref<bool> s_AvoidOverlappingGameObjects = new Pref<bool>("Scattering.AvoidOverlappingObjects", false, SettingsScope.Project);
        /// <summary>
        /// Set the size of previews in the loadout and prefab palette.
        /// </summary>
        [UserSetting]
        static Pref<int> s_PreviewThumbSize = new Pref<int>("ScatteringEditor.PreviewThumSize", 64, SettingsScope.Project);

        double m_LastBrushApplication = 0.0;

		PrefabPalette[] m_AvailablePalettes = null;
		string[] m_AvailablePalettesAsStrings = null;
		int m_CurrentPaletteIndex = -1;

        // all instances of prefabs in the current palette in this scene.
        List<GameObject> m_PrefabsInstances = null;

        internal PrefabPalette prefabPalette
		{
			get
			{
				return m_PrefabPalette;
			}
			set
			{
				m_PrefabPalette = value;
			}
		}

		// An Editor for the prefabLoadouts, managing multiple editors for PrefabPalettes.
		internal PrefabLoadoutEditor prefabLoadoutEditor = null;

		internal override string UndoMessage { get { return "Paint Prefabs"; } }
		protected override string ModeSettingsHeader { get { return "Prefab Scatter Settings"; } }
		protected override string DocsLink { get { return PrefUtility.documentationPrefabPlacementBrushLink; } }

		GUIContent m_GCUsePrefabPivot = new GUIContent("Use Pivot", "By default Polybrush will position placed objects entirely on top of the target plane.  When 'Use Pivot' is enabled objects will instead be placed by their assigned mesh origin.");
		GUIContent m_GCHitSurfaceIsParent = new GUIContent("Hit Surface is Parent", "When enabled any instantiated prefab from this mode will be automatically made a child of the surface it was placed on.");
		GUIContent m_GcAvoidOverlappingGameObjects = new GUIContent("Avoid Overlap", "If enabled Polybrush will attempt to avoid placing prefabs where they may overlap with another placed GameObject.");

		static string FormatInstanceName(GameObject go)
		{
			return string.Format("{0}(Polybrush Clone)", go.name);
		}



        internal override void OnEnable()
		{
			base.OnEnable();

			RefreshAvailablePalettes();
            if (prefabPalette == null)
            {
                prefabPalette = m_AvailablePalettes[0];
            }
            prefabLoadoutEditor = new PrefabLoadoutEditor(m_AvailablePalettes.ToList(), m_PrefabPalette);
        }

        internal override void OnDisable()
        {
            if (prefabLoadoutEditor != null)
            {
                var loadout = new PrefabLoadout(prefabLoadoutEditor.CurrentLoadout);
                var js = JsonUtility.ToJson(loadout);
                EditorPrefs.SetString(k_PrefabLoadouts, js);
            }
        }

        // Inspector GUI shown in the Editor window.  Base class shows BrushSettings by default
        internal override void DrawGUI(BrushSettings brushSettings)
		{
			base.DrawGUI(brushSettings);
            
            /// Verify dependencies
            VerifyLoadedAssetsIntegrity();

            EditorGUI.BeginChangeCheck();

            /// Interface
            s_UsePivotForPlacement.value = PolyGUILayout.Toggle(m_GCUsePrefabPivot, s_UsePivotForPlacement);

			s_ParentObjectWithSurface.value = PolyGUILayout.Toggle(m_GCHitSurfaceIsParent, s_ParentObjectWithSurface);
			s_AvoidOverlappingGameObjects.value = PolyGUILayout.Toggle(m_GcAvoidOverlappingGameObjects, s_AvoidOverlappingGameObjects);

			EditorGUI.BeginChangeCheck();
            m_CurrentPaletteIndex = EditorGUILayout.Popup(m_CurrentPaletteIndex, m_AvailablePalettesAsStrings);
			if(EditorGUI.EndChangeCheck())
			{
				if(m_CurrentPaletteIndex >= m_AvailablePalettes.Length)
					SetPrefabPalette( PrefabPaletteEditor.AddNew() );
				else
					SetPrefabPalette(m_AvailablePalettes[m_CurrentPaletteIndex]);
			}

            using (new GUILayout.HorizontalScope())
            {
                s_PreviewThumbSize.value = (int)EditorGUILayout.Slider("Preview Size", (float)s_PreviewThumbSize, 60f, 128f);

            }

            if (EditorGUI.EndChangeCheck())
            {
                if (m_CurrentPaletteIndex >= m_AvailablePalettes.Length)
                    SetPrefabPalette(PrefabPaletteEditor.AddNew());
                else
                    SetPrefabPalette(m_AvailablePalettes[m_CurrentPaletteIndex]);

                PolybrushSettings.Save();
            }

            using (new GUILayout.VerticalScope())
            {
                if (prefabLoadoutEditor != null)
                    prefabLoadoutEditor.OnInspectorGUI_Internal(s_PreviewThumbSize);
            }
        }

        internal override bool SetDefaultSettings()
        {
            RefreshAvailablePalettes();
            PrefabPalette defaultPalette = m_AvailablePalettes.FirstOrDefault(x => x.name.Contains("Default"));
            if(defaultPalette == null)
                return false;

            SetPrefabPalette(defaultPalette);

            return true;
        }

		internal override void OnBrushSettingsChanged(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushSettingsChanged(target, settings);
		}

		// Called when the mouse begins hovering an editable object.
		internal override void OnBrushEnter(EditableObject target, BrushSettings settings)
		{
			base.OnBrushEnter(target, settings);
		}

		// Called whenever the brush is moved.  Note that @target may have a null editableObject.
		internal override void OnBrushMove(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushMove(target, settings);
		}

		// Called when the mouse exits hovering an editable object.
		internal override void OnBrushExit(EditableObject target)
		{
			base.OnBrushExit(target);
		}

		internal override void OnBrushBeginApply(BrushTarget target, BrushSettings settings)
		{
			base.OnBrushBeginApply(target, settings);
			m_PrefabsInstances = PolySceneUtility.FindInstancesInScene(prefabPalette.prefabs.Select(x => x.gameObject), FormatInstanceName).ToList();
		}

		// Called every time the brush should apply itself to a valid target.  Default is on mouse move.
		internal override void OnBrushApply(BrushTarget target, BrushSettings settings)
		{
			bool invert = settings.isUserHoldingControl;

			if( (EditorApplication.timeSinceStartup - m_LastBrushApplication) > Mathf.Max(.06f, (1f - settings.strength)) )
			{
				m_LastBrushApplication = EditorApplication.timeSinceStartup;

				if(invert)
				{
					foreach(PolyRaycastHit hit in target.raycastHits)
						RemoveGameObjects(hit, target, settings);
				}
				else
				{
                    if (GetPrefab() != null)
					    foreach(PolyRaycastHit hit in target.raycastHits)
						    PlaceGameObject(hit, GetPrefab(), target, settings);
				}
			}
		}

        /// <summary>
        /// Handle Undo locally since it doesn't follow the same pattern as mesh modifications.
        /// </summary>
        /// <param name="brushTarget"></param>
        internal override void RegisterUndo(BrushTarget brushTarget) {}

        void PlaceGameObject(PolyRaycastHit hit, PrefabAndSettings prefabAndSettings, BrushTarget target, BrushSettings settings)
		{
			if(prefabAndSettings == null)
				return;

            GameObject prefab = prefabAndSettings.gameObject;

			Ray ray = RandomRay(hit.position, hit.normal, settings.radius, settings.falloff, settings.falloffCurve);

			PolyRaycastHit rand_hit;

			Vector3[] vertices = target.editableObject.editMesh.vertices;
			int[] triangles = target.editableObject.editMesh.GetTriangles();

			if( PolySceneUtility.MeshRaycast(ray, vertices, triangles, out rand_hit) )
			{
                PlacementSettings placementSettings = prefabAndSettings.settings;
                Vector3 scaleSetting = prefab.transform.localScale;
                if (placementSettings.uniformBool)
                {
                    float uniformScale = Random.Range(placementSettings.uniformScale.x, placementSettings.uniformScale.y);
                    scaleSetting *= uniformScale;
                }
                else
                {
                    if (placementSettings.xScaleBool)
                        scaleSetting.x = Random.Range(placementSettings.scaleRangeMin.x, placementSettings.scaleRangeMax.x);
                    if (placementSettings.yScaleBool)
                        scaleSetting.y = Random.Range(placementSettings.scaleRangeMin.y, placementSettings.scaleRangeMax.y);
                    if (placementSettings.zScaleBool)
                        scaleSetting.z = Random.Range(placementSettings.scaleRangeMin.z, placementSettings.scaleRangeMax.z);
                }

                Vector3 rotationSetting = Vector3.zero;
                if (placementSettings.xRotationBool)
                    rotationSetting.x = Random.Range(placementSettings.rotationRangeMin.x, placementSettings.rotationRangeMax.x);
                if (placementSettings.yRotationBool)
                    rotationSetting.y = Random.Range(placementSettings.rotationRangeMin.y, placementSettings.rotationRangeMax.y);
                if (placementSettings.xRotationBool)
                    rotationSetting.z = Random.Range(placementSettings.rotationRangeMin.z, placementSettings.rotationRangeMax.z);
               

				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, target.transform.TransformDirection(rand_hit.normal));

                GameObject inst = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = target.transform.TransformPoint(rand_hit.position);
                inst.transform.rotation = rotation;
                inst.transform.localScale = scaleSetting;

                float pivotOffset = s_UsePivotForPlacement ? 0f : GetPivotOffset(inst);

                inst.name = FormatInstanceName(prefab);

                inst.transform.position = inst.transform.position - (inst.transform.up * pivotOffset);
                inst.transform.rotation = inst.transform.rotation * Quaternion.Euler(rotationSetting);
    

                if ( s_AvoidOverlappingGameObjects && TestIntersection(inst) )
				{
					Object.DestroyImmediate(inst);
					return;
				}

				if( s_ParentObjectWithSurface )
					inst.transform.SetParent(target.transform);

				m_PrefabsInstances.Add(inst);

				Undo.RegisterCreatedObjectUndo(inst, UndoMessage);
			}
		}

		void RemoveGameObjects(PolyRaycastHit hit, BrushTarget target, BrushSettings settings)
		{
			Vector3 worldHitPosition = target.editableObject.transform.TransformPoint(hit.position);

			int count = m_PrefabsInstances.Count;
            
			for(int i = 0; i < count; i++)
			{
                // Skip the object if prefab is not part of the current loadout.
                if (!prefabLoadoutEditor.ContainsPrefabInstance(m_PrefabsInstances[i]))
                    continue;

                if ( m_PrefabsInstances[i] != null && Vector3.Distance(worldHitPosition, m_PrefabsInstances[i].transform.position) < settings.radius )
                {
                    GameObject go = m_PrefabsInstances[i];
					m_PrefabsInstances.RemoveAt(i);
					count--;
					Undo.DestroyObjectImmediate(go);
				}
			}
		}

		Ray RandomRay(Vector3 position, Vector3 normal, float radius, float falloff, AnimationCurve curve)
		{
			Vector3 a = Vector3.zero;
			Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

			a.x = Mathf.Cos(Random.Range(0f, 360f));
			a.y = Mathf.Sin(Random.Range(0f, 360f));

			float r = Mathf.Sqrt(Random.Range(0f, 1f));

			while(true)
			{
				// this isn't great
				if(r < falloff || Random.Range(0f, 1f) > Mathf.Clamp(curve.Evaluate( 1f - ((r - falloff) / (1f - falloff))), 0f, 1f))
				{
					a = position + (rotation * (a.normalized * r * radius));
					return new Ray(a + normal * 10f, -normal);
				}
				else
				{
					r = Mathf.Sqrt(Random.Range(0f, 1f));
				}
			}
		}

		PrefabAndSettings GetPrefab()
		{
            return prefabLoadoutEditor.GetRandomLoadout();
		}

		float GetPivotOffset(GameObject go)
		{
            Bounds bounds = BoundsUtility.GetHierarchyBounds(go);

            // If size y = 0, there's likely no mesh renderers in the object.
            if (bounds.size.y == 0)
                return 0f;

            Vector3 pivotToBoundsCenter = (bounds.center - go.transform.position);
            Vector3 offset = pivotToBoundsCenter - bounds.extents;

            return offset.y;
		}

        /// <summary>
        /// Tests if go intersects with any painted objects.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        bool TestIntersection(GameObject go)
		{
            BoundsUtility.SphereBounds bounds, it_bounds;

			if(!BoundsUtility.GetSphereBounds(go, out bounds))
				return false;

			int c = m_PrefabsInstances == null ? 0 : m_PrefabsInstances.Count;

			for(int i = 0; i < c; i++)
			{
				if(BoundsUtility.GetSphereBounds(m_PrefabsInstances[i], out it_bounds) && bounds.Intersects(it_bounds))
					return true;
			}

			return false;
		}

        void SetPrefabPalette(PrefabPalette palette)
        {
            prefabPalette = palette;
            prefabLoadoutEditor.ChangePalette(palette);
            RefreshAvailablePalettes();
        }

        void RefreshAvailablePalettes()
        {
            m_AvailablePalettes = PolyEditorUtility.GetAll<PrefabPalette>().ToArray();

            if (m_AvailablePalettes.Length < 1)
                prefabPalette = PolyEditorUtility.GetFirstOrNew<PrefabPalette>();

            m_AvailablePalettesAsStrings = m_AvailablePalettes.Select(x => x.name).ToArray();
            ArrayUtility.Add<string>(ref m_AvailablePalettesAsStrings, string.Empty);
            ArrayUtility.Add<string>(ref m_AvailablePalettesAsStrings, "Add Palette...");
            m_CurrentPaletteIndex = System.Array.IndexOf(m_AvailablePalettes, m_PrefabPalette);
        }

        /// <summary>
        /// Verify if all loaded assets haven't been touched by users.
        /// If one or multiples assets are missing, refresh the Palettes list and loadouts.
        /// </summary>
        void VerifyLoadedAssetsIntegrity()
        {
            if (m_AvailablePalettes.Length > 0 &&
                !System.Array.TrueForAll(m_AvailablePalettes, x => x != null))
            {
                RefreshAvailablePalettes();
                m_CurrentPaletteIndex = 0;
                if (m_AvailablePalettes.Length > 0)
                {
                    SetPrefabPalette(m_AvailablePalettes[m_CurrentPaletteIndex]);
                }
                else
                    SetPrefabPalette(PrefabPaletteEditor.AddNew());

                prefabLoadoutEditor.RefreshPalettesList(m_AvailablePalettes.ToList());
            }
        }

#if POLYBRUSH_DEBUG
        internal override void DrawGizmos(BrushTarget target, BrushSettings settings)
        {
            base.DrawGizmos(target, settings);

            if (m_PrefabsInstances == null)
                return;

            SphereBounds bounds;

            foreach (GameObject go in m_PrefabsInstances)
            {
                if (!GetSphereBounds(go, out bounds))
                    continue;

                Handles.CircleCap(-1, bounds.position, Quaternion.Euler(Vector3.up * 90f), bounds.radius);
                Handles.CircleCap(-1, bounds.position, Quaternion.Euler(Vector3.right * 90f), bounds.radius);
                Handles.CircleCap(-1, bounds.position, Quaternion.Euler(Vector3.forward * 90f), bounds.radius);
            }
        }
#endif
    }
}
