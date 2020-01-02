using System;
using UnityEngine;

namespace Elia.Unity
{
    /// <summary>
    /// Class that provides actions (and properties) on method invocation within Unity <see cref="ScriptableObject"/> life cycle.
    /// </summary>
	public class ScriptableObjectAware : ScriptableObject
	{
        #region Actions

        /// <summary>
        /// Invoked on <see cref="ScriptableObject"/> <see cref="Awake"/> is called.
        /// </summary>
        public Action Awaken;

        /// <summary>
        /// Invoked on <see cref="ScriptableObject"/> <see cref="Start"/> is called.
        /// </summary>
        public Action Started;

        /// <summary>
        /// Invoked on <see cref="ScriptableObject"/> <see cref="OnEnable"/> is called.
        /// </summary>
        public Action Enabled;

        /// <summary>
        /// Invoked on <see cref="ScriptableObject"/> <see cref="OnDisable"/> is called.
        /// </summary>
        public Action Disabled;

        /// <summary>
        /// Invoked on <see cref="ScriptableObject"/> <see cref="OnDestroy"/> is called.
        /// </summary>
        public Action Destroyed;

        #endregion

        #region IBehaviourAware

        /// <summary>
        /// Returns true if <see cref="ScriptableObject"/> <see cref="Awake"/> was called.
        /// </summary>
        public bool IsAwaken { get; private set; }

        /// <summary>
        /// Returns true if <see cref="ScriptableObject"/> <see cref="Start"/> was called.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Returns true if <see cref="ScriptableObject"/> <see cref="OnEnable"/> was called.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Returns true if <see cref="ScriptableObject"/> <see cref="OnDisable"/> was called.
        /// </summary>
        public bool IsDisabled { get; private set; }

        /// <summary>
        /// Returns true if <see cref="ScriptableObject"/> <see cref="OnDestroy"/> was called.
        /// </summary>
        public bool IsDestroyed { get; private set; }

		#endregion

		#region ScriptableObject

		protected virtual void Awake()
		{
			// keep at the end
			IsAwaken = true;
			Awaken?.Invoke();
		}

		protected virtual void Start()
		{
			// keep at the end
			IsStarted = true;
			Started?.Invoke();
		}

		protected virtual void OnEnable()
		{
			// keep at the end
			IsEnabled = true;
			Enabled?.Invoke();
		}

		protected virtual void OnDisable()
		{
			// keep at the end
			IsDisabled = true;
			Disabled?.Invoke();
		}

		protected virtual void OnDestroy()
		{
			// keep at the end
			IsDestroyed = true;
			Destroyed?.Invoke();
		}

		protected virtual void Update() { }

		#endregion
	}
}
