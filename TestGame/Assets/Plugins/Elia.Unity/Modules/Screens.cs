using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elia.Unity.Components.GUI;
using UnityEngine;
using Screen = Elia.Unity.Components.GUI.Screen;

using Elia.Unity.Infrastructure;
namespace Elia.Unity.Modules
{
    /// <summary>
    /// Module that manages instances of <see cref="Screen"/> type. 
    /// Instances of <see cref="GameObject"/> with component of <see cref="Screen"/> should be in scene hierarchy
    /// below all <see cref="GameObject"/> instances with components of either <see cref="Overlay"/> or <see cref="Modal"/> type.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/Screens")]
	public sealed class Screens : BehaviourAwareSingleton<Screens>, INotifyPaused
	{
		#region Actions

        /// <summary>
        /// Invoked on all scene screens were populated. Called within <see cref="PopulateScreens"/> method.
        /// </summary>
		public Action Populated;

        /// <summary>
        /// Invoked on screen of given type was activated.
        /// </summary>
		public Action<Type> ScreenActivated;

        /// <summary>
        /// Invoked on screen of given types were deactivated.
        /// </summary>
		public Action<List<Type>> ScreensDeactivated;

        /// <summary>
        /// Invoked on screens will be deactivated, usually upon other screen will be activated. Called within <see cref="SetScreenActive"/> method.
        /// </summary>
		public Action<List<Type>> ScreensWillBeDeactivated;

		#endregion

		#region Members

        /// <summary>
        /// True if screens were populated from current scene.
        /// </summary>
		public bool IsPopulated { get; private set; }

        /// <summary>
        /// Number of populated screens.
        /// </summary>
		public int ScreenCount { get { return _screens?.Count ?? 0; } }

        /// <summary>
        /// Parent <see cref="GameObject"/> that is used to populate screens from.
        /// </summary>
		public GameObject ScreensParentGo;

        /// <summary>
        /// Parent <see cref="GameObject"/> that is used to populate screens from and should be placed in the scene on top of <see cref="ScreensParentGo"/> and other game related GUI.
        /// </summary>
	    public GameObject TopScreensParentGo;

        /// <summary>
        /// List of <see cref="GameObject"/> that are skipped during <see cref="ScreensParentGo"/> and <see cref="TopScreensParentGo"/> children traversal.
        /// </summary>
		public GameObject[] SkipGos;

        /// <summary>
        /// True to activate/deactivate <see cref="Screen"/> <see cref="Canvas"/> component instead of <see cref="GameObject"/>
        /// </summary>
        public bool ActivateDeactivateCanvases { get; set; }

        private Dictionary<Type, Screen> _screens;
		private List<Screen> _activeScreens;
		private Type _screenTypeSetActive;

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
			// asserts
			if (ScreensParentGo == null)
				throw new ArgumentNullException(nameof(ScreensParentGo));

			_screens = new Dictionary<Type, Screen>();
			_activeScreens = new List<Screen>();

			base.Awake();
		}

		protected override void Start()
		{
			PopulateScreens();

			App.Instance.ScreensPopulated?.Invoke();

			base.Start();
		}

        #endregion

        #region Init

        /// <summary>
        /// Populates all screens in current scene. To locate screens, it traverses children of <see cref="ScreensParentGo"/> and <see cref="TopScreensParentGo"/> objects.
        /// </summary>
        private void PopulateScreens()
		{
			foreach (Transform screenTransform in ScreensParentGo.transform)
			{
				var screenGo = screenTransform.gameObject;
				if (SkipGos != null && SkipGos.Contains(screenGo)) continue;

				var screen = screenGo.GetComponent<Screen>();
				_screens.Add(screen.GetType(), screen);

			}

            // do we have additional screen parent GO?
		    if (TopScreensParentGo != null)
		    {
                foreach (Transform screenTransform in TopScreensParentGo.transform)
                {
                    var screenGo = screenTransform.gameObject;
                    if (SkipGos != null && SkipGos.Contains(screenGo)) continue;

                    var screen = screenGo.GetComponent<Screen>();
                    _screens.Add(screen.GetType(), screen);
                }
            }

            if (App.Instance.Meta.ActivateDeactivateGUIOnStartup)
            {
                foreach (var screenKvp in _screens)
                {
                    var screen = screenKvp.Value;
                    screen.gameObject.SetActive(true); // ensure Awake is called
					screen.gameObject.SetActive(false);
				}
			}

			Populated?.Invoke();
			IsPopulated = true;
		}

        #endregion

        #region Queries

        /// <summary>
        /// Get screen component by given type param.
        /// </summary>
        /// <typeparam name="T">Screen type</typeparam>
        /// <returns>Screen component</returns>
        public T GetScreen<T>()
			where T : Screen
		{
			Screen screen;
			_screens.TryGetValue(typeof(T), out screen);
			return screen as T;
		}

