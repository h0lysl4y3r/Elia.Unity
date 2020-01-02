using System;
using System.Collections;
using Elia.Unity.Components.GUI;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Object that is held by <see cref="App"/> module and manages menu related GUI screens.
    /// </summary>
	public sealed class Menu
    {
        #region Members

        /// <summary>
        /// Determines whether <see cref="Menu"/> is active. Is set in <see cref="SetActive"/> method.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// <see cref="Screen"/> derived type of first screen that will be activated.
        /// </summary>
		public Type FirstActiveScreenType { get; set; }

        /// <summary>
        /// <see cref="Screen"/> derived type of default Options screen.
        /// </summary>
		public Type PausedActiveScreenType { get; set; }

        /// <summary>
        /// Time delay (in seconds) before <see cref="DeactivateActiveScreens"/> method is called on <see cref="Screens"/> module.
        /// </summary>
		public float DefferScreenDeactivationTime { get; set; }

        #endregion

        #region Management

        /// <summary>
        /// Activates <see cref="Menu"/>. Sets <see cref="IsActive"/> property.
        /// If <see cref="IsActive"/> is true, <see cref="OnActivated"/> method is called.
        /// If <see cref="IsActive"/> is false, <see cref="OnDeactivated"/> method is called. 
        /// </summary>
        /// <param name="value">True to activate menu</param>
        /// <returns></returns>
        public bool SetActive(bool value)
        {
            if (value == IsActive) return false;

            IsActive = value;

            if (IsActive)
            {
                OnActivated(App.Instance.IsGamePaused());
            }
            else
            {
                OnDeactivated();
            }

            return true;
        }

        /// <summary>
        /// Activates <see cref="FirstActiveScreenType"/> or <see cref="PausedActiveScreenType"/> screen. Called within <see cref="SetActive"/> method.
        /// </summary>
        /// <param name="isGamePaused">True if game is paused, in that case <see cref="PausedActiveScreenType"/> is activated</param>
        /// <exception cref="ArgumentException">Thrown when <see cref="FirstActiveScreenType"/> or <see cref="PausedActiveScreenType"/> is of invalid type</exception>
		private void OnActivated(bool isGamePaused)
		{
            App.Instance.Timer.Paused += OnAppTimerPaused;

			Type screenType = null;

			if (!isGamePaused && FirstActiveScreenType != null)
			{
				if (!FirstActiveScreenType.IsSubclassOf(typeof(Screen)))
					throw new ArgumentException(Texts.Errors.TypeSpecifiedMustBeOfDifferentType, nameof(FirstActiveScreenType));

				screenType = FirstActiveScreenType;
			}

			if (isGamePaused && PausedActiveScreenType != null)
			{
				if (!PausedActiveScreenType.IsSubclassOf(typeof(Screen)))
					throw new ArgumentException(Texts.Errors.TypeSpecifiedMustBeOfDifferentType, nameof(PausedActiveScreenType));

				screenType = PausedActiveScreenType;
			}

			Screens.Instance.SetScreenActive(screenType, true);
		}

        /// <summary>
        /// Deactivates active screens. Called within <see cref="SetActive"/> method.
        /// </summary>
		private void OnDeactivated()
		{
			Screens.Instance.DeactivateActiveScreens(DefferScreenDeactivationTime);

			App.Instance.Timer.Paused -= OnAppTimerPaused;
		}

        #endregion

        #region App

        /// <summary>
        /// Callback on <see cref="App.Instance.Timer.Paused"/> action.
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
        private void OnAppTimerPaused(bool pauseStatus)
		{
			Modals.Instance?.NotifyAppPaused(pauseStatus);
			Overlays.Instance?.NotifyAppPaused(pauseStatus);
			Screens.Instance?.NotifyAppPaused(pauseStatus);
		}

		#endregion
	}
}
