using Elia.Unity.Components.Games;
using Elia.Unity.Modules;
using System.Collections;
using UnityEngine;

public class TestGameLevel : Level
{
    public override bool Begin()
	{
		if (!base.Begin()) return false;

        Overlays.Instance.SetOverlayActive<HUD>(true);
        Overlays.Instance.GetOverlay<HUD>().UpdateHUD();

        BeginMission<MissionHitThree>("Hit Three");

		return true;
    }

    public override bool End()
	{
		if (!base.End()) return false;

		Overlays.Instance.SetOverlayActive<HUD>(false);

		return true;
	}

    protected override void OnMissionEnded()
    {
        base.OnMissionEnded();

		if (!Game.IsActive) return;

        if (State.CurrentLevelId % 2 == 1) BeginMission<MissionHitThree>("Hit Three");
    }
}