        /// <summary>
        /// Get screen component by given type.
        /// </summary>
        /// <param name="screenType">Screen type</param>
        /// <returns>Screen component</returns>
		public Screen GetScreen(Type screenType)
		{
			Screen screen;
			_screens.TryGetValue(screenType, out screen);
			return screen;
		}

        /// <summary>
        /// Get active screen components.
        /// </summary>
        /// <returns>Active screen components</returns>
		public Screen[] GetActiveScreens()
		{
			return _activeScreens.ToArray();
		}

        /// <summary>
        /// Returns true if screen of <typeparamref name="T"/> type is active.
        /// </summary>
        /// <typeparam name="T">Screen type param</typeparam>
        /// <returns>True if screen of <typeparamref name="T"/> type is active</returns>
        public bool IsScreenActive<T>()
        {
            return _activeScreens.Any(x => x.GetType() == typeof(T));
        }

        /// <summary>
        /// Returns true if screen of <paramref name="type"/> type is active.
        /// </summary>
        /// <param name="type">Screen type</param>
        /// <returns>True if screen of <paramref name="type"/> type is active</returns>
        public bool IsScreenActive(Type type)
        {
            return _activeScreens.Any(x => x.GetType() == type);
        }

        #endregion

        #region Screen Manipulation

        /// <summary>
        /// (De)activates screens of given type. if screen is already activated does nothing.
        /// </summary>
        /// <param name="screenType">Screen type</param>
        /// <param name="value">True to activate the screen</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="screenType"/> is null.</exception>
        public void OnScreenSetActive(Type screenType, bool value)
		{
			if (screenType == null) throw new ArgumentNullException(nameof(screenType));
			if (_screenTypeSetActive == screenType) return; // prevent cycle

			SetScreenActive(screenType, value, 0, 0, null, false);
		}

        /// <summary>
        /// (De)activates screens of given type.
        /// </summary>
        /// <param name="screenType">Screen type</param>
        /// <param name="value">True to activate screen</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="screenType"/> is null.</exception>
		public bool SetScreenActive(Type screenType, bool value)
		{
			if (screenType == null)
				throw new ArgumentNullException(nameof(screenType));

			return SetScreenActive(screenType, value, 0, 0, null, true);
		}

        /// <summary>
        /// (De)activates screens of given type.
        /// </summary>
        /// <typeparam name="T">Screen type param</typeparam>
        /// <param name="value">True to activate screen</param>
        /// <returns></returns>
		public bool SetScreenActive<T>(bool value)
			where T : Screen
		{
			return SetScreenActive(typeof(T), value, 0, 0, null, true);
		}

        /// <summary>
        /// (De)activates screens of given type.
        /// </summary>
        /// <param name="screenType">Screen type</param>
        /// <param name="value">True to activate screen</param>
        /// <param name="defferActivationTime">Time delay (in seconds) to activate</param>
        /// <param name="defferDeactivationTime">Time delay (in seconds) to deactivate</param>
        /// <param name="skipScreenTypes">List of <see cref="Screen"/> types to skip in the process</param>
        /// <param name="changeActiveState">True to notify active screens that will be (de)activated</param>
        /// <returns>True if screen is (de)activated</returns>
		public bool SetScreenActive(Type screenType, bool value, float defferActivationTime, float defferDeactivationTime, List<Type> skipScreenTypes, bool changeActiveState)
		{
			var screen = GetScreen(screenType);
			if (screen == null) return false;

			if (value && _activeScreens.Contains(screen))
			{
				App.Instance?.Logger.LogWarning(Texts.Tags.Screens, Texts.Errors.ScreenIsAlreadyActive);
				return false;
			}
			if (!value && !_activeScreens.Contains(screen))
			{
				App.Instance?.Logger.LogWarning(Texts.Tags.Screens, Texts.Errors.ScreenIsNotActive);
				return false;
			}

            // Active screens will be deactivated, notify them
			NotifyScreensWillBeDeactivated(skipScreenTypes);

			if (value)
			{
				StartCoroutine(SetScreenActiveInner(screenType, screen, value, defferActivationTime, defferDeactivationTime, skipScreenTypes, changeActiveState));
			}
			else
			{
				StartCoroutine(SetScreenActiveInner(screenType, screen, value, defferActivationTime, defferDeactivationTime, skipScreenTypes, changeActiveState));
			}

			return true;
		}

