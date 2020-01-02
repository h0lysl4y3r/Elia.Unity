using System;
using System.Collections;
using Elia.Unity.Components.App;
using Elia.Unity.Components.Debug;
using Elia.Unity.Components.Games;
using Elia.Unity.Serialization;
using UnityEngine;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Game is a basic gaming module that provides game state management.
    /// Game wraps all game related entities as <see cref="Level"/>, <see cref="GameModeMeta"/>, <see cref="GameMeta"/>.
    /// </summary>
	public abstract class Game : BehaviourAwareSingleton<Game>
	{
		#region Actions

        /// <summary>
        /// Invoked on <see cref="Level"/> scene loaded.
        /// </summary>
		public Action<bool> LevelLoaded;

        /// <summary>
        /// Invoked within <see cref="EndLevel"/> method before <see cref="Level.End"/> method is called.
        /// </summary>
		public Action LevelEnding;

        /// <summary>
        /// Invoked within <see cref="EndLevel"/> method after <see cref="Level.End"/> and <see cref="SaveState"/> methods are called.
        /// </summary>
		public Action LevelEnded;

        /// <summary>
        /// Invoked within <see cref="EndLevel"/> method after <see cref="Level.End"/> method and before <see cref="SaveState"/> method is called.
        /// </summary>
		public Action<LevelState> SavingLevelState;

        /// <summary>
        /// Invoked within <see cref="DestroyLevelAsync"/> method before <see cref="Level"/> is destroyed.
        /// </summary>
		public Action LevelDestroying;

        /// <summary>
        /// Invoked within <see cref="DestroyLevelAsync"/> method after <see cref="Level"/> is destroyed.
        /// </summary>
		public Action LevelDestroyed;

        /// <summary>
        /// Invoked on <see cref="Score1"/> or <see cref="Score2"/> or <see cref="Score3"/> or <see cref="Score4"/> is changed.
        /// </summary>
		public Action<string> ScoreChanged;

		#endregion

		#region Members

        /// <summary>
        /// Key string that is used to store <see cref="LevelState"/> to <see cref="PrefsStorage"/>.
        /// </summary>
		public const string LevelStateKey = "LevelState";

        /// <summary>
        /// Invoked within <see cref="OnDeactivatedAsync"/> method and <see cref="WaitUntil"/> coroutine. 
        /// Function that intercepts <see cref="Game"/> deactivation before <see cref="EndLevel"/> and <see cref="DestroyLevelAsync"/> methods are called.
        /// </summary>
		public static Func<bool> DeactivateGameFn = () => true;

        /// <summary>
        /// Determines whether <see cref="Game"/> is active. Is set in <see cref="SetActive"/> method.
        /// </summary>
		public bool IsActive { get; private set; }

        /// <summary>
        /// Instance of <see cref="Timer"/> module.
        /// </summary>
		public Timer Timer { get; private set; }

        /// <summary>
        /// Instance of <see cref="GameMeta"/> object.
        /// </summary>
		public GameMeta Meta { get; private set; }

        /// <summary>
        /// Instance of <see cref="GameModeMeta"/> object.
        /// </summary>
		public GameModeMeta GameMode { get; private set; }

        /// <summary>
        /// Instance of <see cref="Level"/> object.
        /// </summary>
		public Level Level { get; private set; }

        /// <summary>
        /// Instance of <see cref="Tutorial"/> object.
        /// </summary>
        public Tutorial Tutorial { get; private set; }

		/// <summary>
		/// Instance of <see cref="Cheats"/> objects.
		/// </summary>
		public Cheats Cheats { get; private set; }

		/// <summary>
		/// Time delay (in seconds) before <see cref="Level.End"/> method is called.
		/// </summary>
		public float LevelEndDelay { get; set; }

        /// <summary>
        /// Sets/gets score type 1. Invokes <see cref="ScoreChanged"/> action.
        /// </summary>
		public double Score1
		{
			get { return _score1; }
			set
			{
				var prevValue = _score1;
				_score1 = value;
				if (prevValue != _score1)
				{
					ScoreChanged?.Invoke(Meta.Score1Name);
				}
			}
		}
		protected double _score1;

        /// <summary>
        /// Sets/gets score type 2. Invokes <see cref="ScoreChanged"/> action.
        /// </summary>
		public double Score2
		{
			get { return _score2; }
			set
			{
				var prevValue = _score2;
				_score2 = value;
				if (prevValue != _score2)
				{
					ScoreChanged?.Invoke(Meta.Score2Name);
				}
			}
		}
		protected double _score2;

        /// <summary>
        /// Sets/gets score type 3. Invokes <see cref="ScoreChanged"/> action.
        /// </summary>
		public double Score3
		{
			get { return _score3; }
			set
			{
				var prevValue = _score3;
				_score3 = value;
				if (prevValue != _score3)
				{
					ScoreChanged?.Invoke(Meta.Score3Name);
				}
			}
		}
		protected double _score3;

        /// <summary>
        /// Sets/gets score type 4. Invokes <see cref="ScoreChanged"/> action.
        /// </summary>
		public double Score4
		{
			get { return _score4; }
			set
			{
				var prevValue = _score4;
				_score4 = value;
				if (prevValue != _score4)
				{
					ScoreChanged?.Invoke(Meta.Score4Name);
				}
			}
		}
		protected double _score4;

        /// <summary>
        /// Sets/gets currency type 1.
        /// </summary>
		public double Currency1 { get; set; }

        /// <summary>
        /// Sets/gets currency type 2.
        /// </summary>
		public double Currency2 { get; set; }

        /// <summary>
        /// Sets/gets currency type 3.
        /// </summary>
		public double Currency3 { get; set; }

        /// <summary>
        /// Sets/gets currency type 4.
        /// </summary>
		public double Currency4 { get; set; }

		#endregion

		#region Singleton

        private void Initialize()
        {
            if (Timer == null)
            {
                Timer = new Timer
                {
                    IsPaused = true,
                    AffectUnityTimeScale = App.Instance.Meta.SetTimeScaleOnPause
                };
            }

            if (Cheats == null) Cheats = new Cheats(App.Instance.Meta, Timer);
        }

        #endregion

        #region MonoBehaviour

        protected override void Awake()
		{
			base.Awake();

            Initialize();

            // SavingLevelState is invoked in EndLevel()
            SavingLevelState += levelState => UpdateGameAndState(false, levelState);
		}

        protected override void Update()
        {
            base.Update();

            Timer.Update();
        }

        protected override void FixedUpdate()
		{
			base.FixedUpdate();

			Timer.FixedUpdate();
		}

        #endregion

        #region Management

        /// <summary>
        /// Activates <see cref="Game"/>. Sets <see cref="IsActive"/> property.
        /// If <see cref="IsActive"/> is true, <see cref="OnActivated"/> method is called.
        /// If <see cref="IsActive"/> is false, <see cref="OnDeactivatedAsync"/> method is called. 
        /// </summary>
        /// <param name="value">True to activate <see cref="Game"/></param>
        /// <param name="meta">Object of <see cref="GameMeta"/> type</param>
        /// <param name="gameMode">Object of <see cref="GameModeMeta"/> type</param>
        /// <returns>True is (de)activation succeeds.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="meta"/> or <paramref name="gameMode"/> is null.</exception>
        public bool SetActive(bool value, GameMeta meta, GameModeMeta gameMode)
		{
			if (value == IsActive) return false;

			if (value)
			{
				if (meta == null) throw new ArgumentNullException(nameof(meta));
				if (gameMode == null) throw new ArgumentNullException(nameof(gameMode));
			}

			IsActive = value;

			if (IsActive)
			{
				OnActivated(meta, gameMode);
			}
			else
			{
				StartCoroutine(OnDeactivatedAsync());
			}

			return true;
		}

        /// <summary>
        /// Initialize <see cref="Game"/> modules and objects on activated. Called within <see cref="SetActive"/> method.
        /// </summary>
        /// <param name="meta">Object of <see cref="GameMeta"/> type</param>
        /// <param name="gameMode">Object of <see cref="GameModeMeta"/> type</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="meta"/> cannot be found in <see cref="App.Instance.GameMetas"/> or if <paramref name="gameMode"/> cannot be found in <see cref="App.Instance.GameModes"/></exception>
		protected virtual void OnActivated(GameMeta meta, GameModeMeta gameMode)
		{
			if (!App.Instance.GameMetas.Contains(meta)) throw new ArgumentException(Texts.Errors.CollectionDoesNotContainItem, nameof(meta));
			if (!App.Instance.GameModes.Contains(gameMode)) throw new ArgumentException(Texts.Errors.CollectionDoesNotContainItem, nameof(gameMode));

			App.Instance.Timer.Paused += OnAppTimerPaused;

			Meta = meta;
			GameMode = gameMode;
		}

        /// <summary>
        /// De-initializes <see cref="Game"/> upon deactivation. Called within <see cref="SetActive"/> method.
        /// Calls <see cref="EndLevel"/> and <see cref="DestroyLevelAsync"/> methods.
        /// </summary>
        /// <returns>Coroutine's enumerator</returns>
		protected virtual IEnumerator OnDeactivatedAsync()
		{
			App.Instance.Timer.Paused -= OnAppTimerPaused;

			if (DeactivateGameFn != null)
			{
				yield return new WaitUntil(DeactivateGameFn);
			}

			StartCoroutine(EndLevel());
			StartCoroutine(DestroyLevelAsync());
		}

        /// <summary>
        /// Adds <see cref="Level"/> to <see cref="Game"/> and loades its scenes. Sets <see cref="Level.LevelState"/> and calls <see cref="UpdateGameAndState"/> method.
        /// </summary>
        /// <typeparam name="TLevelState"><see cref="LevelState"/> derived type</typeparam>
        /// <typeparam name="TLevel"><see cref="Level"/> derived type</typeparam>
        /// <param name="defaultLevelStateFn">Optional function invoked when no level state was found in <see cref="PrefsStorage"/></param>
        /// <param name="onLevelStateLoaded">Optional action invoked after <see cref="UpdateGameAndState"/> method is called</param>
        /// <exception cref="ArgumentNullException">Thrown when no <see cref="LevelState"/> could be acquired</exception>
		public virtual void LoadLevel<TLevelState, TLevel>(Func<TLevelState> defaultLevelStateFn = null, Action<TLevelState> onLevelStateLoaded = null)
			where TLevelState : LevelState
			where TLevel : Level
		{
			var key = PrefsStorage.Instance.GetTypePrefixedKey<TLevelState>(LevelStateKey);
			var levelState = defaultLevelStateFn != null ? defaultLevelStateFn.Invoke() : PrefsStorage.Instance.GetValue<TLevelState>(key);
			if (levelState == null) throw new ArgumentNullException(nameof(levelState));

			UpdateGameAndState(true, levelState);
			onLevelStateLoaded?.Invoke(levelState);

			Level = gameObject.AddComponent<TLevel>();
			Level.Game = this;
			Level.State = levelState;
			Level.LevelLoaded += OnLevelLoaded;
			Level.LoadScene();
		}

        /// <summary>
        /// Callback method on <see cref="Level.LevelLoaded"/> action.
        /// </summary>
        /// <param name="sceneLoaded">Indicates whether scene was currently loaded or was already loaded</param>
		protected virtual void OnLevelLoaded(bool sceneLoaded)
		{
			LevelLoaded?.Invoke(sceneLoaded);
		}

        /// <summary>
        /// Waits for <see cref="Level.HasEnded"/> is true and destroys <see cref="Level"/>.
        /// Should be always called after <see cref="EndLevel"/> method.
        /// </summary>
        /// <returns>Coroutine's enumerator</returns>
        public IEnumerator DestroyLevelAsync()
		{
			if (Level == null) yield break;
			yield return new WaitUntil(() => Level.HasEnded);

			Level.LevelLoaded -= OnLevelLoaded;

			LevelDestroying?.Invoke();

			Destroy(Level);
			Level = null;

			LevelDestroyed?.Invoke();
		}

        /// <summary>
        /// If <see cref="Level.HasBegun"/> is false, it calls <see cref="Level.Begin"/> and <see cref="Timer.StartTimer"/> methods.
        /// Method should be always called after <see cref="LevelLoaded"/> was invoked.
        /// </summary>
		public void BeginLevel()
		{
			if (Level.HasBegun) return;

			Level.Begin();

			Timer.StartTimer();
		}

        /// <summary>
        /// Invokes <see cref="LevelEnding"/> action and ends <see cref="Level"/> and saves <see cref="Game"/>. 
        /// Finally invokes <see cref="LevelEnded"/> action.
        /// Should be always called before <see cref="DestroyLevelAsync"/> method.
        /// If <see cref="Level"/> is null or <see cref="Level.HasEnded"/> is true, does nothing.
        /// </summary>
        /// <returns>Coroutine's enumerator</returns>
        public IEnumerator EndLevel()
		{
			if (Level == null || Level.HasEnded) yield break;

			Timer.IsPaused = true;

			LevelEnding?.Invoke();

			if (LevelEndDelay > 0f)
				yield return new WaitForSecondsRealtime(LevelEndDelay);

			Level.End();
			SavingLevelState?.Invoke(Level.State);
			SaveState();

			LevelEnded?.Invoke();
		}

        /// <summary>
        /// Returns name of scene at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of scene in <see cref="GameMode"/> scene name list</param>
        /// <returns>Name of scene at <paramref name="index"/></returns>
		public string GetSceneName(int index)
		{
			if (index < 0 || index >= GameMode.SceneNames.Count) return null;
			return GameMode.SceneNames[index];
		}

        /// <summary>
        /// Creates tutorial instance of <typeparamref name="T"/> type.
        /// </summary>
        public void CreateTutorial<T>()
            where T : Tutorial
        {
            Tutorial = Activator.CreateInstance(typeof(T), new[] { this }) as T;
        }

        #endregion

        #region Game

        /// <summary>
        /// Saves <see cref="LevelState"/> to <see cref="PrefsStorage"/>.
        /// When calling the method <see cref="Level"/> must exist.
        /// </summary>
        public virtual void SaveState()
		{
			var key = PrefsStorage.Instance.GetTypePrefixedKey(LevelStateKey, Level.State);
			PrefsStorage.Instance.SetValue(key, Level.State);
		}

        /// <summary>
        /// Loads instance of <see cref="LevelState"/> subclass given by param <typeparamref name="T"/> from <see cref="PrefsStorage"/>.
        /// </summary>
        /// <typeparam name="T">Prefs type param</typeparam>
        /// <returns>Instance of <see cref="LevelState"/> subclass</returns>
        public virtual T LoadState<T>()
        {
            var key = PrefsStorage.Instance.GetTypePrefixedKey<T>(LevelStateKey);
            return PrefsStorage.Instance.GetValue<T>(key);
        }

        /// <summary>
        /// Manages <see cref="Game"/> response to <see cref="App"/> pause status change. Passes the status to <see cref="Level"/>.
        /// </summary>
        /// <param name="pauseStatus">Current pause status</param>
		public virtual void OnGamePauseSet(bool pauseStatus)
		{
			Timer.IsPaused = pauseStatus;
			Level?.Pause(pauseStatus);

			Modals.Instance?.NotifyGamePaused(pauseStatus);
			Overlays.Instance?.NotifyGamePaused(pauseStatus);
			Screens.Instance?.NotifyGamePaused(pauseStatus);
		}

        /// <summary>
        /// Updates <see cref="Game"/> properties from <see cref="LevelState"/> or vice versa. Properties included:
        /// <see cref="Score1"/>, <see cref="Score2"/>, <see cref="Score3"/>, <see cref="Score4"/>,
        /// <see cref="Currency1"/>, <see cref="Currency2"/>, <see cref="Currency3"/>, <see cref="Currency4"/>.
        /// Also updates/resets timer.
        /// </summary>
        /// <param name="updateGame">True to update <see cref="Game"/> from <see cref="LevelState"/> and reset timer, false to update <see cref="LevelState"/> from <see cref="Game"/> and from <see cref="Timer"/></param>
        /// <param name="levelState"><see cref="LevelState"/> to update to/from</param>
		private void UpdateGameAndState(bool updateGame, LevelState levelState)
		{
			if (updateGame)
			{
				Score1 = levelState.Score1;
				Score2 = levelState.Score2;
				Score3 = levelState.Score3;
				Score4 = levelState.Score4;
				Currency1 = levelState.Currency1;
				Currency2 = levelState.Currency2;
				Currency3 = levelState.Currency3;
				Currency4 = levelState.Currency4;
				Timer.ResetTimer(false);
				Timer.OffsetTime(levelState.Time, 0, 0);
			}
			else
			{
				levelState.Score1 = Score1;
				levelState.Score2 = Score2;
				levelState.Score3 = Score3;
				levelState.Score4 = Score4;
				levelState.Currency1 = Currency1;
				levelState.Currency2 = Currency2;
				levelState.Currency3 = Currency3;
				levelState.Currency4 = Currency4;
				levelState.Time = Timer.GameTimeFloat;
			}
		}

        #endregion

        #region App

        /// <summary>
        /// Callback on <see cref="App.Instance.Timer.Paused"/> action.
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
        private void OnAppTimerPaused(bool pauseStatus)
		{
			OnGamePauseSet(pauseStatus);
		}

		#endregion
	}
}
