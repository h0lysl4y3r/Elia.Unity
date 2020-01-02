using System;
using System.Collections;
using Elia.Unity.Components.GameObjects;
using Elia.Unity.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Elia.Unity.Modules
{
	/// <summary>
	/// Bootstraps an Unity application.
	/// Set IntroStartTimes to as many intros as needed.
	/// Use IntroActions to react on scheduled intro start time.
	/// Set SceneNames that should be loaded after intros are finished. Second (and others) scene is loaded additively.
	/// One of the scenes should contain App module.
    /// Starts automatically on first Update.
	/// </summary>
	[AddComponentMenu("ELIA/Modules/Bootstrap")]
	public sealed class Bootstrap : BehaviourAwareSingleton<Bootstrap>
	{
		#region Actions

        /// <summary>
        /// Invoked when last intro (based on provided times) finishes.
        /// </summary>
		public Action LastIntroFinished;

		#endregion

		#region Members

        /// <summary>
        /// Array of scenes to load upon intros end
        /// </summary>
		public string[] SceneNames;

        #region Intros

        /// <summary>
        /// Actions to invoke in context of intro scene load, should have the same element count as <see cref="IntroStartTimes"/>
        /// </summary>
        public Action<int>[] IntroActions { get; private set; }

        /// <summary>
        /// Array of time delays (in seconds) between intro scene loades, should have the same element count as <see cref="IntroActions"/>
        /// </summary>
		public float[] IntroStartTimes;

        /// <summary>
        /// Time delay (in seconds) after last intro scene and before invokation of <see cref="LastIntroFinished"/>
        /// </summary>
		public float LastIntroDuration;

        /// <summary>
        /// True to ignore <see cref="IntroStartTimes"/>. Intros will run alongside scenes defined at <see cref="SceneNames"/> (which will be loaded immediately).
        /// </summary>
		public bool LoadScenesSimultaneously;

		private bool _introsStarted;

		#endregion

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
			if (!IntroStartTimes.IsNullOrEmpty())
				IntroActions = new Action<int>[IntroStartTimes.Length];

			EnsureBasicModules();

            if (SceneNames != null && SceneNames.Length > 0)
            {
                SceneManager.sceneLoaded += (scene, mode) =>
                {
                    EnsureGuiModules();
                };
            }
            else
            {
                EnsureGuiModules();
            }

			base.Awake();
		}

        /// <summary>
        /// Checks for existence and ensures basic modules such as <see cref="App"/>, <see cref="Game"/>, <see cref="Inputs"/>, <see cref="PrefsStorage"/>.
        /// </summary>
        /// <exception cref="Exception">Thrown if modules cannot be found</exception>
		private void EnsureBasicModules()
		{
			if (GetComponent<DontDestroy>() == null) gameObject.AddComponent<DontDestroy>();

			var app = GameObject.FindObjectOfType<App>();
			if (app == null) throw new Exception("No App found.");
			if (app.GetComponent<DontDestroy>() == null) app.gameObject.AddComponent<DontDestroy>();

			var game = GameObject.FindObjectOfType<Game>();
			if (game == null) throw new Exception("No Game found.");
			if (game.GetComponent<DontDestroy>() == null) game.gameObject.AddComponent<DontDestroy>();

			var inputs = GameObject.FindObjectOfType<Inputs>();
			if (inputs == null)
			{
				var inputsGo = new GameObject("inputs");
				inputs = inputsGo.AddComponent<Inputs>();
				inputsGo.AddComponent<DontDestroy>();				
			}
			if (inputs.GetComponent<DontDestroy>() == null) inputs.gameObject.AddComponent<DontDestroy>();

			var storage = GameObject.FindObjectOfType<PrefsStorage>();
			if (storage == null)
			{
				var storageGo = new GameObject("storage");
				storage = storageGo.AddComponent<PrefsStorage>();
				storageGo.AddComponent<DontDestroy>();
			}
			if (storage.GetComponent<DontDestroy>() == null) storage.gameObject.AddComponent<DontDestroy>();
        }

        /// <summary>
        /// Checks for existence and ensures GUI modules such as <see cref="EventSystem"/>.
        /// </summary>
		private void EnsureGuiModules()
		{
			var gui = GameObject.FindObjectOfType<GUI>();
			if (gui != null)
			{
				if (gui.gameObject.GetComponent<DontDestroy>() == null)
					gui.gameObject.AddComponent<DontDestroy>();
			}

			var eventSystem = GameObject.FindObjectOfType<EventSystem>();
			if (eventSystem != null)
			{
				if (eventSystem.gameObject.GetComponent<DontDestroy>() == null)
					eventSystem.gameObject.AddComponent<DontDestroy>();
			}
		}

		protected override void Update()
		{
			if (!_introsStarted)
			{
				StartIntrosAndLoadScenes();
				_introsStarted = true;
			}
		}

        #endregion

        #region Intros

        /// <summary>
        /// Calls in sequence <see cref="InvokeIntroActions"/>, <see cref="OnLastIntroFinished"/> and <see cref="LoadScenesAsync"/> methods.
        /// </summary>
        private void StartIntrosAndLoadScenes()
		{
			float totalTime = 0;

			if (!IntroStartTimes.IsNullOrEmpty())
			{
				for (var i = 0; i < IntroStartTimes.Length; i++)
				{
					var introTime = IntroStartTimes[i];
					StartCoroutine(InvokeIntroActions(introTime, i));
					totalTime += introTime;
				}
			}

			totalTime += LastIntroDuration;
			StartCoroutine(OnLastIntroFinished(totalTime));

			if (LoadScenesSimultaneously) totalTime = 0;
			StartCoroutine(LoadScenesAsync(totalTime));
		}

        /// <summary>
        /// Calls an action at <paramref name="introIndex"/> index defined in <see cref="IntroActions"/> array
        /// </summary>
        /// <param name="time">Time delay (in seconds) to invoke the action</param>
        /// <param name="introIndex">Index in IntroActions array</param>
        /// <returns></returns>
		private IEnumerator InvokeIntroActions(float time, int introIndex)
		{
			yield return new WaitForSecondsRealtime(time);

			IntroActions[introIndex].Invoke(introIndex);
		}

        /// <summary>
        /// Invokes action bound to <see cref="LastIntroFinished"/>.
        /// </summary>
        /// <param name="time">Time delay (in seconds) to invoke the action</param>
        /// <returns></returns>
		private IEnumerator OnLastIntroFinished(float time)
		{
			yield return new WaitForSecondsRealtime(time);

			LastIntroFinished?.Invoke();
		}

        #endregion

        #region Scenes

        /// <summary>
        /// Helper to load all <see cref="SceneNames"/>.
        /// </summary>
        /// <param name="time">Time delay (in seconds) to invoke the action</param>
        /// <returns></returns>
        private IEnumerator LoadScenesAsync(float time)
		{
            if (SceneNames == null) yield break;
			yield return new WaitForSecondsRealtime(time);

			for (var i = 0; i < SceneNames.Length; i++)
			{
				var sceneName = SceneNames[i];
				SceneManager.LoadScene(sceneName, i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive);
			}
		}

		#endregion
	}
}
