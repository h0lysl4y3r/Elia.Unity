using Elia.Unity.Components.Games;
using Elia.Unity.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionHitThree : Mission
{
    private GameObject _models;
    private int _objectCount;
    private int _hits;

    public MissionHitThree(string name, Level level, Action missionEnded) : base(name, level, missionEnded)
    {
    }

    public override void Begin()
    {
        if (HasBegun) return;

        base.Begin();

        _models = GameObject.Find("models");

        App.Instance.Logger.Log($"Mission {Name} began");
    }

    public override void End(bool succeeded)
    {
        if (HasEnded) return;

		var success = succeeded ? "success" : "fail";
		App.Instance.Logger.Log($"Mission {Name} ended with {success}");

        for (int i = 0; i < _models.transform.childCount; i++)
        {
            App.Instance.DestroyGameObject(_models.transform.GetChild(i).gameObject);
        }

        base.End(succeeded);
    }

    public override void Update()
    {
        base.Update();

        if (!HasBegun || HasEnded) return;

        if (_objectCount == 0)
        {
            GenerateSphere();
        }

        if (Inputs.Instance.IsPressed(0))
        {
            CheckAndDestroySphere();
        }

        CheckMissionEnd();
    }

    private void CheckMissionEnd()
    {
        if (_hits < 3
            || HasEnded
            || Level.Game.Timer.IsPaused)
            return;

        // prepare for state save
        Level.State.CurrentLevelId++;

        End(true);

        if (Level.State.CurrentLevelId % 2 == 0)
        {
            Level.State.CurrentSceneId++;
            if (Level.State.CurrentSceneId >= Level.Game.GameMode.SceneNames.Count) Level.State.CurrentSceneId = 0;

            App.Instance.GoToMenu();
        }
    }

    private void GenerateSphere()
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(_models.transform);
        sphere.transform.position = UnityEngine.Random.insideUnitSphere * 3f;
        sphere.tag = "hitObject";
        _objectCount++;
    }

    private void CheckAndDestroySphere()
    {
        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit))
        {
            var hitObject = hit.transform.gameObject;
            if (hitObject.tag == "hitObject")
            {
                GameObject.Destroy(hitObject);
                _objectCount--;

                Level.Game.Score1 += 10;
                _hits++;
            }
        }
    }
}
