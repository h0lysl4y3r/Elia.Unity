using System;
using Elia.Unity.Modules;

namespace Elia.Unity.Components.GUI
{
    /// <summary>
    /// Single GUI overlay managed by <see cref="Overlays"/> module.
    /// </summary>
	public abstract class Overlay : BehaviourAware
	{
		#region Members

        /// <summary>
        /// True if overlay is active
        /// </summary>
		public virtual bool IsActive { get; private set; }

		#endregion

		#region MonoBehaviour

		protected override void OnEnable()
		{
			Overlays.Instance.OverlayActivated += OnOverlayActivated;

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			Overlays.Instance.OverlayActivated -= OnOverlayActivated;

			base.OnDisable();
		}

		#endregion

		#region Management

        /// <summary>
        /// (De)activates overlay.
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

			Overlays.Instance.OnOverlaySetActive(this.GetType(), value);
		}

        /// <summary>
        /// Callback called within <see cref="SetActive"/> method on overlay activated. If <see cref="App.AppMeta.ModalsAutoActivateGameObjects"/> is true, activates overlay.
        /// </summary>
		protected virtual void OnActivated()
		{
			if (Modules.App.Instance.Meta.OverlaysAutoActivateGameObjects)
				gameObject.SetActive(true);
		}

        /// <summary>
        /// Callback called within <see cref="SetActive"/> method on overlay deactivated. If <see cref="App.AppMeta.ModalsAutoActivateGameObjects"/> is true, deactivates overlay.
        /// </summary>
		protected virtual void OnDeactivated()
		{
			if (Modules.App.Instance.Meta.OverlaysAutoActivateGameObjects)
				gameObject.SetActive(false);
		}

        /// <summary>
        /// Callback that is invoked on <see cref="Overlays.SetOverlayActive"/> method called.
        /// </summary>
        /// <param name="overlayType">Overlay type</param>
		protected virtual void OnOverlayActivated(Type overlayType) { }

        #endregion

        #region Overlays

        /// <summary>
        /// If overlay is active, it is notified via this callback of <see cref="App"/> pause status change.
        /// </summary>
        /// <param name="pauseStatus">True if <see cref="App"/> is paused</param>
        public virtual void OnAppTimerPaused(bool pauseStatus) { }

		/// <summary>
		/// If overlay is active, it is notified via this callback of <see cref="Game"/> pause status change.
		/// </summary>
		/// <param name="pauseStatus">True if <see cref="Game"/> is paused</param>
		public virtual void OnGameTimerPaused(bool pauseStatus) { }

		#endregion
	}
}
