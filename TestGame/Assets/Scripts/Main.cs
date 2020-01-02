using System;
using Elia.Unity.Helpers;
using UnityEngine;
using Elia.Unity.Modules;

public class Main : MonoBehaviour
{
	public App App;

	private void Start()
	{
		PrefsStorage.Instance.Initialize(App.Instance.Meta.Id, new JsonPrefsSerializer(), false, null, null);

        App.Menu.FirstActiveScreenType = typeof(MainMenuScreen);
		App.Menu.PausedActiveScreenType = typeof(PauseScreen);
		App.ScreensPopulated += () =>
		{
			App.GoToMenu();
		};
	}
}
