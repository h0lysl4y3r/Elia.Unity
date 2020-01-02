using System;
using Elia.Unity.Modules;

namespace Elia.Unity.Components.GUI
{
    /// <summary>
    /// Single GUI modal managed by <see cref="Modals"/> module.
    /// </summary>
	public abstract class Modal : BehaviourAware
	{
		#region Members

        /// <summary>
        /// True if modal is active
        /// </summary>
		public virtual bool IsActive { get; private set; }

        /// <summary>
        /// Modal metadata
        /// </summary>
		public object Data { get; private set; }

		#endregion

		#region MonoBehaviour

		protected override void OnEnable()
		{
			Modals.Instance.ModalActivated += OnModalActivated;

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			Modals.Instance.ModalActivated -= OnModalActivated;

			base.OnDisable();
		}

		#endregion

		#region Management

        /// <summary>
        /// (De)activates modal.
        /// </summary>
        /// <param name="value">True to activate</param>
        /// <param name="data">Modal metadata</param>
		public virtual void SetActive(bool value, object data)
		{
			if (value == IsActive) return;

			IsActive = value;
			Data = data;

			if (IsActive)
				OnActivated();
			else
				OnDeactivated();

			Modals.Instance.OnModalSetActive(this.GetType(), value, data);
		}

        /// <summary>
        /// Callback called within <see cref="SetActive"/> method. If <see cref="App.AppMeta.ModalsAutoActivateGameObjects"/> is true, activates modal.
        /// </summary>
		protected virtual void OnActivated()
		{
			if (Modules.App.Instance.Meta.ModalsAutoActivateGameObjects)
				gameObject.SetActive(true);
		}

        /// <summary>
        /// Callback called within <see cref="SetActive"/> method. If <see cref="App.AppMeta.ModalsAutoActivateGameObjects"/> is true, deactivates modal.
        /// </summary>
		protected virtual void OnDeactivated()
		{
			if (Modules.App.Instance.Meta.ModalsAutoActivateGameObjects)
				gameObject.SetActive(false);
		}

        /// <summary>
        /// Callback that is invoked on <see cref="Modals.SetModalActive"/> method called.
        /// </summary>
        /// <param name="modalType"></param>
		protected virtual void OnModalActivated(Type modalType) { }

        #endregion

        #region Modals

        /// <summary>
        /// If modal is active, it is notified via this callback of <see cref="App"/> pause status change.
        /// </summary>
        /// <param name="pauseStatus">True if <see cref="App"/> is paused</param>
        public virtual void OnAppTimerPaused(bool pauseStatus) { }

		/// <summary>
		/// If modal is active, it is notified via this callback of <see cref="Game"/> pause status change.
		/// </summary>
		/// <param name="pauseStatus">True if <see cref="Game"/> is paused</param>
		public virtual void OnGameTimerPaused(bool pauseStatus) { }

		#endregion
	}
}
