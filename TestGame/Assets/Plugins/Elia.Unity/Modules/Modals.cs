using System;
using System.Collections;
using System.Collections.Generic;
using Elia.Unity.Components.GameObjects;
using Elia.Unity.Components.GUI;
using UnityEngine;
using System.Linq;
using Elia.Unity.Infrastructure;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Module that manages GUI overlay.
    /// Instances of <see cref="GameObject"/> with component of <see cref="Modal"/> should be in scene hierarchy
    /// above all <see cref="GameObject"/> instances with component of <see cref="Screen"/> or <see cref="Overlay"/> type.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/Modals")]
	public sealed class Modals : BehaviourAwareSingleton<Modals>, INotifyPaused
	{
        #region Actions

        /// <summary>
        /// Invoked on all scene modals were populated. Called within <see cref="PopulateModals"/> method.
        /// </summary>
        public Action Populated;

        /// <summary>
        /// Invoked on modal of given type was activated.
        /// </summary>
		public Action<Type> ModalActivated;

        /// <summary>
        /// Invoked on modal of given types were deactivated.
        /// </summary>
		public Action<List<Type>> ModalsDeactivated;

        #endregion

        #region Members

        /// <summary>
        /// True if modals were populated from current scene.
        /// </summary>
        public bool IsPopulated { get; private set; }

        /// <summary>
        /// Number of populated modals.
        /// </summary>
		public int ModalCount { get { return _modals?.Count ?? 0; } }

        /// <summary>
        /// Parent <see cref="GameObject"/> that is used to populate modals from.
        /// </summary>
		public GameObject ModalsParentGo;

        /// <summary>
        /// List of <see cref="GameObject"/> that are skipped during <see cref="ModalsParentGo"/> children traversal.
        /// </summary>
		public GameObject[] SkipGos;

		private Dictionary<Type, Modal> _modals;
		private List<Modal> _activeModals;
		private Type _modalTypeSetActive;

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
			// asserts
			if (ModalsParentGo == null)
				throw new ArgumentNullException(nameof(ModalsParentGo));

			_modals = new Dictionary<Type, Modal>();
			_activeModals = new List<Modal>();

			base.Awake();
		}

		protected override void Start()
		{
			PopulateModals();

			App.Instance.ModalsPopulated?.Invoke();

			base.Start();
		}

        #endregion

        #region Init

        /// <summary>
        /// Populates all modals in current scene. To locate modals, it traverses children of <see cref="ModalsParentGo"/> object.
        /// </summary>
        private void PopulateModals()
		{
			foreach (Transform modalTransform in ModalsParentGo.transform)
			{
				var modalGo = modalTransform.gameObject;
				if (SkipGos != null && SkipGos.Contains(modalGo)) continue;

				var modal = modalGo.GetComponent<Modal>();
				_modals.Add(modal.GetType(), modal);
                
                if (App.Instance.Meta.ActivateDeactivateGUIOnStartup)
                {
                    var go = modal.gameObject;
                    go.SetActive(true); // ensure Awake is called on modals
					go.SetActive(false);
                }
            }

            Populated?.Invoke();
			IsPopulated = true;
		}

        #endregion

        #region Queries

        /// <summary>
        /// Get modal component by given type param.
        /// </summary>
        /// <typeparam name="T">Modal type</typeparam>
        /// <returns>Modal component</returns>
        public T GetModal<T>()
			where T : Modal
		{
			Modal modal;
			_modals.TryGetValue(typeof(T), out modal);
			return modal as T;
		}

        /// <summary>
        /// Get modal component by given type.
        /// </summary>
        /// <param name="modalType">Modal type</param>
        /// <returns>Modal component</returns>
		public Modal GetModal(Type modalType)
		{
			Modal modal;
			_modals.TryGetValue(modalType, out modal);
			return modal;
		}

        /// <summary>
        /// Get active modal components.
        /// </summary>
        /// <returns>Active modal components</returns>
		public Modal[] GetActiveModals()
		{
			return _activeModals.ToArray();
		}

        /// <summary>
        /// Returns true if modal of <typeparamref name="T"/> type is active.
        /// </summary>
        /// <typeparam name="T">Modal type param</typeparam>
        /// <returns>True if modal of <typeparamref name="T"/> type is active</returns>

        public bool IsModalActive<T>()
        {
            return _activeModals.Any(x => x.GetType() == typeof(T));
        }

        /// <summary>
        /// Returns true if modal of <paramref name="type"/> type is active.
        /// </summary>
        /// <param name="type">Modal type</param>
        /// <returns>True if modal of <paramref name="type"/> type is active</returns>
        public bool IsModalActive(Type type)
        {
            return _activeModals.Any(x => x.GetType() == type);
        }

        #endregion

        #region Modal Manipulation

        /// <summary>
        /// (De)activates modals of given type. if modal is already activated does nothing.
        /// </summary>
        /// <param name="modalType">Modal type</param>
        /// <param name="value">True to activate the modal</param>
        /// <param name="data">Data object to pass to modal</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="modalType"/> is null.</exception>
        public void OnModalSetActive(Type modalType, bool value, object data)
		{
			if (modalType == null) throw new ArgumentNullException(nameof(modalType));
			if (_modalTypeSetActive == modalType) return; // prevent cycle

			SetModalActive(modalType, value, data);
		}

        /// <summary>
        /// (De)activates modals of given type.
        /// </summary>
        /// <param name="modalType">Modal type</param>
        /// <param name="value">True to activate the modal</param>
        /// <param name="data">Data object to pass to modal</param>
        /// <returns>True if modal was (de)activated</returns>
		public bool SetModalActive(Type modalType, bool value, object data)
		{
			return SetModalActive(modalType, value, data, 0);
		}

        /// <summary>
        /// (De)activates modals of given type.
        /// </summary>
        /// <typeparam name="T">Modal type param</typeparam>
        /// <param name="value">True to activate the modal</param>
        /// <param name="data">Data object to pass to modal</param>
        /// <returns>True if modal was (de)activated</returns>
		public bool SetModalActive<T>(bool value, object data)
			where T : Modal
		{
			return SetModalActive(typeof(T), value, data, 0);
		}

        /// <summary>
        /// (De)activates modals of given type.
        /// </summary>
        /// <typeparam name="T">Modal type param</typeparam>
        /// <param name="value">True to activate the modal</param>
        /// <returns>True if modal was (de)activated</returns>
		public bool SetModalActive<T>(bool value)
			where T : Modal
		{
			return SetModalActive(typeof(T), value, null, 0);
		}

        /// <summary>
        /// (De)activates modals of given type.
        /// </summary>
        /// <param name="modalType">Modal type</param>
        /// <param name="value">True to activate the modal</param>
        /// <param name="data">Data object to pass to modal</param>
        /// <param name="defferActiveStateChangeByTime">Time delay (in seconds) of (de)activation</param>
        /// <returns>True if modal was (de)activated</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="modalType"/> is null</exception>
		public bool SetModalActive(Type modalType, bool value, object data, float defferActiveStateChangeByTime)
		{
			if (modalType == null)
				throw new ArgumentNullException(nameof(modalType));

			var modal = GetModal(modalType);

			if (value && _activeModals.Contains(modal))
			{
				App.Instance?.Logger.LogWarning(Texts.Tags.Modals, Texts.Errors.ModalIsAlreadyActive);
				return false;
			}
			if (!value && !_activeModals.Contains(modal))
			{
				App.Instance?.Logger.LogWarning(Texts.Tags.Modals, Texts.Errors.ModalIsNotActive);
				return false;
			}

			StartCoroutine(SetModalActiveInner(modal, value, data, defferActiveStateChangeByTime));

			return true;
		}

        /// <summary>
        /// (De)activates given modal.
        /// </summary>
        /// <param name="modal">Modal instance</param>
        /// <param name="value">True to activate the modal</param>
        /// <param name="data">Data object to pass to modal</param>
        /// <param name="defferActiveStateChangeByTime">Time delay (in seconds) of (de)activation</param>
        /// <returns>Coroutine's enumerator</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="modal"/> is null</exception>
		private IEnumerator SetModalActiveInner(Modal modal, bool value, object data, float defferActiveStateChangeByTime)
		{
			if (modal == null) throw new ArgumentNullException(nameof(modal));

			if (defferActiveStateChangeByTime > 0) yield return new WaitForSecondsRealtime(defferActiveStateChangeByTime);

			if (value)
			{
				_activeModals.Add(modal);
			}
			else
			{
				_activeModals.Remove(modal);
			}

			SetModalActiveAtomic(modal, value, data);

			if (value) ModalActivated?.Invoke(modal.GetType());
			else ModalsDeactivated?.Invoke(new List<Type>() { modal.GetType() });
		}

        /// <summary>
        /// Deactivates all active modals.
        /// </summary>
        /// <param name="skipModalTypes">List of <see cref="Modal"/> types to skip in the process</param>
		public void DeactivateAllModals(List<Type> skipModalTypes)
		{
			var deactivatedTypes = new List<Type>();
			foreach (var modal in _modals)
			{
				if (skipModalTypes != null && skipModalTypes.Contains(modal.Key)) continue;
				if (!modal.Value.IsActive) continue;

				_activeModals.Remove(modal.Value);
				SetModalActiveAtomic(modal.Value, false, null);

				deactivatedTypes.Add(modal.Key);
			}

			ModalsDeactivated?.Invoke(deactivatedTypes);
		}

        /// <summary>
        /// (De)activates given modal.
        /// </summary>
        /// <param name="modal">Instance of <see cref="Modal"/></param>
        /// <param name="value">True to activate</param>
        /// <param name="data">Data object to pass to modal</param>
		private void SetModalActiveAtomic(Modal modal, bool value, object data)
		{
			_modalTypeSetActive = modal.GetType();
			modal.SetActive(value, data);
			_modalTypeSetActive = null;
		}

        #endregion

        #region INotifyPaused

        /// <summary>
        /// Notifies active modal on <see cref="App"/> pause status change.
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
        public void NotifyAppPaused(bool pauseStatus)
		{
			if (_activeModals == null) return;

			foreach (var activeModal in _activeModals)
			{
				activeModal.OnAppTimerPaused(pauseStatus);
			}
		}

		/// <summary>
		/// Notifies active modal on <see cref="Game"/> pause status change.
		/// </summary>
		/// <param name="pauseStatus"></param>
		public void NotifyGamePaused(bool pauseStatus)
		{
			if (_activeModals == null) return;

			foreach (var activeModal in _activeModals)
			{
				activeModal.OnGameTimerPaused(pauseStatus);
			}
		}

		#endregion
	}
}
