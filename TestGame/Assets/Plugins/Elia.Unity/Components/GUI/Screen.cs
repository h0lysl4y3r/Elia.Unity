using System;
using System.Collections.Generic;
using Elia.Unity.Modules;
using UnityEngine;

namespace Elia.Unity.Components.GUI
{
    /// <summary>
    /// Single GUI screen managed by <see cref="Screens"/> module.
    /// </summary>
	public abstract class Screen : BehaviourAware
	{
		#region Members

        /// <summary>
        /// True if this screen is active.
        /// </summary>
		public virtual bool IsActive { get; private set; }

        private Canvas _canvas;

        #endregion

        #region MonoBehaviour

        protected override void OnEnable()
		{
            _canvas = GetComponent<Canvas>();

            Screens.Instance.ScreenActivated += OnScreenActivated;
			Screens.Instance.ScreensWillBeDeactivated += OnScreensWillBeDeactivated;

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			Screens.Instance.ScreenActivated -= OnScreenActivated;
			Screens.Instance.ScreensWillBeDeactivated -= OnScreensWillBeDeactivated;

			base.OnDisable();
		}

		#endregion

		#region Management

        /// <summary>
        /// (De)activates screen.
        /// </summary>
        /// <param name="value">True to activate</param>
		public virtual void SetActive(bool value)
		{
			if (value == IsActive) return;

			IsActive = value;

			if (IsActive)
				OnActivated();
			else
				OnDeactivated();

			Screens.Instance.OnScreenSetActive(this.GetType(), value);
		}

        /// <summary>
        /// Callback called within <see cref="SetActive"/> method. If <see cref="App.AppMeta.ModalsAutoActivateGameObjects"/> is true, activates screen.
        /// </summary>
		protected virtual void OnActivated()
		{
			if (Modules.App.Instance.Meta.ScreensAutoActivateGameObjects)
            {
                if (_canvas != null && Screens.Instance.ActivateDeactivateCanvases)
                    _canvas.enabled = true;
                else
                    gameObject.SetActive(true);
            }
		}

        /// <summary>
        /// Callback called within <see cref="SetActive"/> method. If <see cref="App.AppMeta.ModalsAutoActivateGameObjects"/> is true, deactivates screen.
        /// </summary>
		protected virtual void OnDeactivated()
		{
			if (Modules.App.Instance.Meta.ScreensAutoActivateGameObjects)
            {
                if (_canvas != null && Screens.Instance.ActivateDeactivateCanvases)
                    _canvas.enabled = false;
                else
                    gameObject.SetActive(false);
            }
		}

        /// <summary>
        /// Callback that is invoked on <see cref="Screens.SetScreenActive"/> method called.
        /// </summary>
        /// <param name="screenType">Screen type</param>
		protected virtual void OnScreenActivated(Type screenType) { }

        /// <summary>
        /// Callback that is invoked on <see cref="Screens.SetScreenActive"/> method called upon deactivation of screen that is currently active.
        /// </summary>
        /// <param name="screenTypes">List of screen types</param>
		protected virtual void OnScreensWillBeDeactivated(List<Type> screenTypes) { }

        #endregion

        #region Screens

        /// <summary>
        /// If screen is active, it is notified via this callback of <see cref="App"/> pause status change.
        /// </summary>
        /// <param name="pauseStatus">True if <see cref="App"/> is paused</param>
        public virtual void OnAppTimerPaused(bool pauseStatus) { }

		/// <summary>
		/// If screen is active, it is notified via this callback of <see cref="Game"/> pause status change.
		/// </summary>
		/// <param name="pauseStatus">True if <see cref="Game"/> is paused</param>
		public virtual void OnGameTimerPaused(bool pauseStatus) { }

		#endregion
	}
}
