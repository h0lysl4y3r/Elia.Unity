using System;
using UnityEngine;

namespace Elia.Unity.Serialization
{
    [Serializable]
    public class StringAudioClipDictionary : SerializableDictionary<string, AudioClip> { }

    [Serializable]
    public class StringStringDictionary : SerializableDictionary<string, string> { }

	[Serializable]
	public class StringObjectDictionary : SerializableDictionary<string, UnityEngine.Object> { }
}
