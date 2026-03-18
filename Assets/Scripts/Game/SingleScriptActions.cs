using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameLogic;

public class SingleScriptActions : MonoBehaviour
{
    [Serializable]
    public class GameFunction
    {
        public Action DataAction;
        public Action<GameFunction> VisualAction;
        public int Delay;
    }

    public GridEntity gridEntity;

    public List<GameFunction> functions = new();

    //Functions - only player inputs for IDE
    GameFunction DroneMoveFunction(AdjacentDirection direction) => new()
    {
        DataAction = () => gridEntity.SingleStepInDirection(direction),
        VisualAction = (self) =>
        {
            if (gridEntity.View is DroneView droneView)
                droneView.MoveBetween(
                    gridEntity.Data.Position,
                    gridEntity.Data.Position + gridEntity.GetDirectionVector(direction),
                    self.Delay, 1f, Mathf.RoundToInt(gridEntity.Height));
        },
        Delay = 0
    };

    void Start()
    {
        GameFunction function = functions[0];
        IDE_Function(function);
    }

    //layout
    public void IDE_Function(GameFunction function)
    {
        StartCoroutine(RunAfterDelay(function.DataAction, function.Delay));
        function.VisualAction(function);
    }

    //just testing with IEnumerator
    public IEnumerator RunAfterDelay(Action action, int delayTicks)
    {
        int startTick = Clock.I.tick;
        yield return new WaitUntil(() => Clock.I.tick >= startTick + delayTicks);
        action();
    }
}