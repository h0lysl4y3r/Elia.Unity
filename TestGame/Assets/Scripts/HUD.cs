using System;
using Elia.Unity.Components.GUI;
using Elia.Unity.Helpers;
using Elia.Unity.Modules;
using UnityEngine;
using UnityEngine.UI;

public class HUD : Overlay
{
	public GameObject LeaveButton;
	public GameObject PauseButton;
    public Text ScoreText;
    public Text TimeText;
    public Text LevelText;

    protected override void Awake()
    {
        base.Awake();

        App.Instance.Game.LevelLoaded += OnLevelLoaded;
    }

    protected override void Update()
    {
        base.Update();

        UpdateHUD();
    }

    public void GoToMenu()
	{
		App.Instance.GoToMenu();
	}

	public void ShowPauseScreen()
	{
		App.Instance.SetGamePause(true);
	}

    private void OnLevelLoaded(bool sceneLoaded)
    {
	    App.Instance.Game.ScoreChanged += (scoreName) =>
	    {
		    UpdateHUD();
	    };

		ScoreText.text = "Score: 0";
        TimeText.text = "Time: 00:00";
        LevelText.text = "Level: 1";
    }

    public void UpdateHUD()
    {
	    var game = App.Instance.Game;
		ScoreText.text = "Score: " + game.Score1.ToString();
		TimeText.text = "Time: " + Formatting.FormatTime(game.Timer.GameTime);
        if (game.Level != null) LevelText.text = "Level: " + (game.Level.State.CurrentLevelId + 1).ToString();
    }
}
