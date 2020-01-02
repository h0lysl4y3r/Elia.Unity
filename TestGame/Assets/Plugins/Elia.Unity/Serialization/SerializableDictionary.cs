// Author: Fredrik Ludvigsen (http://wiki.unity3d.com/index.php?title=SerializableDictionary&oldid=19559)
using System.Collections.Generic;
using UnityEngine;

namespace Elia.Unity.Serialization
{
    abstract public class SerializableDictionary<K, V> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private K[] _keys;
        [SerializeField]
        private V[] _values;

        public Dictionary<K, V> Dictionary;

        static public T New<T>() where T : SerializableDictionary<K, V>, new()
        {
            var result = new T();
            result.Dictionary = new Dictionary<K, V>();
            return result;
        }

        public void OnAfterDeserialize()
        {
            var c = _keys.Length;
            Dictionary = new Dictionary<K, V>(c);
            for (int i = 0; i < c; i++)
            {
                Dictionary[_keys[i]] = _values[i];
            }
            _keys = null;
            _values = null;
        }

        public void OnBeforeSerialize()
        {
            var c = Dictionary.Count;
            _keys = new K[c];
            _values = new V[c];
            int i = 0;
            using (var e = Dictionary.GetEnumerator())
                while (e.MoveNext())
                {
                    var kvp = e.Current;
                    _keys[i] = kvp.Key;
                    _values[i] = kvp.Value;
                    i++;
                }
        }
    }
}
