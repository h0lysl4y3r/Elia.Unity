using Elia.Unity.Components.GUI;
using Elia.Unity.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialOverlay : Overlay
{
    public GameObject Step1Go;
    public GameObject Step2Go;

    public void OnStep1ClickToContinue()
    {
        App.Instance.Game.Tutorial.Run();
    }

    public void OnStep2ClickToContinue()
    {
        App.Instance.Game.Tutorial.Run();
    }

    public void UpdateVisibility(int index)
    {
        Step1Go.SetActive(false);
        Step2Go.SetActive(false);
        if (index == 0) Step1Go.SetActive(true);
        if (index == 1) Step2Go.SetActive(true);
    }
}
