using Elia.Unity.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elia.Unity.Components.GameObjects
{
	/// <summary>
	/// Pool of instances of <seealso cref="UnityEngine.Object"/>. If exhausted creates a new instance of prefab given by <see cref="GameObjectPool.PrefabMap"/>.
	/// </summary>
	public class GameObjectPool : BehaviourAwareSingleton<GameObjectPool>
	{
		/// <summary>
		/// Map of keys and prefabs to create pool <see cref="UnityEngine.Object"/> instances
		/// </summary>
		[SerializeField]
		public StringObjectDictionary PrefabMap = StringObjectDictionary.New<StringObjectDictionary>();

		private Dictionary<string, List<UnityEngine.Object>> _objectMap;

		protected override void Awake()
		{
			base.Awake();

			_objectMap = new Dictionary<string, List<UnityEngine.Object>>();
		}

        /// <summary>
        /// Creates <paramref name="count"/> of instances of <see cref="GameObject"/> by <paramref name="prefabKey"/> and stores it to pool.
        /// </summary>
        /// <param name="prefabKey">Key to <see cref="PrefabMap"/></param>
        /// <param name="count">Number of instances to create</param>
        public void Initialize(string prefabKey, int count)
        {
            if (prefabKey == null) throw new ArgumentNullException(nameof(prefabKey));
            if (count <= 0) throw new ArgumentException(nameof(count));
            if (!PrefabMap.Dictionary.ContainsKey(prefabKey)) throw new Exception(string.Format(Texts.Errors.ObjectNotFound, nameof(prefabKey)));

            for (var i = 0; i < count; i++)
            {
                var go = GameObject.Instantiate(PrefabMap.Dictionary[prefabKey]) as GameObject;
                Set(prefabKey, go);
            }
        }

        /// <summary>
        /// Returns instance of <see cref="UnityEngine.Object"/> given by <paramref name="prefabKey"/> to <see cref="PrefabMap"/>.
        /// </summary>
        /// <param name="prefabKey">Key to <see cref="PrefabMap"/></param>
        /// <returns>Instance of <see cref="UnityEngine.Object"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="prefabKey"/> is null</exception>
        /// <exception cref="Exception">Thrown if no object under <paramref name="prefabKey"/> at <see cref="PrefabMap"/></exception>
        public UnityEngine.Object Get(string prefabKey)
		{
			if (prefabKey == null) throw new ArgumentNullException(nameof(prefabKey));
			if (!PrefabMap.Dictionary.ContainsKey(prefabKey)) throw new Exception(string.Format(Texts.Errors.ObjectNotFound, nameof(prefabKey)));

			if (!_objectMap.ContainsKey(prefabKey)) _objectMap[prefabKey] = new List<UnityEngine.Object>();
			var list = _objectMap[prefabKey];
			UnityEngine.Object result = null;
			while (true)
			{
				if (list.Count == 0) break;

				result = list[0];
				if (result == null) // Unity overrides the operator, so if the object is destroyed it equals null
				{
					result = null;
					list.RemoveAt(0);
					continue;
				}
				break;
			}

			if (result != null)
			{
				list.RemoveAt(0);
				return result;
			}

			result = GameObject.Instantiate(PrefabMap.Dictionary[prefabKey]);
			var go = result as GameObject;
            go.SetActive(false);
            go.transform.SetParent(gameObject.transform);
			return result;
		}

		/// <summary>
		/// Sets instance of <see cref="UnityEngine.Object"/> to pool.
		/// </summary>
		/// <param name="prefabKey">Key to <see cref="PrefabMap"/></param>
		/// <param name="obj">Instance of <see cref="UnityEngine.Object"/></param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="prefabKey"/> or <paramref name="obj"/> is null</exception>
		/// <exception cref="Exception">Thrown if no object under <paramref name="prefabKey"/> at <see cref="PrefabMap"/></exception>
		public void Set(string prefabKey, GameObject obj)
		{
			if (prefabKey == null) throw new ArgumentNullException(nameof(prefabKey));
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (!PrefabMap.Dictionary.ContainsKey(prefabKey)) throw new Exception(string.Format(Texts.Errors.ObjectNotFound, nameof(prefabKey)));

			if (!_objectMap.ContainsKey(prefabKey)) _objectMap[prefabKey] = new List<UnityEngine.Object>();
			_objectMap[prefabKey].Add(obj);

            obj.SetActive(false);
            obj.transform.SetParent(gameObject.transform);
        }
    }
}
