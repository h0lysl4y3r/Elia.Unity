using Elia.Unity.Components.Games;
using Elia.Unity.Modules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameTutorialStep2 : TutorialStep
{
    public TestGameTutorialStep2(Tutorial tutorial) : base(tutorial)
    {
    }

    public override void OnStarted()
    {
        base.OnStarted();

        PrefsStorage.Instance.SetValue<int>(PrefsKeys.TutorialStepIndex, Tutorial.CurrentStepIndex);
        Overlays.Instance.SetOverlayActive<TutorialOverlay>(true);
        Overlays.Instance.GetOverlay<TutorialOverlay>().UpdateVisibility(Tutorial.CurrentStepIndex);

        App.Instance.Logger.Log("Tutorial step 2 started");
    }

    public override void OnFinished()
    {
        base.OnFinished();

        PrefsStorage.Instance.SetValue<int>(PrefsKeys.TutorialStepIndex, int.MaxValue);
        Overlays.Instance.SetOverlayActive<TutorialOverlay>(false);
        Overlays.Instance.GetOverlay<TutorialOverlay>().UpdateVisibility(Tutorial.CurrentStepIndex);

        App.Instance.Logger.Log("Tutorial step 2 finished");
    }
}
