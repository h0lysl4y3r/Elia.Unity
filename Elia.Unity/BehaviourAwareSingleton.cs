using System;
using UnityEngine;

namespace Elia.Unity
{
    /// <summary>
    /// <see cref="BehaviourAware"/> singleton created on <see cref="MonoBehaviour"/> <see cref="Awake"/> is called.
    /// Can be accessed via <see cref="Instance"/> property. First time calls <see cref="OnSingletonInstantiating"/> method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public abstract class BehaviourAwareSingleton<T> : BehaviourAware
		where T : MonoBehaviour
	{
        /// <summary>
        /// Singleton instance created on <see cref="MonoBehaviour"/> <see cref="Awake"/> is called.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if current instance is null and there exists another instance of <see cref="T"/> type.</exception>
		public static T Instance
		{
			get
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						var instanceGameObjects = FindObjectsOfType<T>();
						if (instanceGameObjects.Length == 0) return null;
						if (instanceGameObjects.Length > 1) throw new ArgumentException(Texts.Errors.TooManyObjectsOfGivenTypeFound, nameof(instanceGameObjects));

						_instance = instanceGameObjects[0].GetComponent<T>();

						var behaviourAwareInstance = _instance as BehaviourAwareSingleton<T>;
						if (behaviourAwareInstance != null && !behaviourAwareInstance.IsInstantiated)
						{
							behaviourAwareInstance.OnSingletonInstantiating();
							behaviourAwareInstance.IsInstantiated = true;
						}
					}
					return _instance;
				}
			}
		}

        /// <summary>
        /// True if singleton instance was created.
        /// </summary>
		protected bool IsInstantiated { get; private set; }
		private static T _instance;
		private static readonly object _lock = new object();

        /// <summary>
        /// Sub-classes can override this method to call initialization logic.
        /// </summary>
		public virtual void OnSingletonInstantiating() { }

		protected override void Awake()
		{
			base.Awake();

			var instance = Instance;
		}
	}
}
