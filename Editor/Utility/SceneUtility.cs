using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;
using System;
using Math = UnityEngine.Polybrush.Math;


namespace UnityEditor.Polybrush
{
	internal static class PolySceneUtility
	{
        [UserSetting]
        internal static Pref<int> s_GIWorkflowMode = new Pref<int>("GI.WorkflowMode", (int)Lightmapping.giWorkflowMode, SettingsScope.Project);

		internal static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
		{
			Vector3 o = InWorldRay.origin;
			o -= transform.position;
			o = transform.worldToLocalMatrix * o;
			Vector3 d = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
			return new Ray(o, d);
		}

        /// <summary>
        /// Find the nearest triangle intersected by InWorldRay on this mesh.  InWorldRay is in world space.
        /// @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
        /// point may be.  @cullingMode determines what face orientations are tested (Culling.Front only tests front
        /// faces, Culling.Back only tests back faces, and Culling.FrontBack tests both).
        /// Ray origin and position values are in local space.
        /// </summary>
        /// <param name="InWorldRay"></param>
        /// <param name="transform"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="hit"></param>
        /// <param name="distance"></param>
        /// <param name="cullingMode"></param>
        /// <returns></returns>
        internal static bool WorldRaycast(Ray InWorldRay, Transform transform, Vector3[] vertices, int[] triangles, out PolyRaycastHit hit, float distance = Mathf.Infinity, Culling cullingMode = Culling.Front)
		{
            //null checks, must have a transform, vertices and triangles
            if(transform == null || vertices == null || triangles == null )
            {
                hit = null;
                return false;
            }


			Ray ray = transform.InverseTransformRay(InWorldRay);
			return MeshRaycast(ray, vertices, triangles, out hit, distance, cullingMode);
		}

        /// <summary>
        /// Cast a ray (in model space) against a mesh.
        /// </summary>
        /// <param name="InRay"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="hit"></param>
        /// <param name="distance"></param>
        /// <param name="cullingMode"></param>
        /// <returns></returns>
        internal static bool MeshRaycast(Ray InRay, Vector3[] vertices, int[] triangles, out PolyRaycastHit hit, float distance = Mathf.Infinity, Culling cullingMode = Culling.Front)
		{
            Vector3 hitNormal = Vector3.zero;	// vars used in loop
			Vector3 vert0, vert1, vert2;
			Vector3 origin = InRay.origin, direction = InRay.direction;

            hit = new PolyRaycastHit(Mathf.Infinity,
                                    Vector3.zero,
                                    Vector3.zero,
                                    -1);

            /**
			 * Iterate faces, testing for nearest hit to ray origin.
			 */
            for (int CurTri = 0; CurTri < triangles.Length; CurTri += 3)
			{
                if (CurTri + 2 >= triangles.Length) continue;
                if (triangles[CurTri + 2] >= vertices.Length) continue;

				vert0 = vertices[triangles[CurTri+0]];
				vert1 = vertices[triangles[CurTri+1]];
				vert2 = vertices[triangles[CurTri+2]];

                // Second pass, test intersection with triangle
                if (Math.RayIntersectsTriangle2(origin, direction, vert0, vert1, vert2, ref distance, ref hitNormal))
				{
                    if (distance < hit.distance)
                    {
                        hit.distance = distance;
                        hit.triangle = CurTri / 3;
                        hit.position = InRay.GetPoint(hit.distance);
                        hit.normal = hitNormal;
                    }

                }
			}

			hit = new PolyRaycastHit( hit.distance,
									InRay.GetPoint(hit.distance),
									hitNormal,
									hit.triangle);

			return hit.triangle > -1;
		}

        /// <summary>
        /// Returns true if the event is one that should consume the mouse or keyboard.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static bool SceneViewInUse(Event e)
		{
			return 	e.alt
					|| Tools.current == Tool.View
					|| GUIUtility.hotControl > 0
					|| (e.isMouse ? e.button > 1 : false)
					|| Tools.viewTool == ViewTool.FPS
					|| Tools.viewTool == ViewTool.Orbit;
		}

        static Vector3[] s_WorldBuffer = new Vector3[4096];

