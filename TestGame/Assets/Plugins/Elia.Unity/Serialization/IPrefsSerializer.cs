using System;
using UnityEngine;

namespace Elia.Unity.Serialization
{
    /// <summary>
    /// Interface for Unity <see cref="PlayerPrefs"/> serialization within <see cref="Modules.PrefsStorage"/> module.
    /// </summary>
	public interface IPrefsSerializer
	{
		string Serialize(object value);
		object Deserialize(Type type, string text, object defaultValue);
	}
}
