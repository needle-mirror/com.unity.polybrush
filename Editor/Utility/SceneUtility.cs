using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;
using UnityEditor.SettingsManagement;
using System;
using UnityEngine.Profiling;
using Math = UnityEngine.Polybrush.Math;


namespace UnityEditor.Polybrush
{
	internal static class PolySceneUtility
	{
#pragma warning disable 618
        [UserSetting]
        internal static Pref<int> s_GIWorkflowMode = new Pref<int>("GI.WorkflowMode", (int)Lightmapping.giWorkflowMode, SettingsScope.Project);
#pragma warning restore 618

        const string k_shaderPath = "/Content/ComputeShader/MeshRaycastCS.compute";
        const float k_maxCSIntersectionDist = 1000000f;
        static ComputeShader m_RaycastShader;

        public static ComputeShader raycastShader
        {
            get
            {
                if(m_RaycastShader == null)
                    m_RaycastShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(PolyEditorUtility.RootFolder + k_shaderPath);

                if(m_RaycastShader == null)
                    Debug.LogWarning("Compute Shader not found at "+PolyEditorUtility.RootFolder + k_shaderPath);

                return m_RaycastShader;
            }
        }

		internal static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
		{
			Vector3 o = InWorldRay.origin;
			o -= transform.position;
			o = transform.worldToLocalMatrix * o;
			Vector3 d = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);

			return new Ray(o, d);
		}

        /// <summary>
        /// Only used for tests using legacy raycasting.
        ///
        /// Find the nearest triangle intersected by InWorldRay on this mesh.  InWorldRay is in world space.
        /// @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
        /// point may be.
        /// Ray origin and position values are in local space.
        /// </summary>
        /// <param name="InWorldRay"></param>
        /// <param name="transform"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        internal static bool WorldRaycast_Legacy(Ray InWorldRay, Transform transform, Vector3[] vertices, int[] triangles, out PolyRaycastHit hit)
        {
            //null checks, must have a transform, vertices and triangles
            if(transform == null || vertices == null || triangles == null )
            {
                hit = null;
                return false;
            }

            //empty checks for vertices and triangles
            if(vertices.Length == 0 || triangles.Length == 0)
            {
                hit = null;
                return false;
            }

            Ray ray = transform.InverseTransformRay(InWorldRay);
            return MeshRaycast_Legacy(ray, vertices, triangles, out hit);
        }

        /// <summary>
        /// Find the nearest triangle intersected by InWorldRay on this mesh.  InWorldRay is in world space.
        /// @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
        /// point may be.
        /// Ray origin and position values are in local space.
        /// </summary>
        /// <param name="InWorldRay"></param>
        /// <param name="transform"></param>
        /// <param name="mesh"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        internal static bool WorldRaycast(Ray InWorldRay, Transform transform, PolyMesh mesh, out PolyRaycastHit hit)
        {
            //null checks, must have a transform and a mesh
            if(transform == null
                || mesh == null
                || mesh.vertexCount == 0
                || mesh.GetTriangles().Length == 0)
            {
                hit = null;
                return false;
            }

            Ray ray = transform.InverseTransformRay(InWorldRay);
            return MeshRaycast(ray, mesh, out hit);
        }

        /// <summary>
        /// Cast a ray (in model space) against a mesh.
        /// </summary>
        /// <param name="InRay"></param>
        /// <param name="mesh"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        internal static bool MeshRaycast(Ray InRay, PolyMesh mesh, out PolyRaycastHit hit)
        {
            hit = default;

            Profiler.BeginSample("PolyBrush MeshRaycast");

            hit = new PolyRaycastHit(Mathf.Infinity,
                Vector3.zero,
                Vector3.zero,
                -1);

            if(SystemInfo.supportsComputeShaders)
                return MeshRaycast_ComputeShader(InRay, mesh, out hit);
            else
                return MeshRaycast_Legacy(InRay, mesh.vertices, mesh.GetTriangles(), out hit);
        }