        /// <summary>
        /// Calculates the per-vertex weight for each raycast hit and fills in brush target weights.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="settings"></param>
        /// <param name="tool"></param>
        /// <param name="bMode"></param>
        internal static void CalculateWeightedVertices(BrushTarget target, BrushSettings settings, BrushTool tool = BrushTool.None, BrushMode bMode = null)
		{
            if(target == null || settings == null)
                return;

			if(target.editableObject == null)
                return;

            bool uniformScale = Math.VectorIsUniform(target.transform.lossyScale);
			float scale = uniformScale ? 1f / target.transform.lossyScale.x : 1f;

			PolyMesh mesh = target.editableObject.visualMesh;

            Transform transform = target.transform;
            int vertexCount = mesh.vertexCount;
            Vector3[] vertices = mesh.vertices;

            if (!uniformScale)
            {
                // As we only increase size only when it's needed, always make sure to
                // use the vertexCount variable in loop statements and not the buffer length.
                if (s_WorldBuffer.Length < vertexCount)
                    System.Array.Resize<Vector3>(ref s_WorldBuffer, vertexCount);

                for (int i = 0; i < vertexCount; i++)
                    s_WorldBuffer[i] = transform.TransformPoint(vertices[i]);
                vertices = s_WorldBuffer;
            }

            AnimationCurve curve = settings.falloffCurve;
            float radius = settings.radius * scale, falloff_mag = Mathf.Max((radius - radius * settings.falloff), 0.00001f);

            Vector3 hitPosition = Vector3.zero;
            PolyRaycastHit hit;

            if (tool == BrushTool.Texture && mesh.subMeshCount > 1)
            {
                var mode = bMode as BrushModeTexture;
                int[] submeshIndices = mesh.subMeshes[mode.currentMeshACIndex].indexes;

                for (int n = 0; n < target.raycastHits.Count; n++)
                {
                    hit = target.raycastHits[n];
                    hit.SetVertexCount(vertexCount);

                    for(int i = 0; i < vertexCount; i++)
                    {
                        hit.weights[i] = 0f;
                    }

                    hitPosition = uniformScale ? hit.position : transform.TransformPoint(hit.position);

                    for (int i = 0; i < submeshIndices.Length; i++)
                    {
                        int currentIndex = submeshIndices[i];
                        float dist = (hitPosition - vertices[currentIndex]).magnitude;
                        float delta = radius - dist;

                        if (delta >= 0)
                        {
                            float weight = Mathf.Clamp(curve.Evaluate(1f - Mathf.Clamp(delta / falloff_mag, 0f, 1f)), 0f, 1f);

                            hit.weights[currentIndex] = weight;
                        }
                    }
                }
            }
            else
            {
                int[][] common = PolyMeshUtility.GetCommonVertices(mesh);

                Vector3 buf = Vector3.zero;

                for (int n = 0; n < target.raycastHits.Count; n++)
                {
                    hit = target.raycastHits[n];
                    hit.SetVertexCount(vertexCount);

                    hitPosition = uniformScale ? hit.position : transform.TransformPoint(hit.position);

                    for (int i = 0; i < common.Length; i++)
                    {
                        int[] commonItem = common[i];
                        int commonArrayCount = commonItem.Length;

                        Math.Subtract(vertices[commonItem[0]], hitPosition, ref buf);

                        float sqrDist = buf.sqrMagnitude;

                        if (sqrDist > radius * radius)
                        {
                            for (int j = 0; j < commonArrayCount; j++)
                                hit.weights[commonItem[j]] = 0f;
                        }
                        else
                        {
                            float weight = Mathf.Clamp(curve.Evaluate(1f - Mathf.Clamp((radius - Mathf.Sqrt(sqrDist)) / falloff_mag, 0f, 1f)), 0f, 1f);

                            for (int j = 0; j < commonArrayCount; j++)
                            {
                                hit.weights[commonItem[j]] = weight;
                            }
                        }
                    }
                }
            }

			target.GetAllWeights(true);
		}

		internal static IEnumerable<GameObject> FindInstancesInScene(IEnumerable<GameObject> match, System.Func<GameObject, string> instanceNamingFunc)
		{
            //null checks
            if(match == null || instanceNamingFunc == null)
            {
                return null;
            }

			IEnumerable<string> matches = match.Where(x => x != null).Select(y => instanceNamingFunc(y));

			return UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => {
				return matches.Contains( x.name );
				});
		}

        /// <summary>
        /// Store the previous GIWorkflowMode and set the current value to OnDemand (or leave it Legacy).
        /// </summary>
        internal static void PushGIWorkflowMode()
		{
            s_GIWorkflowMode.value = (int)Lightmapping.giWorkflowMode;
            PolybrushSettings.Save();

            if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.Legacy)
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
		}

        /// <summary>
        /// Return GIWorkflowMode to it's prior state.
        /// </summary>
        internal static void PopGIWorkflowMode()
		{
            Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)s_GIWorkflowMode.value;
		}
	}
}