        /// <summary>
        /// (De)activates screen of given type.
        /// </summary>
        /// <param name="screenType">Screen type</param>
        /// <param name="screen"><see cref="Screen"/> instance to (de)activate</param>
        /// <param name="value">True to activate screen</param>
        /// <param name="defferActivationTime">Time delay (in seconds) to activate</param>
        /// <param name="defferDeactivationTime">Time delay (in seconds) to deactivate</param>
        /// <param name="skipScreenTypes">List of <see cref="Screen"/> types to skip in the process</param>
        /// <param name="changeActiveState">True to notify active screens that will be (de)activated</param>
        /// <returns>Coroutine's enumerator</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="screenType"/> or <paramref name="screen"/> is null</exception>
		private IEnumerator SetScreenActiveInner(Type screenType, Screen screen, bool value, float defferActivationTime, float defferDeactivationTime, List<Type> skipScreenTypes, bool changeActiveState)
		{
			if (screenType == null) throw new ArgumentNullException(nameof(screenType));
			if (screen == null) throw new ArgumentNullException(nameof(screen));

			if (defferActivationTime > 0) yield return new WaitForSecondsRealtime(defferActivationTime);
			if (defferDeactivationTime > 0) yield return new WaitForSecondsRealtime(defferDeactivationTime);

			List<Type> deactivatedTypes;
			if (value)
			{
				deactivatedTypes = DeactivateActiveScreensInner(screenType, skipScreenTypes);
				_activeScreens.Add(screen);
			}
			else
			{
				_activeScreens.Remove(screen);
				deactivatedTypes = new List<Type>() { screenType };
			}

			if (changeActiveState)
			{
				SetScreenActiveAtomic(screen, value);
			}

			if (value) ScreenActivated?.Invoke(screenType);
			ScreensDeactivated?.Invoke(deactivatedTypes);
		}

        /// <summary>
        /// Invokes <see cref="ScreensWillBeDeactivated"/> action on screens will be deactivated.
        /// </summary>
        /// <param name="skipScreenTypes">List of <see cref="Screen"/> types to skip in the process</param>
		private void NotifyScreensWillBeDeactivated(List<Type> skipScreenTypes)
		{
			if (ScreensWillBeDeactivated == null) return;

			var notifyScreens = skipScreenTypes != null 
				? _activeScreens.Where(x => !skipScreenTypes.Contains(x.GetType())).ToList() 
				: _activeScreens;
			ScreensWillBeDeactivated.Invoke(notifyScreens.Select(x => x.GetType()).ToList());
		}

        /// <summary>
        /// Deactivates active screens with time delay.
        /// </summary>
        /// <param name="defferDeactivationTime">Time delay (in seconds)</param>
		public void DeactivateActiveScreens(float defferDeactivationTime)
		{
			StartCoroutine(DeactivateActiveScreensDeffered(defferDeactivationTime));
		}

        /// <summary>
        /// Deactivates active screens with time delay.
        /// </summary>
        /// <param name="defferDeactivationTime">Time delay (in seconds)</param>
        /// <returns>Coroutine's enumerator</returns>
		private IEnumerator DeactivateActiveScreensDeffered(float defferDeactivationTime)
		{
			if (defferDeactivationTime > 0) yield return new WaitForSecondsRealtime(defferDeactivationTime);

			DeactivateActiveScreensInner(null, null);
		}

        /// <summary>
        /// Deactivates active screens.
        /// </summary>
        /// <param name="activatingScreenType">Type of screen that is being activated (is skipped)</param>
        /// <param name="skipScreenTypes">List of <see cref="Screen"/> types to skip in the process</param>
        /// <returns>List of <see cref="Screen"/> types deactivated</returns>
		private List<Type> DeactivateActiveScreensInner(Type activatingScreenType, List<Type> skipScreenTypes)
		{
			var deactivatedTypes = new List<Type>();

			foreach (var screen in _screens)
			{
				if (skipScreenTypes != null && skipScreenTypes.Contains(screen.Key)) continue;
				if (!screen.Value.IsActive) continue;
                if (activatingScreenType != null && activatingScreenType == screen.Key) continue;

				_activeScreens.Remove(screen.Value);

				SetScreenActiveAtomic(screen.Value, false);

				deactivatedTypes.Add(screen.Key);
			}

			return deactivatedTypes;
		}

        /// <summary>
        /// (De)activates given screen.
        /// </summary>
        /// <param name="screen">Instance of <see cref="Screen"/> type</param>
        /// <param name="value">True to activate</param>
		private void SetScreenActiveAtomic(Screen screen, bool value)
		{
			_screenTypeSetActive = screen.GetType();
			screen.SetActive(value);
			_screenTypeSetActive = null;
		}

        #endregion

        #region INotifyPaused

        /// <summary>
        /// Notifies active screen on <see cref="App"/> pause status change.
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
        public void NotifyAppPaused(bool pauseStatus)
		{
			if (_activeScreens == null) return;

			foreach (var activeScreen in _activeScreens)
			{
				activeScreen.OnAppTimerPaused(pauseStatus);
			}
		}

		/// <summary>
		/// Notifies active screen on <see cref="Game"/> pause status change.
		/// </summary>
		/// <param name="pauseStatus"></param>
		public void NotifyGamePaused(bool pauseStatus)
		{
			if (_activeScreens == null) return;

			foreach (var activeScreen in _activeScreens)
			{
				activeScreen.OnGameTimerPaused(pauseStatus);
			}
		}

		#endregion
	}
}
