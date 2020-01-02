using System;
using Newtonsoft.Json;
using Elia.Unity.Serialization;

public class JsonPrefsSerializer : IPrefsSerializer
{
	public string Serialize(object value)
	{
		return JsonConvert.SerializeObject(value);
	}

	public object Deserialize(Type type, string text, object defaultValue)
	{
		return JsonConvert.DeserializeObject(text, type) ?? defaultValue;
	}
}
