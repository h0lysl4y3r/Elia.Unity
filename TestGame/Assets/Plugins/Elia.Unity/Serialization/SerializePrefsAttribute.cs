using System;

namespace Elia.Unity.Serialization
{
    /// <summary>
    /// Attribute used to decorate class members with key and default value for use with <see cref="Modules.PrefsStorage"/> module.
    /// </summary>
	public class SerializePrefsAttribute : Attribute
	{
        /// <summary>
        /// Key for class member to <see cref="Modules.PrefsStorage"/> storage module.
        /// </summary>
		public string Key { get; private set; }

        /// <summary>
        /// Defualt value for class member to <see cref="Modules.PrefsStorage"/> storage module.
        /// </summary>
		public object DefaultValue { get; private set; }

        #region Constructor

        public SerializePrefsAttribute() : this(null, null) { }
		public SerializePrefsAttribute(string key) : this(key, null) { }
		public SerializePrefsAttribute(string key, object defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}

        #endregion
    }
}
