using System;
using System.Linq;
using Elia.Unity.Modules;
using UnityEngine;
using Screen = Elia.Unity.Components.GUI.Screen;

public class MainMenuScreen : Screen
{
    protected override void Awake()
    {
        base.Awake();

        App.Instance.AddDebugConsole();
        App.Instance.Console.ToggleConsoleKey = KeyCode.F1;
    }

    public void GoToGame()
	{
		App.Instance.GoToGame(App.Instance.GameMetas.First(), App.Instance.GameModes.First());
		if (App.Instance.Game.IsActive)
		{
            var state = App.Instance.Game.LoadState<TestGameLevelState>() ?? new TestGameLevelState();
			App.Instance.Game.LoadLevel<TestGameLevelState, TestGameLevel>(() => { return state; });
		}
	}

	public void GoToOptions()
	{
		Screens.Instance.SetScreenActive<OptionsScreen>(true);
	}

	public void ShowTestModal()
	{
		Modals.Instance.SetModalActive<TestModal>(true);
	}
}
