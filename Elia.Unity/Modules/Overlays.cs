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
    /// Module that manages instances of <see cref="Overlay"/> type. 
    /// Instances of <see cref="GameObject"/> with component of <see cref="Overlay"/> should be in scene hierarchy
    /// below all <see cref="GameObject"/> instances with component of <see cref="Modal"/> type and above instances of <see cref="Screen"/> type.
    /// </summary>
    [AddComponentMenu("ELIA/Modules/Overlays")]
	public sealed class Overlays : BehaviourAwareSingleton<Overlays>, INotifyPaused
	{
        #region Actions

        /// <summary>
        /// Invoked on all scene overlays were populated. Called within <see cref="PopulateOverlays"/> method.
        /// </summary>
        public Action Populated;

        /// <summary>
        /// Invoked on overlay of given type was activated.
        /// </summary>
		public Action<Type> OverlayActivated;

        /// <summary>
        /// Invoked on overlay of given types were deactivated.
        /// </summary>
		public Action<List<Type>> OverlaysDeactivated;

        #endregion

        #region Members

        /// <summary>
        /// True if overlays were populated from current scene.
        /// </summary>
        public bool IsPopulated { get; private set; }

        /// <summary>
        /// Number of populated overlays.
        /// </summary>
		public int OverlayCount { get { return _overlays?.Count ?? 0; } }

        /// <summary>
        /// Parent <see cref="GameObject"/> that is used to populate overlays from.
        /// </summary>
		public GameObject OverlaysParentGo;

        /// <summary>
        /// List of <see cref="GameObject"/> that are skipped during <see cref="OverlaysParentGo"/> children traversal.
        /// </summary>
		public GameObject[] SkipGos;

		private Dictionary<Type, Overlay> _overlays;
		private List<Overlay> _activeOverlays;
		private Type _overlayTypeSetActive;

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
			// asserts
			if (OverlaysParentGo == null)
				throw new ArgumentNullException(nameof(OverlaysParentGo));

			_overlays = new Dictionary<Type, Overlay>();
			_activeOverlays = new List<Overlay>();

			base.Awake();
		}

		protected override void Start()
		{
			PopulateOverlays();

			App.Instance.OverlaysPopulated?.Invoke();

			base.Start();
		}

        #endregion

        #region Init

        /// <summary>
        /// Populates all overlays in current scene. To locate overlays, it traverses children of <see cref="ModalsParentGo"/> object.
        /// </summary>
        private void PopulateOverlays()
		{
			foreach (Transform overlayTransform in OverlaysParentGo.transform)
			{
				var overlayGo = overlayTransform.gameObject;
				if (SkipGos != null && SkipGos.Contains(overlayGo)) continue;

				var overlay = overlayGo.GetComponent<Overlay>();
				_overlays.Add(overlay.GetType(), overlay);

                if (App.Instance.Meta.ActivateDeactivateGUIOnStartup)
			    {
			        var go = overlay.gameObject;
                    go.SetActive(true); // ensure Awake is called
					go.SetActive(false);
                }
            }

			Populated?.Invoke();
			IsPopulated = true;
		}

        #endregion

        #region Queries

        /// <summary>
        /// Get overlay component by given type param.
        /// </summary>
        /// <typeparam name="T">Overlay type</typeparam>
        /// <returns>Overlay component</returns>
        public T GetOverlay<T>()
			where T : Overlay
		{
			Overlay overlay;
			_overlays.TryGetValue(typeof (T), out overlay);
			return overlay as T;
		}

        /// <summary>
        /// Get overlay component by given type.
        /// </summary>
        /// <param name="overlayType">Overlay type</param>
        /// <returns>Overlay component</returns>
		public Overlay GetOverlay(Type overlayType)
		{
			Overlay overlay;
			_overlays.TryGetValue(overlayType, out overlay);
			return overlay;
		}

        /// <summary>
        /// Get active overlay components.
        /// </summary>
        /// <returns>Active overlay components</returns>
        public Overlay[] GetActiveOverlays()
		{
			return _activeOverlays.ToArray();
		}

        /// <summary>
        /// Returns true if overlay of <typeparamref name="T"/> type is active.
        /// </summary>
        /// <typeparam name="T">Overlay type param</typeparam>
        /// <returns>True if overlay of <typeparamref name="T"/> type is active</returns>
        public bool IsOverlayActive<T>()
        {
            return _activeOverlays.Any(x => x.GetType() == typeof(T));
        }

        /// <summary>
        /// Returns true if overlay of <paramref name="type"/> type is active.
        /// </summary>
        /// <param name="type">Overlay type</param>
        /// <returns>True if overlay of <paramref name="type"/> type is active</returns>
        public bool IsOverlayActive(Type type)
        {
            return _activeOverlays.Any(x => x.GetType() == type);
        }

        #endregion

        #region Overlay Manipulation

        /// <summary>
        /// (De)activates overlays of given type. if overlay is already activated does nothing.
        /// </summary>
        /// <param name="overlayType">Overlay type</param>
        /// <param name="value">True to activate</param>
        public void OnOverlaySetActive(Type overlayType, bool value)
		{
			if (overlayType == null) throw new ArgumentNullException(nameof(overlayType));
			if (_overlayTypeSetActive == overlayType) return; // prevent cycle

			SetOverlayActive(overlayType, value);
		}

        /// <summary>
        /// (De)activates overlays of given type.
        /// </summary>
        /// <param name="overlayType">Overlay type</param>
        /// <param name="value">True to activate</param>
        /// <returns>True if overlay was (de)activated</returns>
		public bool SetOverlayActive(Type overlayType, bool value)
		{
			return SetOverlayActive(overlayType, value, 0);
		}

        /// <summary>
        /// (De)activates overlays of given type.
        /// </summary>
        /// <typeparam name="T">Overlay type param</typeparam>
        /// <param name="value">True to activate</param>
        /// <returns>True if overlay was (de)activated</returns>
		public bool SetOverlayActive<T>(bool value)
			where T : Overlay
		{
			return SetOverlayActive(typeof(T), value, 0);
		}

        /// <summary>
        /// (De)activates overlays of given type.
        /// </summary>
        /// <param name="overlayType">Overlay type</param>
        /// <param name="value">True to activate</param>
        /// <param name="defferActiveStateChangeByTime">Time delay (in seconds) of (de)activation</param>
        /// <returns>True if overlay was (de)activated</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="overlayType"/> is null</exception>
		public bool SetOverlayActive(Type overlayType, bool value, float defferActiveStateChangeByTime)
		{
			if (overlayType == null)
				throw new ArgumentNullException(nameof(overlayType));

			var overlay = GetOverlay(overlayType);

			if (value && _activeOverlays.Contains(overlay))
			{
				App.Instance?.Logger.LogWarning(Texts.Tags.Overlays, Texts.Errors.OverlayIsAlreadyActive);
				return false;
			}
			if (!value && !_activeOverlays.Contains(overlay))
			{
				App.Instance?.Logger.LogWarning(Texts.Tags.Overlays, Texts.Errors.OverlayIsNotActive);
				return false;
			}

			StartCoroutine(SetOverlayActiveInner(overlay, value, defferActiveStateChangeByTime));

			return true;
		}

        /// <summary>
        /// (De)activates overlay instance.
        /// </summary>
        /// <param name="overlay">Overlay instance</param>
        /// <param name="value">True to activate</param>
        /// <param name="defferActiveStateChangeByTime">Time delay (in seconds) of (de)activation</param>
        /// <returns>Coroutine's enumerator</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="overlay"/> is null</exception>
		private IEnumerator SetOverlayActiveInner(Overlay overlay, bool value, float defferActiveStateChangeByTime)
		{
			if (overlay == null) throw new ArgumentNullException(nameof(overlay));

			if (defferActiveStateChangeByTime > 0) yield return new WaitForSecondsRealtime(defferActiveStateChangeByTime);

			if (value)
			{
				_activeOverlays.Add(overlay);
			}
			else
			{
				_activeOverlays.Remove(overlay);
			}

			SetOverlayActiveAtomic(overlay, value);

			if (value) OverlayActivated?.Invoke(overlay.GetType());
			else OverlaysDeactivated?.Invoke(new List<Type>() { overlay.GetType() });
		}

        /// <summary>
        /// Deactivates all active overlays.
        /// </summary>
        /// <param name="skipOverlayTypes">List of <see cref="Overlay"/> types to skip in the process</param>
		public void DeactivateAllOverlays(List<Type> skipOverlayTypes)
		{
			var deactivatedTypes = new List<Type>();
			foreach (var overlay in _overlays)
			{
				if (skipOverlayTypes != null && skipOverlayTypes.Contains(overlay.Key)) continue;
				if (!overlay.Value.IsActive) continue;

				_activeOverlays.Remove(overlay.Value);
				SetOverlayActiveAtomic(overlay.Value, false);

				deactivatedTypes.Add(overlay.Key);
			}

			OverlaysDeactivated?.Invoke(deactivatedTypes);
		}

        /// <summary>
        /// (De)activates given overlay.
        /// </summary>
        /// <param name="overlay">Overlay instance</param>
        /// <param name="value">True to activate</param>
		private void SetOverlayActiveAtomic(Overlay overlay, bool value)
		{
			_overlayTypeSetActive = overlay.GetType();
			overlay.SetActive(value);
			_overlayTypeSetActive = null;
		}

        #endregion

        #region INotifyPaused

        /// <summary>
        /// Notifies active overlay on <see cref="App"/> pause status change.
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
        public void NotifyAppPaused(bool pauseStatus)
		{
			if (_activeOverlays == null) return;

			foreach (var activeOverlay in _activeOverlays)
			{
				activeOverlay.OnAppTimerPaused(pauseStatus);
			}
		}

		/// <summary>
		/// Notifies active overlay on <see cref="Game"/> pause status change.
		/// </summary>
		/// <param name="pauseStatus"></param>
		public void NotifyGamePaused(bool pauseStatus)
		{
			if (_activeOverlays == null) return;

			foreach (var activeOverlay in _activeOverlays)
			{
				activeOverlay.OnGameTimerPaused(pauseStatus);
			}
		}

		#endregion
	}
}