using System;
using System.Collections.Generic;
using Elia.Unity.Components.App;
using Elia.Unity.Components.Games;
using Elia.Unity.Components.Debug;
using Elia.Unity.Extensions;
using Elia.Unity.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// App is a basic module that manages GUI (<see cref="Screens"/>, <see cref="Modals"/>, <see cref="Overlays"/>) and the <see cref="Game"/>.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/App")]
	public sealed class App : BehaviourAwareSingleton<App>
	{
		#region Inner Types

        /// <summary>
        /// Basic application state
        /// </summary>
		public enum ApplicationStateEnum
		{
			Undefined,
			InMenu,
			InGame,
		}

		#endregion

		#region Actions

        /// <summary>
        /// Invoked on system application (un)pause
        /// </summary>
		public Action<bool> ApplicationPaused;

        /// <summary>
        /// Invoked on system application quit
        /// </summary>
		public Action ApplicationQuit;

        /// <summary>
        /// Invoked on system application (un)focus
        /// </summary>
		public Action<bool> ApplicationFocus;

        /// <summary>
        /// Invoked before logger instance is created and after loggers where added
        /// </summary>
		public Action<LoggerFactory> AddingLoggers;

        /// <summary>
        /// Invoked on <see cref="CurrentState"/> change
        /// </summary>
		public Action<ApplicationStateEnum> StateChanged;

        /// <summary>
        /// Invoked on all <see cref="Screens"/> are populated
        /// </summary>
		public Action ScreensPopulated;

        /// <summary>
        /// Invoked on all <see cref="Overlays"/> are populated
        /// </summary>
		public Action OverlaysPopulated;

        /// <summary>
        /// Invoked on all <see cref="Modals"/> are populated
        /// </summary>
		public Action ModalsPopulated;

		#endregion

		#region Members

		#region Singleton

		public override void OnSingletonInstantiating()
		{
			if (IsInstantiated) return;
			Initialize();
		}

        /// <summary>
        /// Initializes instances of <see cref="Menu"/>, <see cref="Timer"/>, <see cref="Game"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown when <see cref="Meta"/> is null.</exception>
		private void Initialize()
		{
			if (Meta == null) throw new NullReferenceException(nameof(Meta));
			if (Menu == null) Menu = new Menu();
			if (Timer == null)
			{
				Timer = Timer.CreateAndStartTimer();
				Timer.AffectUnityTimeScale = Meta.SetTimeScaleOnPause;
			}
			if (Game == null) Game = GameObject.FindObjectOfType<Game>();
		}

		#endregion

        /// <summary>
        /// Current application metadata
        /// </summary>
		public AppMeta Meta;

        /// <summary>
        /// Custom level of debug information
        /// </summary>
		public int DebugLevel { get; set; }

        /// <summary>
        /// Instance of application logger
        /// </summary>
		public ILogger Logger { get; private set; }

        /// <summary>
        /// Instance of application timer
        /// </summary>
		public Timer Timer { get; private set; }

        /// <summary>
        /// Instance of application menu
        /// </summary>
		public Menu Menu { get; private set; }

        /// <summary>
        /// Instance of application game
        /// </summary>
		public Game Game { get; private set; }

        /// <summary>
        /// Prefab to <see cref="DebugConsole"/>
        /// </summary>
        public UnityEngine.Object DebugConsolePrefab;

        /// <summary>
        /// List of available <see cref="Game"/> metadata
        /// </summary>
		public List<GameMeta> GameMetas;

        /// <summary>
        /// List of available <see cref="Game"/> game mode metadata
        /// </summary>
		public List<GameModeMeta> GameModes;

        /// <summary>
        /// Current application state, default is <see cref="ApplicationStateEnum.Undefined"/>
        /// </summary>
		public ApplicationStateEnum CurrentState { get; private set; }

        /// <summary>
        /// Instance of <see cref="DebugConsole"/>
        /// </summary>
        public DebugConsole Console { get; set; }

        private readonly List<string> _scenesLoaded = new List<string>();
        private bool _defferMenuDeactivationToLevelLoaded;

        #endregion

        #region MonoBehaviour

        protected override void Awake()
		{
			Initialize();

			SceneManager.sceneLoaded += (scene, mode) =>
			{
				if (!_scenesLoaded.Contains(scene.name))
					_scenesLoaded.Add(scene.name);
			};

			SceneManager.sceneUnloaded += scene =>
			{
				_scenesLoaded.Remove(scene.name);
			};

            Game.LevelLoaded += (sceneLoaded) =>
            {
                if (_defferMenuDeactivationToLevelLoaded)
                {
                    _defferMenuDeactivationToLevelLoaded = false;
                    Menu.SetActive(false);
                }
            };

			base.Awake();
		}

		protected override void Start()
		{
			Instance.Timer.ResetTimer(false); // ensure reset

			base.Start();
		}

		protected override void OnEnable()
		{
			Instance.Timer.IsPaused = false; // ensure started

			InitLoggers();

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			Instance.Timer.IsPaused = true;

            // try to unload game
		    Game.SetActive(false, null, null);

			LoggerFactory.Instance.DisposeProviders();

			base.OnDisable();
		}

		protected override void Update()
		{
			Timer.Update();
		}

        protected override void FixedUpdate()
        {
            Timer.FixedUpdate();
        }

        #endregion

        #region App

        /// <summary>
        /// Activates <see cref="Menu"/> and deactivates <see cref="Game"/>.
        /// If successful, <see cref="StateChanged"/> is invoked and <see cref="CurrentState"/> is set to <see cref="ApplicationStateEnum.InMenu"/>.
        /// All scenes loaded from <see cref="App"/> start are unloaded.
        /// </summary>
        /// <returns>True if <see cref="Menu"/> is active</returns>
        public bool GoToMenu()
		{
			if (CurrentState == ApplicationStateEnum.InMenu) return false;

			CurrentState = ApplicationStateEnum.InMenu;
			StateChanged?.Invoke(CurrentState);

			Game.SetActive(false, null, null);
			Menu.SetActive(true);

			return true;
		}

        /// <summary>
        /// Activates <see cref="Game"/> and deactivates <see cref="Menu"/>.
        /// If successful, <see cref="StateChanged"/> is invoked and <see cref="CurrentState"/> is set to <see cref="ApplicationStateEnum.InGame"/>.
        /// </summary>
        /// <returns>True if <see cref="Game"/> is active</returns>
        /// <param name="meta">Instance of <see cref="GameMeta"/></param>
        /// <param name="gameMode">Instance of <see cref="GameModeMeta"/></param>
        /// <param name="defferMenuDeactivationToLevelLoaded">If true, menu is deactivated on scene loaded</param>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="meta"/> or <paramref name="gameMode"/> is empty or null.</exception>
		public bool GoToGame(GameMeta meta, GameModeMeta gameMode, bool defferMenuDeactivationToLevelLoaded = false)
		{
			if (CurrentState == ApplicationStateEnum.InGame) return false;

			if (GameMetas.IsNullOrEmpty()) throw new ArgumentException(Texts.Errors.NoGamesToRunFound, nameof(GameMetas));
			if (GameModes.IsNullOrEmpty()) throw new ArgumentException(Texts.Errors.NoGamesToRunFound, nameof(GameModes));

			CurrentState = ApplicationStateEnum.InGame;
			StateChanged?.Invoke(CurrentState);

			Game.SetActive(true, meta, gameMode);
            _defferMenuDeactivationToLevelLoaded = defferMenuDeactivationToLevelLoaded;
            if (!defferMenuDeactivationToLevelLoaded) Menu.SetActive(false);

            return true;
		}

        /// <summary>
        /// Pauses <see cref="Game"/> and activates <see cref="Menu"/> or vice versa.
        /// </summary>
        /// <param name="pauseStatus">True to pause <see cref="Game"/></param>
		public void SetGamePause(bool pauseStatus)
		{
			Game.OnGamePauseSet(pauseStatus);
			Menu.SetActive(pauseStatus);
		}

        /// <summary>
        /// Returns whether <see cref="Game"/> is paused.
        /// </summary>
        /// <returns>True if <see cref="Game"/> is paused</returns>
        public bool IsGamePaused()
        {
            return Game.Timer.IsPaused && CurrentState == ApplicationStateEnum.InGame;
        }

        /// <summary>
        /// Determines whether scene specified was loaded.
        /// </summary>
        /// <param name="sceneName">Specifies scene name</param>
        /// <returns></returns>
		public bool IsSceneLoaded(string sceneName)
		{
			return _scenesLoaded.Contains(sceneName);
		}

        /// <summary>
        /// Adds <see cref="DebugConsole"/> as a <see cref="Component"/> of <see cref="App"/> <see cref="GameObject"/>.
        /// Instantiates <see cref="DebugConsolePrefab"/>, adds it as a child of <see cref="GUI"/> and deactivates it.
        /// </summary>
        public void AddDebugConsole()
        {
            if (DebugConsolePrefab == null) throw new Exception(string.Format(Texts.Errors.ObjectCannotBeNull, nameof(DebugConsolePrefab)));

            var gui = GameObject.FindObjectOfType<GUI>();
            if (gui == null) throw new Exception(string.Format(Texts.Errors.ObjectCannotBeNull, nameof(GUI)));
            var go = Instantiate(DebugConsolePrefab, gui.transform) as GameObject;
            var rectTransform = go.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(0, rectTransform.sizeDelta.y);

            Console = gameObject.AddComponent<DebugConsole>();
            Console.Root = go.GetComponent<DebugConsoleRoot>();

            go.SetActive(false);
        }

        #endregion

        #region Unity Application

        private void OnApplicationPause(bool pauseStatus)
		{
			Timer.IsPaused = pauseStatus;

			ApplicationPaused?.Invoke(pauseStatus);
		}

		private void OnApplicationQuit()
		{
			ApplicationQuit?.Invoke();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			ApplicationFocus?.Invoke(hasFocus);
		}

		#endregion

		#region Logging

        /// <summary>
        /// Adds selected logs and creates <see cref="Logger"/> instance. Before creation an action <see cref="AddingLoggers"/> is invoked.
        /// </summary>
		private void InitLoggers()
		{
			LoggerFactory.Instance.AddUnityLog();
			AddingLoggers?.Invoke(LoggerFactory.Instance);

			Logger = LoggerFactory.Instance.CreateLogger();
		}

        #endregion

        #region Helpers

        /// <summary>
        /// Destroys <see cref="GameObject"/>.
        /// </summary>
        /// <param name="go"></param>
        public void DestroyGameObject(GameObject go)
        {
            Destroy(go);
        }

        #endregion
    }
}
