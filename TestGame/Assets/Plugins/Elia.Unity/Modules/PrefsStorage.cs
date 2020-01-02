using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Elia.Unity.Helpers;
using Elia.Unity.Serialization;
using Elia.Unity.Extensions;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Unity Prefs storage management module.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/PrefsStorage")]
	public sealed class PrefsStorage : BehaviourAwareSingleton<PrefsStorage>
	{
		#region Actions

		/// <summary>
		/// Invoked on <see cref="Initialize"/> called.
		/// </summary>
		public Action Initialized;

		#endregion

		#region Members

		/// <summary>
		/// Secret to create crypto secret key hash 
		/// </summary>
		public static string SecretWord = "SecretWord";

        /// <summary>
        /// Key to store list of keys with
        /// </summary>
		private const string AllKeysKey = "PrefsStorage.AllKeys";

		private IPrefsSerializer _serializer;
		private List<Type> _serializedTypes = new List<Type>();
		private List<string> _keys = new List<string>();
		private bool _useCrypto;
		private static byte[] SecretKey = null;
		private TripleDESCryptoServiceProvider _cryptoProvider;
		private ICryptoTransform _encryptor;
		private ICryptoTransform _decryptor;
		private bool _initialized;
		private string _appId;

        #endregion

        #region Init

        /// <summary>
        /// Initializes storage from <paramref name="serializedTypes"/> or traversing types with custom attribute <see cref="SerializePrefsAttribute"/> in <paramref name="serializedTypesAssemblies"/>.
        /// </summary>
        /// <param name="appId">Application id</param>
        /// <param name="serializer">Serializer of <see cref="IPrefsSerializer"/> type</param>
        /// <param name="useCrypto">True to encrypt storage data</param>
        /// <param name="serializedTypes">Array of types that should be saved</param>
        /// <param name="serializedTypesAssemblies">Array of assemblies that should be looked up for types to save</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serializer"/> is null</exception>
        public void Initialize(string appId, IPrefsSerializer serializer, bool useCrypto, Type[] serializedTypes = null, Assembly[] serializedTypesAssemblies = null)
		{
			if (serializedTypes != null) _serializedTypes.AddRange(serializedTypes);
			if (!serializedTypesAssemblies.IsNullOrEmpty())
			{
				foreach (var assembly in serializedTypesAssemblies)
				{
					foreach (var type in assembly.GetExportedTypes())
					{
						if (type.GetCustomAttributes(typeof(SerializePrefsAttribute), true).Length > 0)
							_serializedTypes.Add(type);
					}
				}
			}

			_appId = appId;
			_serializer = serializer;
			if (_serializer == null) throw new ArgumentNullException(nameof(serializer));

			_useCrypto = useCrypto;
			if (_useCrypto) InitializeCrypto();

			var localKeys = LoadKeys();
			if (localKeys != null) _keys = localKeys;

			LoadAttributesWithTypes(GetSerializedTypes().ToList());

			_initialized = true;
			Initialized?.Invoke();
		}

        /// <summary>
        /// Returns types that are saved.
        /// </summary>
        /// <returns>Array of types that are saved</returns>
		public Type[] GetSerializedTypes()
		{
			return _serializedTypes.ToArray();
		}

        /// <summary>
        /// Traverses objects of <paramref name="classTypes"/> for <paramref name="propertyMemberTypes"/> and <paramref name="fieldMemberTypes"/> to save them.
        /// </summary>
        /// <param name="classTypes">List of class types</param>
        /// <param name="propertyMemberTypes">List of explicit property types (within <paramref name="classTypes"/>) to save</param>
        /// <param name="fieldMemberTypes">>List of explicit field types (within <paramref name="classTypes"/>) to save</param>
		private void SaveAttributesWithTypes(List<Type> classTypes, List<Type> propertyMemberTypes = null, List<Type> fieldMemberTypes = null)
		{
			foreach (var t in classTypes)
			{
				ProcessAllPropertiesOfType(t, propertyMemberTypes, (Type type, SerializePrefsAttribute attribute, PropertyInfo info, UnityEngine.Object obj) =>
				{
					var value = info.GetValue(obj, null);
					var key = (attribute.Key != null) ? attribute.Key : CreatePropertyKey(info);
					value = (value != null) ? value : attribute.DefaultValue ?? CreateInstanceSafe(type);
					SetValue(key, value);
				});

				ProcessAllFieldsOfType(t, fieldMemberTypes, (Type type, SerializePrefsAttribute attribute, FieldInfo info, UnityEngine.Object obj) =>
				{
					var value = info.GetValue(obj);
					var key = (attribute.Key != null) ? attribute.Key : CreateFieldKey(info);
					value = (value != null) ? value : attribute.DefaultValue ?? CreateInstanceSafe(type);
					SetValue(key, value);
				});
			}
		}

        /// <summary>
        /// Traverses objects of <paramref name="classTypes"/> for <paramref name="propertyMemberTypes"/> and <paramref name="fieldMemberTypes"/> to load them.
        /// </summary>
        /// <param name="classTypes">List of class types</param>
        /// <param name="propertyMemberTypes">List of explicit property types (within <paramref name="classTypes"/>) to load</param>
        /// <param name="fieldMemberTypes">>List of explicit field types (within <paramref name="classTypes"/>) to load</param>
		private void LoadAttributesWithTypes(List<Type> classTypes, List<Type> propertyMemberTypes = null, List<Type> fieldMemberTypes = null)
		{
			foreach (var t in classTypes)
			{
				ProcessAllPropertiesOfType(t, propertyMemberTypes, (Type type, SerializePrefsAttribute attribute, PropertyInfo info, UnityEngine.Object obj) =>
				{
					var key = (attribute.Key != null) ? attribute.Key : CreatePropertyKey(info);
					var value = GetValue(type, key, attribute.DefaultValue);
					if (value == null)
					{
						// if value type set default value, otherwise create new instance of reference type
						value = (type.IsValueType) ? attribute.DefaultValue : CreateInstanceSafe(type);
					}
					info.SetValue(obj, value, null);
				});

				ProcessAllFieldsOfType(t, fieldMemberTypes, (Type type, SerializePrefsAttribute attribute, FieldInfo info, UnityEngine.Object obj) =>
				{
					var key = (attribute.Key != null) ? attribute.Key : CreateFieldKey(info);

					var value = GetValue(type, key, attribute.DefaultValue);

					if (value == null)
					{
						// if value type set default value, otherwise create new instance of reference type
						value = (type.IsValueType) ? attribute.DefaultValue : CreateInstanceSafe(type);
					}
					info.SetValue(obj, value);
				});
			}
		}

        /// <summary>
        /// For given <paramref name="type"/> and property <paramref name="memberTypes"/> invokes given <paramref name="action"/>.
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="memberTypes">Object type property types</param>
        /// <param name="action">Action to invoke</param>
        /// <returns>True if successed</returns>
		private bool ProcessAllPropertiesOfType(Type type, List<Type> memberTypes, Action<Type, SerializePrefsAttribute, PropertyInfo, UnityEngine.Object> action)
		{
			if (!type.IsSubclassOf(typeof(UnityEngine.Object))) return false;

			var obj = GameObject.FindObjectOfType(type);
			if (obj == null) return true;

			foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (memberTypes != null && !memberTypes.Contains(propertyInfo.PropertyType)) continue;

				var attributes = propertyInfo.GetCustomAttributes(typeof(SerializePrefsAttribute), false);
				if (attributes.Length <= 0) continue;

				SerializePrefsAttribute attribute = attributes[0] as SerializePrefsAttribute;
				action.Invoke(propertyInfo.PropertyType, attribute, propertyInfo, obj);
			}

			return true;
		}

        /// <summary>
        /// For given <paramref name="type"/> and field <paramref name="memberTypes"/> invokes given <paramref name="action"/>.
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="memberTypes">Object type field types</param>
        /// <param name="action">Action to invoke</param>
        /// <returns>True if successed</returns>
		private bool ProcessAllFieldsOfType(Type type, List<Type> memberTypes, Action<Type, SerializePrefsAttribute, FieldInfo, UnityEngine.Object> action)
		{
			if (!type.IsSubclassOf(typeof(UnityEngine.Object))) return false;

			var obj = GameObject.FindObjectOfType(type);
			if (obj == null) return true;
			foreach (var fieldInfo in type.GetFields())
			{
				if (memberTypes != null && !memberTypes.Contains(fieldInfo.FieldType)) continue;

				var attributes = fieldInfo.GetCustomAttributes(typeof(SerializePrefsAttribute), false);
				if (attributes.Length <= 0) continue;

				SerializePrefsAttribute attribute = attributes[0] as SerializePrefsAttribute;
				action.Invoke(fieldInfo.FieldType, attribute, fieldInfo, obj);
			}

			return true;
		}

        /// <summary>
        /// Creates storage key from <see cref="PropertyInfo"/> instance.
        /// </summary>
        /// <param name="info"><see cref="PropertyInfo"/> instance</param>
        /// <returns>Storage key</returns>
		private string CreatePropertyKey(PropertyInfo info)
		{
			var appKey = (_appId != null) ? string.Concat(_appId, ".") : "";
			return string.Concat(appKey, string.Format("{0}.{1}", info.PropertyType.FullName, info.Name));
		}

        /// <summary>
        /// Creates storage key from <see cref="FieldInfo"/> instance.
        /// </summary>
        /// <param name="info"><see cref="FieldInfo"/> instance</param>
        /// <returns>Storage key</returns>
		private string CreateFieldKey(FieldInfo info)
		{
			var appKey = (_appId != null) ? string.Concat(_appId, ".") : "";
			return string.Concat(appKey, string.Format("{0}.{1}", info.FieldType.FullName, info.Name));
		}

        #endregion

        #region Set Methods

        /// <summary>
        /// Sets storage at <paramref name="key"/> with <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">Value type param</typeparam>
        /// <param name="key">Storage key</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if succeeded</returns>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
        public bool SetValue<T>(string key, T value)
		{
			if (!_initialized) throw new NotInitializedException();
			return SetValueInner(key, value);
		}

        /// <summary>
        /// Sets storage at <paramref name="key"/> with <paramref name="value"/>.
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if succeeded</returns>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
		public bool SetValue(string key, object value)
		{
			if (!_initialized) throw new NotInitializedException();
			return SetValueInner(key, value);
		}

        /// <summary>
        /// Sets storage at <paramref name="key"/> with <paramref name="value"/>. If crypto was initialized, encrypted value is stored.
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if succeeded</returns>
		private bool SetValueInner(string key, object value)
		{
			var text = _serializer.Serialize(value);
			var result = EncryptValue(text);
            if (result == null) return false;

			SetLocalEncryptedValue(key, result);
            return true;
        }

        /// <summary>
        /// Sets storage at <paramref name="key"/> with <paramref name="value"/> (with encrypted value if available).
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="value">Value to set</param>
		private void SetLocalEncryptedValue(string key, string value)
		{
			if (!_keys.Contains(key)) _keys.Add(key);
			PlayerPrefs.SetString(key, value);
		}

        #endregion

        #region Get Methods

        /// <summary>
        /// Returns storage value at <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">Value's type param</typeparam>
        /// <param name="key">Storage key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Storage value (decrypted if available) or default value in case of exception or null value was found</returns>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
        public T GetValue<T>(string key, T defaultValue = default(T))
		{
			if (!_initialized) throw new NotInitializedException();
			return (T)GetValueInner(typeof(T), key, defaultValue);
		}

        /// <summary>
        /// Returns storage value at <paramref name="key"/>.
        /// </summary>
        /// <param name="type">Value's type</param>
        /// <param name="key">Storage key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Storage value (decrypted if available) or default value in case of exception or null value was found</returns>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
		public object GetValue(Type type, string key, object defaultValue)
		{
			if (!_initialized) throw new NotInitializedException();
			return GetValueInner(type, key, defaultValue);
		}

        /// <summary>
        /// Returns storage value at <paramref name="key"/>.
        /// </summary>
        /// <param name="type">Value's type</param>
        /// <param name="key">Storage key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Storage value (decrypted if available) or default value in case of exception or null value was found</returns>
		private object GetValueInner(Type type, string key, object defaultValue)
		{
			var encryptedValue = GetLocalEncryptedValue(key);
			if (encryptedValue == null) return defaultValue;
			else if (encryptedValue == string.Empty) return defaultValue;

			var value = DecryptValue(encryptedValue);
            if (value == null) return defaultValue;

			try
			{
				return _serializer.Deserialize(type, value, defaultValue);
			}
			catch (Exception ex)
			{
				App.Instance?.Logger.LogException(ex);
				return defaultValue;
			}
		}

        /// <summary>
        /// Returns storage value at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <returns>Storage value or null if key was not found</returns>
		private string GetLocalEncryptedValue(string key)
		{
			return PlayerPrefs.GetString(key, null);
		}

        /// <summary>
        /// Returns storage key prefixed with <see cref="App"/> id and <typeparamref name="T"/> type name.
        /// </summary>
        /// <typeparam name="T">Type param</typeparam>
        /// <param name="key">Storage key</param>
        /// <returns>Storage key prefixed with <typeparamref name="T"/> type name</returns>
		public string GetTypePrefixedKey<T>(string key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			var appKey = (_appId != null) ? string.Concat(_appId, ".") : "";
			return string.Concat(appKey, typeof(T).Name, ".", key);
		}

		/// <summary>
		/// Returns storage key prefixed with <see cref="App"/> id and <paramref name="obj"/> type name.
		/// </summary>
		/// <param name="key">Storage key</param>
		/// <param name="obj">Instance to get type from</param>
		/// <returns>Storage key prefixed with <paramref name="obj"/> type name</returns>
		public string GetTypePrefixedKey(string key, object obj)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (obj == null) throw new ArgumentNullException(nameof(obj));

			var appKey = (_appId != null) ? string.Concat(_appId, ".") : "";
			return string.Concat(appKey, obj.GetType().Name, ".", key);
		}

        #endregion

        #region Crypto

        /// <summary>
        /// Initializes <see cref="TripleDESCryptoServiceProvider"/>.
        /// </summary>
        private void InitializeCrypto()
		{
			using (var md5 = MD5.Create())
			{
				SecretKey = md5.ComputeHash(UTF8Encoding.ASCII.GetBytes(SecretWord));
			}
			_cryptoProvider = new TripleDESCryptoServiceProvider();
			_cryptoProvider.Key = SecretKey;
			_cryptoProvider.IV = new byte[] { 0xf, 0x6f, 0x13, 0x2e, 0x35, 0xc2, 0xcd, 0xf9 };
			_cryptoProvider.Mode = CipherMode.ECB;
			_cryptoProvider.Padding = PaddingMode.PKCS7;
			_encryptor = _cryptoProvider.CreateEncryptor();
			_decryptor = _cryptoProvider.CreateDecryptor();
		}

        /// <summary>
        /// Encrypts <paramref name="plainValue"/> via <see cref="TripleDESCryptoServiceProvider"/>.
        /// If <see cref="PrefsStorage"/> was not initialized with encryption, <paramref name="plainValue"/> is returned.
        /// </summary>
        /// <param name="plainValue">Value to encrypt</param>
        /// <returns>Encrypted value or null if operation failed</returns>
		private string EncryptValue(string plainValue)
		{
			if (plainValue == null) return null;
			if (!_useCrypto) return plainValue;

			var plainData = Encoding.UTF8.GetBytes(plainValue);
			byte[] cryptoData = null;
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, _encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainData, 0, plainData.Length);
                        cryptoStream.FlushFinalBlock();
                        cryptoData = memoryStream.ToArray();
                    }
                }
                return Convert.ToBase64String(cryptoData, 0, cryptoData.GetLength(0));
            }
            catch (Exception ex)
            {
				App.Instance?.Logger.LogException(ex);
				return null;
            }
		}

        /// <summary>
        /// Decrypts <paramref name="cryptoValue"/> via <see cref="TripleDESCryptoServiceProvider"/>.
        /// If <see cref="PrefsStorage"/> was not initialized with encryption, <paramref name="cryptoValue"/> is returned.
        /// </summary>
        /// <param name="cryptoValue">Value to decrypt</param>
        /// <returns>Decrypted value or null if operation failed</returns>
		private string DecryptValue(string cryptoValue)
		{
			if (cryptoValue == null) return null;
			if (!_useCrypto) return cryptoValue;

			try
			{
				var cryptoData = Convert.FromBase64String(cryptoValue);
				using (var memoryStream = new MemoryStream(cryptoData, 0, cryptoData.Length))
				{
					using (var cryptoStream = new CryptoStream(memoryStream, _decryptor, CryptoStreamMode.Read))
					{
						using (var streamReader = new StreamReader(cryptoStream))
						{
							return streamReader.ReadToEnd();
						}
					}
				}
			}
			catch (Exception ex)
			{
				App.Instance?.Logger.LogException(ex);
				return null; 
			}
		}

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes and saves values to <see cref="PlayerPrefs"/>.
        /// </summary>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
        public void Save()
		{
			if (!_initialized) throw new NotInitializedException();

			SaveAttributesWithTypes(_serializedTypes);
			SetValue(AllKeysKey, _keys);
			PlayerPrefs.Save();
		}

        /// <summary>
        /// Deletes all values from <see cref="PlayerPrefs"/> and removes all keys stored.
        /// </summary>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
		public void DeleteAll()
		{
			if (!_initialized) throw new NotInitializedException();

			foreach (var key in _keys)
			{
				PlayerPrefs.DeleteKey(key);
			}
			_keys.Clear();
		}

        /// <summary>
        /// Returns all keys stored.
        /// </summary>
        /// <returns>All keys stored</returns>
        /// <exception cref="NotInitializedException">Thrown if <see cref="PrefsStorage"/> was not initialized</exception>
		public string[] GetAllKeys()
		{
			if (!_initialized) throw new NotInitializedException();
			return _keys.ToArray();
		}

        /// <summary>
        /// Loades all storage keys stored at <see cref="AllKeysKey"/> key.
        /// </summary>
        /// <returns>List of storage keys</returns>
		private List<string> LoadKeys()
		{
			return GetValueInner(typeof(List<string>), AllKeysKey, null) as List<string>;
		}

        /// <summary>
        /// Creates instance of <paramref name="type"/> or in case <paramref name="type"/> is <see cref="String"/> type, returns null.
        /// </summary>
        /// <param name="type">Instance type</param>
        /// <returns>Instance of <paramref name="type"/> or in case <paramref name="type"/> is <see cref="String"/> type, returns null</returns>
		private object CreateInstanceSafe(Type type)
		{
			return type == typeof(string) ? (string)null : Activator.CreateInstance(type);
		}

		#endregion
	}
}
