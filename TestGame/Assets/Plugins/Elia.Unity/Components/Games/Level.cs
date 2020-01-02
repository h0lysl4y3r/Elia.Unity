using System;
using Elia.Unity.Modules;
using UnityEngine.SceneManagement;

namespace Elia.Unity.Components.Games
{
    /// <summary>
    /// Level is usually connected with Unity scene. 
    /// </summary>
	public abstract class Level : BehaviourAware
	{
        #region Actions

        /// <summary>
        /// Invoked within <see cref="LoadScene"/> method on scene loaded by <see cref="SceneManager"/> or if already loaded on scene activated.
        /// </summary>
        public Action<bool> LevelLoaded;

        /// <summary>
        /// Invoked within <see cref="UnloadScene"/> method on scene unloaded by <see cref="SceneManager"/> or if already unloaded immediately.
        /// </summary>
        public Action<bool> LevelUnloaded;

        #endregion

        #region Members

        /// <summary>
        /// True if <see cref="Level"/> was initialized
        /// </summary>
        public bool HasBegun { get; private set; }

        /// <summary>
        /// True if <see cref="Level"/> was finalized
        /// </summary>
		public bool HasEnded { get; private set; }

        /// <summary>
        /// Instance of <see cref="LevelState"/> that is stored in <see cref="PrefsStorage"/>
        /// </summary>
		public LevelState State { get; set; }

        /// <summary>
        /// Instance of <see cref="Game"/> that is assigned from it
        /// </summary>
		public Game Game { get; set; }

        /// <summary>
        /// Instance of <see cref="Mission"/> that is current in the <see cref="Level"/>
        /// </summary>
        public Mission Mission { get; private set; }

        #endregion

        #region MonoBehaviour

        protected override void Update()
        {
            if (!HasBegun && !HasEnded) return;
            if (HasEnded) return;

            base.Update();

            Mission?.Update();
        }

        #endregion

        #region Management

        /// <summary>
        /// Used to initialize <see cref="Level"/>. Sets <see cref="HasBegun"/> to true.
        /// </summary>
		/// <returns>True if successful</returns>
        public virtual bool Begin()
		{
            if (HasBegun) return false;

			HasBegun = true;
            HasEnded = false;

			return true;
		}

		/// <summary>
		/// Used to finalize <see cref="Level"/>. Sets <see cref="HasBegun"/> to false.
		/// </summary>
		/// <returns>True if successful</returns>
		public virtual bool End()
		{
            if (HasEnded) return false;

            HasBegun = false;
            HasEnded = true;

            // If ending via Level, assuming that mission failed
            Mission?.End(false);

			return true;
		}

		/// <summary>
		/// Reacts to higher level component changed pause status.
		/// </summary>
		/// <param name="pauseStatus">Pause status</param>
		/// <returns>True if successful</returns>
		public virtual bool Pause(bool pauseStatus)
		{
            if (!HasBegun && !HasEnded) return false;
            if (HasEnded) return false;
            
			Mission?.Pause(pauseStatus);

			return true;
		}

        #endregion

        #region Mission

        /// <summary>
        /// Begins new <see cref="Mission"/>.
        /// </summary>
        /// <typeparam name="T">Mission type param</typeparam>
        /// <param name="missionName">Name of mission</param>
        /// <exception cref="Exception">Thrown if <see cref="Mission"/> is non-null and has not ended</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="missionName"/> is null</exception>
        public void BeginMission<T>(string missionName)
            where T : Mission
        {
            if (missionName == null) throw new ArgumentNullException(nameof(missionName));
            if (Mission != null && !Mission.HasEnded) throw new Exception(Texts.Errors.MissionHasNotEnded);

            Mission = Activator.CreateInstance(typeof(T), missionName, this, new Action(OnMissionEnded)) as T;
            Mission.Begin();
        }

        /// <summary>
        /// Callback invoked when <see cref="Mission.End"/> is called method.
        /// </summary>
        protected virtual void OnMissionEnded()
        {

        }

        #endregion

        #region Scenes

        /// <summary>
        /// Loades scene at <see cref="State.CurrentSceneId"/> from <see cref="Game"/> scene list.
        /// If scene is loaded, activates it via <see cref="SceneManager"/>.
        /// Invokes <see cref="LevelLoaded"/> action.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown if <see cref="State.CurrentSceneId"/> points to non-existing scene</exception>
        public virtual void LoadScene()
		{
			var currentSceneName = Game.GetSceneName(State.CurrentSceneId);
			if (currentSceneName == null) throw new NullReferenceException(nameof(currentSceneName));

			SceneManager.sceneLoaded += (scene, sceneMode) =>
			{
				if (currentSceneName == scene.name)
				{
					LevelLoaded?.Invoke(true);
				}
			};

			if (!Modules.App.Instance.IsSceneLoaded(currentSceneName))
			{
				SceneManager.LoadSceneAsync(currentSceneName);
			}
			else
			{
				var scene = SceneManager.GetSceneByName(currentSceneName);
				SceneManager.SetActiveScene(scene);
				LevelLoaded?.Invoke(false);
			}
		}

        #endregion
    }
}
