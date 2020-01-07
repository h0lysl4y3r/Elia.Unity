using System;
using Elia.Unity.Helpers;
using UnityEngine;
using Elia.Unity.Modules;
using Elia.Unity.Components.Analytics;
using Firebase.Analytics;

public class Main : MonoBehaviour
{
	public App App;

	private void Start()
	{
		PrefsStorage.Instance.Initialize(App.Instance.Meta.Id, new JsonPrefsSerializer(), false, null, null);

		Analytics.Instance.ProviderInitialized += (provider) =>
		{
			Analytics.Instance.LogEvent(FirebaseAnalytics.EventSearch, new[] { new Tuple<string, object>("term", "test") });
		};
		Analytics.Instance.AddProvider<FirebaseProvider>();

        App.Menu.FirstActiveScreenType = typeof(MainMenuScreen);
		App.Menu.PausedActiveScreenType = typeof(PauseScreen);
		App.ScreensPopulated += () =>
		{
			App.GoToMenu();
		};
	}
}
