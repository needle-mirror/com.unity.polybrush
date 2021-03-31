#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
#define UNITY_5_4_OR_LOWER
#else
#define UNITY_5_5_OR_HIGHER
#endif

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Draw scene view handles and gizmos.
    /// </summary>
    static class PolyHandles
	{
		internal static void DrawBrush(	Vector3 point,
										Vector3 normal,
										BrushSettings brushSettings,
										Matrix4x4 matrix,
										Color innerColor,
										Color outerColor)
        {
            Vector3 p = matrix.MultiplyPoint3x4(point);
            Vector3 n = matrix.inverse.MultiplyVector(normal);

            DrawBrushDisc(p, n, brushSettings.radius, brushSettings.falloff, innerColor, outerColor);

            // normal indicator
            Handles.color = new Color(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z), 1f);
            Handles.DrawLine(p, p + n.normalized * HandleUtility.GetHandleSize(p));
        }

		static void DrawBrushSphere(Vector3 p, Vector3 n, float radius, float falloff, Color innerColor, Color outerColor)
        {
            var ztest = Handles.zTest;

			// radius
			Handles.color = outerColor;
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawWireDisc(p, n, radius);

            Handles.zTest = CompareFunction.Greater;
			Handles.color = outerColor * .6f;
            Handles.DrawWireDisc(p, n, radius);

			// falloff
			Handles.color = innerColor;
            Handles.zTest = CompareFunction.LessEqual;
            DrawWireSphere(p, n, radius * falloff);

			Handles.color = innerColor * .6f;
            Handles.zTest = CompareFunction.Greater;
			DrawWireSphere(p, n, radius * falloff);

            Handles.zTest = ztest;
		}

		static void DrawBrushDisc(Vector3 p, Vector3 n, float radius, float falloff, Color innerColor, Color outerColor)
        {
            Handles.color = outerColor;
            Handles.DrawWireDisc(p, n, radius);

            Handles.color = innerColor;
            Handles.DrawWireDisc(p, n, radius * falloff);
        }

        static void DrawWireSphere(Vector3 center, Vector3 normal, float radius)
        {
            Vector3 nt = normal == Vector3.up
                ? Vector3.Cross(normal, Vector3.right)
                : Vector3.Cross(normal, Vector3.up);

            Vector3 nb = Vector3.Cross(normal, nt);
            nt.Normalize();
            nb.Normalize();

            Handles.DrawWireDisc(center, normal,radius);
            Handles.DrawWireDisc(center, nt,radius);
            Handles.DrawWireDisc(center, nb,radius);
        }

		internal static void DrawScatterBrush(Vector3 point, Vector3 normal, BrushSettings settings, Matrix4x4 localToWorldMatrix)
		{
			Vector3 p = localToWorldMatrix.MultiplyPoint3x4(point);
			Vector3 n = localToWorldMatrix.MultiplyVector(normal).normalized;

			float r = settings.radius;
			Vector3 a = Vector3.zero;
			Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

			for(int i = 0; i < 10; i++)
			{
				a.x = Mathf.Cos(Random.Range(0f, 360f));
				a.y = Mathf.Sin(Random.Range(0f, 360f));
				a = a.normalized * Random.Range(0f, r);

				Vector3 v = localToWorldMatrix.MultiplyPoint3x4(point + rotation * a);

				Handles.DrawLine(v, v  + (n * .5f));
				Handles.CubeHandleCap(i + 2302, v, Quaternion.identity, .01f, Event.current.type);
			}

			/// radius
			Handles.DrawWireDisc(p, n, settings.radius);
		}
	}
}