        /// <summary>
        /// Cast a ray (in model space) against a mesh using compute shaders.
        /// </summary>
        internal static bool MeshRaycast_ComputeShader(Ray InRay, PolyMesh mesh, out PolyRaycastHit hit)
        {
            hit = null;

            int kernelIndex = raycastShader.FindKernel("MeshRaycastCS");

            ComputeBuffer vertexBuffer = mesh.vertexBuffer;
            ComputeBuffer triangleBuffer = mesh.triangleBuffer;

            float[] hitDistances = new float[mesh.triangleBuffer.count/3];

            ComputeBuffer resultBuffer = new ComputeBuffer(hitDistances.Length, sizeof(float));
            resultBuffer.SetData(hitDistances);

            if(raycastShader == null)
            {
                Profiler.EndSample();
                return false;
            }

            raycastShader.SetBuffer(kernelIndex, "vertexBuffer", vertexBuffer);
            raycastShader.SetBuffer(kernelIndex, "triangleBuffer", triangleBuffer);
            raycastShader.SetBuffer(kernelIndex, "resultHits", resultBuffer);

            raycastShader.SetVector("rayOrigin", InRay.origin);
            raycastShader.SetVector("rayDirection", InRay.direction);

            raycastShader.SetFloat("epsilon", Mathf.Epsilon);
            raycastShader.SetFloat("infinityValue", k_maxCSIntersectionDist);

            uint threadGroupSize ;
            raycastShader.GetKernelThreadGroupSizes(kernelIndex, out threadGroupSize, out _, out _);
            raycastShader.Dispatch(kernelIndex, Mathf.CeilToInt((hitDistances.Length / threadGroupSize) + 1), 1, 1);

            resultBuffer.GetData(hitDistances);
            resultBuffer.Dispose();

            int[] triangles = mesh.GetTriangles();
            int triangleIndex = -1;
            float minDistance = k_maxCSIntersectionDist;
            for(int i = 0; i < hitDistances.Length; i++)
            {
                if(hitDistances[i] < minDistance)
                {
                    triangleIndex = i;
                    minDistance = hitDistances[i];
                }
            }

            if(triangleIndex == -1)
            {
                Profiler.EndSample();
                return false;
            }

            var vert0 = mesh.vertices[triangles[3 * triangleIndex]];
            var v1 = mesh.vertices[triangles[3 * triangleIndex + 1]] - vert0;
            var v2 = mesh.vertices[triangles[3 * triangleIndex + 2]] - vert0;
            var normal = Vector3.Cross(v1, v2).normalized;

            hit = new PolyRaycastHit(minDistance, InRay.GetPoint(minDistance), normal, triangleIndex);

            Profiler.EndSample();
			return hit.triangle > -1;
		}

        /// <summary>
        /// Cast a ray (in model space) against a mesh without the use of compute shader (when the system does not support these).
        /// </summary>
        internal static bool MeshRaycast_Legacy(Ray InRay, Vector3[] vertices, int[] triangles, out PolyRaycastHit hit)
        {
            Vector3 hitNormal, vert0, vert1, vert2;
            Vector3 origin = InRay.origin, direction = InRay.direction;

            hit = new PolyRaycastHit(Mathf.Infinity,
                Vector3.zero,
                Vector3.zero,
                -1);

            float distance = k_maxCSIntersectionDist;

            // Iterate faces, testing for nearest hit to ray origin.
            for (int CurTri = 0; CurTri < triangles.Length; CurTri += 3)
            {
                if (CurTri + 2 >= triangles.Length) continue;
                if (triangles[CurTri + 2] >= vertices.Length) continue;

                vert0 = vertices[triangles[CurTri+0]];
                vert1 = vertices[triangles[CurTri+1]];
                vert2 = vertices[triangles[CurTri+2]];

                // Second pass, test intersection with triangle
                if (Math.RayIntersectsTriangle2(origin, direction, vert0, vert1, vert2, out distance, out hitNormal))
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
                int[] submeshIndices = mesh.subMeshes[mode.m_CurrentMeshACIndex].indexes;

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

#if UNITY_2020_3 || UNITY_2021_3 || UNITY_2022_2_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(x => matches.Contains(x.name));
#else
            return UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => {
                return matches.Contains(x.name);
            });
#endif
        }

        /// <summary>
        /// Store the previous GIWorkflowMode and set the current value to OnDemand (or leave it Legacy).
        /// </summary>
        internal static void PushGIWorkflowMode()
		{
#pragma warning disable 618
            s_GIWorkflowMode.value = (int)Lightmapping.giWorkflowMode;
            PolybrushSettings.Save();

            if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.Legacy)
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#pragma warning restore 618
		}

        /// <summary>
        /// Return GIWorkflowMode to it's prior state.
        /// </summary>
        internal static void PopGIWorkflowMode()
		{
#pragma warning disable 618
            Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)s_GIWorkflowMode.value;
#pragma warning restore 618
		}
	}
}
