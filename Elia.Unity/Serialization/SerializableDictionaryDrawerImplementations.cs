using UnityEngine;

namespace Elia.Unity.Serialization
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(StringAudioClipDictionary))]
    public class StringAudioClipDictionaryDrawer : SerializableDictionaryDrawer<string, AudioClip>
    {
        protected override SerializableKeyValueTemplate<string, AudioClip> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringAudioClipTemplate>();
        }
    }
    internal class SerializableStringAudioClipTemplate : SerializableKeyValueTemplate<string, AudioClip> { }


    [UnityEditor.CustomPropertyDrawer(typeof(StringStringDictionary))]
    public class StringStringDictionaryDrawer : SerializableDictionaryDrawer<string, string>
    {
        protected override SerializableKeyValueTemplate<string, string> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringStringTemplate>();
        }
    }
    internal class SerializableStringStringTemplate : SerializableKeyValueTemplate<string, string> { }

	[UnityEditor.CustomPropertyDrawer(typeof(StringObjectDictionary))]
	public class StringObjectDictionaryDrawer : SerializableDictionaryDrawer<string, UnityEngine.Object>
	{
		protected override SerializableKeyValueTemplate<string, UnityEngine.Object> GetTemplate()
		{
			return GetGenericTemplate<SerializableStringObjectTemplate>();
		}
	}
	internal class SerializableStringObjectTemplate : SerializableKeyValueTemplate<string, UnityEngine.Object> { }
#endif
}
