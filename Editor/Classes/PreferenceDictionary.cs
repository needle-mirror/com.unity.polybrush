#if UNITY_EDITOR

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Polybrush;

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Store Polybrush settings in a project-local manner.
    /// </summary>
    internal class PreferenceDictionary : ScriptableObject, ISerializationCallbackReceiver, IHasDefault, ICustomSettings
	{
		Dictionary<string, bool> 		m_bool 		= new Dictionary<string, bool>();
		Dictionary<string, int> 		m_int 		= new Dictionary<string, int>();
		Dictionary<string, float> 		m_float 	= new Dictionary<string, float>();
		Dictionary<string, string> 		m_string 	= new Dictionary<string, string>();
		Dictionary<string, Color> 		m_Color 	= new Dictionary<string, Color>();

		[SerializeField] List<string> 	m_bool_keys;
		[SerializeField] List<string>	m_int_keys;
		[SerializeField] List<string>	m_float_keys;
		[SerializeField] List<string>	m_string_keys;
		[SerializeField] List<string>	m_Color_keys;

		[SerializeField] List<bool> 	m_bool_values;
		[SerializeField] List<int> 		m_int_values;
		[SerializeField] List<float> 	m_float_values;
		[SerializeField] List<string> 	m_string_values;
		[SerializeField] List<Color> 	m_Color_values;

        public string assetsFolder { get { return ""; } }

        /// <summary>
        /// Perform the ritual "Please Serialize My Dictionary" dance.
        /// </summary>
        public void OnBeforeSerialize()
		{
			m_bool_keys 		= m_bool.Keys.ToList();
			m_int_keys 			= m_int.Keys.ToList();
			m_float_keys 		= m_float.Keys.ToList();
			m_string_keys 		= m_string.Keys.ToList();
			m_Color_keys 		= m_Color.Keys.ToList();

			m_bool_values 		= m_bool.Values.ToList();
			m_int_values 		= m_int.Values.ToList();
			m_float_values 		= m_float.Values.ToList();
			m_string_values 	= m_string.Values.ToList();
			m_Color_values 		= m_Color.Values.ToList();
		}

        /// <summary>
        /// Reconstruct preference dictionaries from serialized lists.
        /// </summary>
        public void OnAfterDeserialize()
		{
			for(int i = 0; i < m_bool_keys.Count; i++)
				m_bool.Add(m_bool_keys[i], m_bool_values[i]);

			for(int i = 0; i < m_int_keys.Count; i++)
				m_int.Add(m_int_keys[i], m_int_values[i]);

			for(int i = 0; i < m_float_keys.Count; i++)
				m_float.Add(m_float_keys[i], m_float_values[i]);

			for(int i = 0; i < m_string_keys.Count; i++)
				m_string.Add(m_string_keys[i], m_string_values[i]);

			for(int i = 0; i < m_Color_keys.Count; i++)
				m_Color.Add(m_Color_keys[i], m_Color_values[i]);
		}

        /// <summary>
        /// Clear dictionary values.
        /// </summary>
        public void SetDefaultValues()
		{
			m_bool.Clear();
			m_int.Clear();
			m_float.Clear();
			m_string.Clear();
			m_Color.Clear();

			EditorUtility.SetDirty(this);
		}

        /// <summary>
        /// Check if a key is contained within any type dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool HasKey(string key)
		{
			return 	m_bool.ContainsKey(key) ||
					m_int.ContainsKey(key) ||
					m_float.ContainsKey(key) ||
					m_string.ContainsKey(key) ||
					m_Color.ContainsKey(key);
		}

        /// <summary>
        /// Fetch a value from the stored preferences.  If key is not found, a default value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        internal bool GetBool(string key, bool fallback = default(bool))
		{
			bool value;
			if(m_bool.TryGetValue(key, out value))
				return value;
			return fallback;
		}

        /// <summary>
        /// Fetch a value from the stored preferences.  If key is not found, a default value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        internal int GetInt(string key, int fallback = default(int))
		{
			int value;
			if(m_int.TryGetValue(key, out value))
				return value;
			return fallback;
		}

        /// <summary>
        /// Fetch a value from the stored preferences.  If key is not found, a default value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        internal float GetFloat(string key, float fallback = default(float))
		{
			float value;
			if(m_float.TryGetValue(key, out value))
				return value;
			return fallback;
		}

        /// <summary>
        /// Fetch a value from the stored preferences.  If key is not found, a default value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        internal string GetString(string key, string fallback = default(string))
		{
			string value;
			if(m_string.TryGetValue(key, out value))
				return value;
			return fallback;
		}

        /// <summary>
        /// Fetch a value from the stored preferences.  If key is not found, a default value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        internal Color GetColor(string key, Color fallback = default(Color))
		{
			Color value;
			if(m_Color.TryGetValue(key, out value))
				return value;
			return fallback;
		}

        /// <summary>
        /// Set a value for key in the saved prefs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SetBool(string key, bool value)
		{
			if(m_bool.ContainsKey(key))
				m_bool[key] = value;
			else
				m_bool.Add(key, value);

			EditorUtility.SetDirty(this);
		}

        /// <summary>
        /// Set a value for key in the saved prefs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SetInt(string key, int value)
		{
			if(m_int.ContainsKey(key))
				m_int[key] = value;
			else
				m_int.Add(key, value);

			EditorUtility.SetDirty(this);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		internal void SetFloat(string key, float value)
		{
			if(m_float.ContainsKey(key))
				m_float[key] = value;
			else
				m_float.Add(key, value);

			EditorUtility.SetDirty(this);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		internal void SetString(string key, string value)
		{
			if(m_string.ContainsKey(key))
				m_string[key] = value;
			else
				m_string.Add(key, value);

			EditorUtility.SetDirty(this);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		internal void SetColor(string key, Color value)
		{
			if(m_Color.ContainsKey(key))
				m_Color[key] = value;
			else
				m_Color.Add(key, value);

			EditorUtility.SetDirty(this);
		}
        /// <summary>
        /// Removes key and its corresponding value from the preferences. 
        /// </summary>
        /// <param name="key">Key to delete</param>
        internal void DeleteKey(string key)
        {
            if(m_bool.ContainsKey(key))
            {
                m_bool.Remove(key);
                return;
            }

            if (m_int.ContainsKey(key))
            {
                m_int.Remove(key);
                return;
            }

            if (m_float.ContainsKey(key))
            {
                m_float.Remove(key);
                return;
            }

            if (m_string.ContainsKey(key))
            {
                m_string.Remove(key);
                return;
            }

            if (m_Color.ContainsKey(key))
            {
                m_Color.Remove(key);
                return;
            }
        }
    }
}

#endif
