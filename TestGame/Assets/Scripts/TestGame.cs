using System;
using System.Collections.Generic;
using Elia.Unity.Components.Games;
using Elia.Unity.Modules;
using UnityEngine;

public class TestGame : Game
{
    protected override void Awake()
    {
        base.Awake();

        LevelLoaded += (loadedScene) =>
        {
            App.Instance.Game.BeginLevel();

            var tutorialStepIndex = PrefsStorage.Instance.GetValue<int>(PrefsKeys.TutorialStepIndex, -1);
            if (tutorialStepIndex < int.MaxValue)
            {
                Tutorial.Begin();
            }
        };

        CreateTutorial<Tutorial>();
        Tutorial.SetSteps(new List<TutorialStep>()
        {
            new TestGameTutorialStep1(Tutorial),
            new TestGameTutorialStep2(Tutorial)
        });

		Cheats.AddCheat(new int[] { 0, 0, 0 }, "cheat1", () =>
		{
			App.Instance.Logger.Log("cheat1 activated");
		});
    }
}
