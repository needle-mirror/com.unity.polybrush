using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace UnityEditor.Polybrush
{
    internal static class ProBuilderBridge
    {
        /// <summary>
        /// Defines what objects are selectable for the scene tool.
        /// </summary>
        [System.Flags]
        public enum SelectMode
        {
            /// <summary>
            /// No selection mode defined.
            /// </summary>
            None = 0 << 0,

            /// <summary>
            /// Objects are selectable.
            /// </summary>
            Object = 1 << 0,

            /// <summary>
            /// Vertices are selectable.
            /// </summary>
            Vertex = 1 << 1,

            /// <summary>
            /// Edges are selectable.
            /// </summary>
            Edge = 1 << 2,

            /// <summary>
            /// Faces are selectable.
            /// </summary>
            Face = 1 << 3,

            /// <summary>
            /// Texture coordinates are selectable.
            /// </summary>
            TextureFace = 1 << 4,

            /// <summary>
            /// Texture coordinates are selectable.
            /// </summary>
            TextureEdge = 1 << 5,

            /// <summary>
            /// Texture coordinates are selectable.
            /// </summary>
            TextureVertex = 1 << 6,

            /// <summary>
            /// Other input tool (Poly Shape editor, Bezier editor, etc)
            /// </summary>
            InputTool = 1 << 7,

            /// <summary>
            /// Match any value.
            /// </summary>
            Any = 0xFFFF
        }

        /// <summary>
        /// Selectively rebuild and apply mesh attributes to the UnityEngine.Mesh asset.
        /// </summary>
        /// <seealso cref="ProBuilderMesh.Refresh"/>
        [System.Flags]
        public enum RefreshMask
        {
            /// <summary>
            /// Textures channel will be rebuilt.
            /// </summary>
            UV = 0x1,

            /// <summary>
            /// Colors will be rebuilt.
            /// </summary>
            Colors = 0x2,

            /// <summary>
            /// Normals will be recalculated and applied.
            /// </summary>
            Normals = 0x4,

            /// <summary>
            /// Tangents will be recalculated and applied.
            /// </summary>
            Tangents = 0x8,

            /// <summary>
            /// Re-assign the MeshCollider sharedMesh.
            /// </summary>
            Collisions = 0x10,

            /// <summary>
            /// Refresh all optional mesh attributes.
            /// </summary>
            All = UV | Colors | Normals | Tangents | Collisions
        };

        const string k_ProBuilderName =
            "UnityEditor.ProBuilder.ProBuilderEditor, Unity.ProBuilder.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        const string k_ProBuilderMeshName =
            "UnityEngine.ProBuilder.ProBuilderMesh, Unity.ProBuilder, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        const string k_RefreshMaskName =
            "UnityEngine.ProBuilder.RefreshMask, Unity.ProBuilder, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        const string k_EditorUtilityName =
            "UnityEditor.ProBuilder.EditorMeshUtility, Unity.ProBuilder.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        static Type s_ProBuilderType = null;
        static Type s_ProBuilderMeshType = null;
        static Type s_RefreshMaskType = null;
        static Type s_EditorUtilityType = null;

        static MethodInfo m_PolybrushOnSelectModeListenerMethodInfo;

        static MethodInfo m_ProBuilderRefreshMethodInfo = null;
        static EventInfo m_ProBuilderOnSelectModeChanged = null;
        static PropertyInfo m_ProBuilderSelectModePropertyInfo = null;

        static MethodInfo m_ProBuilderMeshToMeshMethodInfo = null;
        static MethodInfo m_ProBuilderMeshRefreshMethodInfo = null;
        static MethodInfo m_ProBuilderMeshOptimizeMethodInfo = null;
        static MethodInfo m_ProBuilderMeshSetUVMethodInfo = null;

        static PropertyInfo m_ProBuilderMeshPositionsPropertyInfo = null;
        static PropertyInfo m_ProBuilderMeshTangentsPropertyInfo = null;
        static PropertyInfo m_ProBuilderMeshColorsPropertyInfo = null;
        static PropertyInfo m_ProBuilderMeshVertexCountPropertyInfo = null;

        internal static bool ProBuilderExists()
        {
            if (GetProBuilderType() != null &&
                GetProBuilderMeshType() != null &&
                GetRefreshMaskType() != null &&
                GetEditorUtilityType() != null)
                return true;
            return false;
        }

        static Type GetProBuilderType()
        {
            if (s_ProBuilderType == null)
                s_ProBuilderType = Type.GetType(k_ProBuilderName);

            return s_ProBuilderType;
        }
        static Type GetProBuilderMeshType()
        {
            try
            {
                if (s_ProBuilderMeshType == null)
                    s_ProBuilderMeshType = Type.GetType(k_ProBuilderMeshName);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            return s_ProBuilderMeshType;
        }
        static Type GetRefreshMaskType()
        {
            if (s_RefreshMaskType == null)
                s_RefreshMaskType = Type.GetType(k_RefreshMaskName);

            return s_RefreshMaskType;
        }
        static Type GetEditorUtilityType()
        {
            if (s_EditorUtilityType == null)
                s_EditorUtilityType = Type.GetType(k_EditorUtilityName);

            return s_EditorUtilityType;
        }

        /// <summary>
        /// Get ProBuilderMesh component from given GameObject.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static UnityEngine.Object GetProBuilderComponent(GameObject obj)
        {
            return obj.GetComponent(GetProBuilderMeshType());
        }

        static MethodInfo PolybrushOnSelectModeListenerMethodInfo
        {
            get
            {
                if (m_PolybrushOnSelectModeListenerMethodInfo == null)
                {
                    m_PolybrushOnSelectModeListenerMethodInfo = typeof(PolybrushEditor).GetMethod("OnProBuilderSelectModeChanged",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }

                return m_PolybrushOnSelectModeListenerMethodInfo;
            }
        }

        static MethodInfo ProBuilderRefreshMethodInfo
        {
            get
            {
                if (m_ProBuilderRefreshMethodInfo == null)
                {
                    m_ProBuilderRefreshMethodInfo = GetProBuilderType().GetMethod("Refresh",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new Type[] {typeof(bool)},
                        null);
                }

                return m_ProBuilderRefreshMethodInfo;
            }
        }

        static EventInfo ProBuilderOnSelectModeChanged
        {
            get
            {
                if (m_ProBuilderOnSelectModeChanged == null)
                {
                    m_ProBuilderOnSelectModeChanged = GetProBuilderType().GetEvent("selectModeChanged",
                        BindingFlags.Public | BindingFlags.Static);
                }

                return m_ProBuilderOnSelectModeChanged;
            }
        }

        static PropertyInfo ProBuilderSelectModePropertyInfo
        {
            get
            {
                if (m_ProBuilderSelectModePropertyInfo == null)
                {
                    m_ProBuilderSelectModePropertyInfo = GetProBuilderType().GetProperty("selectMode",
                        BindingFlags.Public | BindingFlags.Static);
                }

                return m_ProBuilderSelectModePropertyInfo;
            }
        }

        static MethodInfo ProBuilderMeshToMeshMethodInfo
        {
            get
            {
                if (m_ProBuilderMeshToMeshMethodInfo == null)
                {
                    m_ProBuilderMeshToMeshMethodInfo = GetProBuilderMeshType().GetMethod("ToMesh",
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new Type[] {typeof(MeshTopology)},
                        null);
                }

                return m_ProBuilderMeshToMeshMethodInfo;
            }
        }

        static MethodInfo ProBuilderMeshRefreshMethodInfo
        {
            get
            {
                if (m_ProBuilderMeshRefreshMethodInfo == null)
                {
                    m_ProBuilderMeshRefreshMethodInfo = GetProBuilderMeshType().GetMethod("Refresh",
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new Type[] {GetRefreshMaskType()},
                        null);
                }

                return m_ProBuilderMeshRefreshMethodInfo;
            }
        }

        static MethodInfo ProBuilderMeshOptimizeMethodInfo
        {
            get
            {
                if (m_ProBuilderMeshOptimizeMethodInfo == null)
                {
                    m_ProBuilderMeshOptimizeMethodInfo = GetEditorUtilityType().GetMethod("Optimize",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new Type[] {GetProBuilderMeshType(), typeof(bool)},
                        null);
                }

                return m_ProBuilderMeshOptimizeMethodInfo;
            }
        }

        static MethodInfo ProBuilderMeshSetUVMethodInfo
        {
            get
            {
                if (m_ProBuilderMeshSetUVMethodInfo == null)
                {
                    m_ProBuilderMeshSetUVMethodInfo = GetProBuilderMeshType().GetMethod("SetUVs",
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new Type[] {typeof(int), typeof(List<Vector4>)},
                        null);
                }

                return m_ProBuilderMeshSetUVMethodInfo;
            }
        }

        static PropertyInfo ProBuilderMeshPositionsPropertyInfo
        {
            get
            {
                if (m_ProBuilderMeshPositionsPropertyInfo == null)
                {
                    m_ProBuilderMeshPositionsPropertyInfo =
                        GetProBuilderMeshType().GetProperty("positions", BindingFlags.Instance | BindingFlags.Public);
                }

                return m_ProBuilderMeshPositionsPropertyInfo;
            }
        }

        static PropertyInfo ProBuilderMeshTangentsPropertyInfo
        {
            get
            {
                if (m_ProBuilderMeshTangentsPropertyInfo == null)
                {
                    m_ProBuilderMeshTangentsPropertyInfo =
                        GetProBuilderMeshType().GetProperty("tangents", BindingFlags.Instance | BindingFlags.Public);
                }

                return m_ProBuilderMeshTangentsPropertyInfo;
            }
        }

        static PropertyInfo ProBuilderMeshColorsPropertyInfo
        {
            get
            {
                if (m_ProBuilderMeshColorsPropertyInfo == null)
                {
                    m_ProBuilderMeshColorsPropertyInfo =
                        GetProBuilderMeshType().GetProperty("colors", BindingFlags.Instance | BindingFlags.Public);
                }

                return m_ProBuilderMeshColorsPropertyInfo;
            }
        }

        static PropertyInfo ProBuilderMeshVertexCountPropertyInfo
        {
            get
            {
                if (m_ProBuilderMeshVertexCountPropertyInfo == null)
                {
                    m_ProBuilderMeshVertexCountPropertyInfo =
                        GetProBuilderMeshType().GetProperty("vertexCount", BindingFlags.Instance | BindingFlags.Public);
                }

                return m_ProBuilderMeshVertexCountPropertyInfo;
            }
        }

        /// <summary>
        /// Return true if given GameObject has a ProBuilderMesh component.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static bool IsValidProBuilderMesh(GameObject obj)
        {
            if (ProBuilderExists())
            {
                Component comp = obj.GetComponent(GetProBuilderMeshType());

                return comp != null;
            }

            return false;
        }

        /// <summary>
        /// Call ProBuilderEditor.Refresh().
        /// </summary>
        /// <param name="vertexCountChanged"></param>
        internal static void RefreshEditor(bool vertexCountChanged)
        {
            if (ProBuilderRefreshMethodInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.RefreshEditor() failed to find an appropriate `Refresh` method on `ProBuilderEditor` type");
                return;
            }

            ProBuilderRefreshMethodInfo.Invoke(null, new object[] {vertexCountChanged});
        }

        /// <summary>
        /// Set ProBuilderEditor.selectMode.
        /// </summary>
        /// <param name="mode"></param>
        internal static void SetSelectMode(SelectMode mode)
        {
            if (ProBuilderSelectModePropertyInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderSelectMode() failed to find an appropriate `selectMode` property on `ProBuilder` type");
                return;
            }

            ProBuilderSelectModePropertyInfo.SetValue(null, (int) mode);
        }

        /// <summary>
        /// Return ProBuilderMesh.vertexCount value.
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        /// <returns></returns>
        internal static int GetVertexCount(GameObject obj)
        {
            if (ProBuilderMeshVertexCountPropertyInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderPosition() failed to find an appropriate `tangents` property on `ProBuilderMesh` type");
                return 0;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());
            return (int) ProBuilderMeshVertexCountPropertyInfo.GetValue(comp);
        }

        /// <summary>
        /// Assign vertices information to ProBuilderMesh.position property.
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        /// <param name="position"></param>
        internal static void SetPositions(GameObject obj, IList<Vector3> position)
        {
            if (ProBuilderMeshPositionsPropertyInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderPosition() failed to find an appropriate `positions` property on `ProBuilderMesh` type");
                return;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());
            ProBuilderMeshPositionsPropertyInfo.SetValue(comp, position);
        }

        /// <summary>
        /// Assign tangents information to ProBuilderMesh.tangents property.
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        /// <param name="tangents"></param>
        internal static void SetTangents(GameObject obj, IList<Vector4> tangents)
        {
            if (ProBuilderMeshTangentsPropertyInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderPosition() failed to find an appropriate `tangents` property on `ProBuilderMesh` type");
                return;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());

            ProBuilderMeshTangentsPropertyInfo.SetValue(comp, tangents);
        }

        /// <summary>
        /// Assign colors information to ProBuilderMesh.colors property.
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        /// <param name="colors"></param>
        internal static void SetColors(GameObject obj, IList<Color> colors)
        {
            if (ProBuilderMeshColorsPropertyInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderColors() failed to find an appropriate `colors` property on `ProBuilderMesh` type");
                return;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());

            ProBuilderMeshColorsPropertyInfo.SetValue(comp, colors);
        }

        /// <summary>
        /// Call ProBuilderMesh.SetUVs().
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        /// <param name="index"></param>
        /// <param name="uv"></param>
        internal static void SetUVs(GameObject obj, int index, List<Vector4> uv)
        {
            if (ProBuilderMeshSetUVMethodInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderMeshSetUV() failed to find an appropriate `SetUV` method on `ProBuilderMesh` type");
                return;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());

            ProBuilderMeshSetUVMethodInfo.Invoke(comp, new object[] {index, uv});
        }

        /// <summary>
        /// Call ProBuilder.ToMesh().
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        internal static void ToMesh(GameObject obj)
        {
            if (ProBuilderMeshToMeshMethodInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ToMesh() failed to find an appropriate `ToMesh` method on `ProBuilderMesh` type");
                return;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());

            ProBuilderMeshToMeshMethodInfo.Invoke(comp, new object[] {MeshTopology.Triangles});
        }

        /// <summary>
        /// Call ProBuilderMesh.Refresh().
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        /// <param name="mask"></param>
        internal static void Refresh(GameObject obj, RefreshMask mask = RefreshMask.All)
        {
            if (ProBuilderMeshRefreshMethodInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.Refresh() failed to find an appropriate `Refresh` method on `ProBuilderMesh` type");
                return;
            }

            obj.GetComponent(GetProBuilderMeshType());
            object comp = obj.GetComponent(GetProBuilderMeshType());

            ProBuilderMeshRefreshMethodInfo.Invoke(comp, new object[] {(int) mask});
        }

        /// <summary>
        /// Call ProBuilderMesh.Optimize().
        /// </summary>
        /// <param name="obj">GameObject with instance of ProBuilderMesh.</param>
        internal static void Optimize(GameObject obj)
        {
            if (ProBuilderMeshOptimizeMethodInfo == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.Optimize() failed to find an appropriate `Optimize` method on `ProBuilderMesh` type");
                return;
            }

            object comp = obj.GetComponent(GetProBuilderMeshType());
            ProBuilderMeshOptimizeMethodInfo.Invoke(null, new object[]{comp, false});
        }

        /// <summary>
        /// Subscribe to ProBuilderEditor.selectModeChanged event.
        /// </summary>
        /// <param name="listener"></param>
        internal static void SubscribeToSelectModeChanged(Action<int> listener)
        {
            if (ProBuilderOnSelectModeChanged == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderSubscribeToSelectModeChanged() failed to find an appropriate `selectModeChanged` event on `ProBuilderEditor` type");
                return;
            }

            Type tDelegate = ProBuilderOnSelectModeChanged.EventHandlerType;
            Delegate d = Delegate.CreateDelegate(tDelegate, PolybrushEditor.instance, PolybrushOnSelectModeListenerMethodInfo);

            MethodInfo addMethod = ProBuilderOnSelectModeChanged.GetAddMethod();
            addMethod.Invoke(null, new object[] { d });
        }

        /// <summary>
        /// Unsubscribe from ProBuilderEditor.selectModeChanged event.
        /// </summary>
        /// <param name="listener"></param>
        internal static void UnsubscribeToSelectModeChanged(Action<int> listener)
        {
            if (ProBuilderOnSelectModeChanged == null)
            {
                Debug.LogWarning(
                    "ProBuilderBridge.ProBuilderUnsubscribeToSelectModeChanged() failed to find an appropriate `selectModeChanged` event on `ProBuilderEditor` type");
                return;
            }

            Type tDelegate = ProBuilderOnSelectModeChanged.EventHandlerType;
            Delegate d = Delegate.CreateDelegate(tDelegate, PolybrushEditor.instance, PolybrushOnSelectModeListenerMethodInfo);

            MethodInfo removeMethod = ProBuilderOnSelectModeChanged.GetRemoveMethod();
            removeMethod.Invoke(null, new object[] { d });
        }

        /// <summary>
        /// Tests utility class for ProBuilderBridge class.
        /// Primarily used in Tests Runner.
        /// </summary>
        internal class TestsUtility
        {
            /// <summary>
            /// Validate that all MethodInfo fields in ProBuilderBridge are defined
            /// when ProBuilder is imported in the project.
            /// If one is not defined, most likely ProBuilder API changed.
            /// </summary>
            internal static void ValidateIntegration()
            {
                ValidatePropertyInfo(ProBuilderMeshPositionsPropertyInfo);
                ValidatePropertyInfo(ProBuilderMeshTangentsPropertyInfo);
                ValidatePropertyInfo(ProBuilderMeshColorsPropertyInfo);
                ValidatePropertyInfo(ProBuilderMeshVertexCountPropertyInfo);
                ValidatePropertyInfo(ProBuilderSelectModePropertyInfo);
                ValidateMethodInfo(ProBuilderRefreshMethodInfo);
                ValidateMethodInfo(ProBuilderMeshToMeshMethodInfo);
                ValidateMethodInfo(ProBuilderMeshRefreshMethodInfo);
                ValidateMethodInfo(ProBuilderMeshOptimizeMethodInfo);
                ValidateMethodInfo(ProBuilderMeshSetUVMethodInfo);
                ValidateEventInfo(ProBuilderOnSelectModeChanged);
            }

            /// <summary>
            /// Validate Polybrush has defined a proper event listener to use with ProBuilder.
            /// It ensures that we don't rename our listener without updating the ProBuilder integration.
            /// </summary>
            internal static void ValidatePolybrushListeners()
            {
                ValidateMethodInfo(PolybrushOnSelectModeListenerMethodInfo);
            }

            static void ValidateMethodInfo(MethodInfo method)
            {
                if (method == null)
                {
                    throw new NullReferenceException(string.Format("{0} not found.", method.Name));
                }
            }

            static void ValidateEventInfo(EventInfo evt)
            {
                if (evt == null)
                {
                    throw new NullReferenceException(string.Format("{0} not found.", evt.Name));
                }
            }

            static void ValidatePropertyInfo(PropertyInfo property)
            {
                if (property == null)
                {
                    throw new NullReferenceException(string.Format("{0} not found.", property.Name));
                }
            }
        }
    }
}
