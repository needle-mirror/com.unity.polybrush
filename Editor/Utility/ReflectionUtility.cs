using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace UnityEditor.Polybrush
{
    static class ReflectionUtility
    {	
        const BindingFlags k_AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Prints a warning
        /// </summary>
        /// <param name="text"></param>
        static void Warning(string text)
        {
            Debug.LogWarning(text);
        }

        /// <summary>
        /// Fetch a type with name and optional assembly name.  `type` should include namespace.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        internal static Type GetType(string type, string assembly = null)
        {
            Type t = Type.GetType(type);

            if(t == null)
            {
                IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

                if(assembly != null)
                    assemblies = assemblies.Where(x => x.FullName.Contains(assembly));

                foreach(Assembly ass in assemblies)
                {
                    t = ass.GetType(type);

                    if(t != null)
                        return t;
                }
            }

            return t;
        }

        /// <summary>
        /// Fetch a value using GetProperty or GetField.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        internal static object GetValue(object target, string type, string member, BindingFlags flags = k_AllFlags)
        {
            Type t = GetType(type);

            if(t == null)
            {
                Warning(string.Format("Could not find type \"{0}\"!", type));
                return null;
            }
            else
                return GetValue(target, t, member, flags);
        }

        internal static object GetValue(object target, Type type, string member, BindingFlags flags = k_AllFlags)
        {
            PropertyInfo pi = type.GetProperty(member, flags);

            if(pi != null)
                return pi.GetValue(target, null);

            FieldInfo fi = type.GetField(member, flags);

            if(fi != null)
                return fi.GetValue(target);

            Warning(string.Format("Could not find member \"{0}\" matching type {1}!", member, type));

            return null;
        }
    }
}