using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameLogic;

public class SingleScriptActions : MonoBehaviour
{
    public static SingleScriptActions I;
    public Coroutine active;

    void Awake()
    {
        I = this;
    }

    [Serializable]
    public class GameFunction
    {
        public int Delay;
        public Action<object[]> DataAction;
        public Action<GameFunction, object[]> VisualAction;
    }


    //Functions - only player inputs for IDE

    public GameFunction TestFunction => new()
    {
        //code delay time
        Delay = 30,
        //what the function does to the data (backend)
        DataAction = (vars) =>
        {
            
        },
        //what the function does visually (frontend)
        VisualAction = (self, vars) =>
        {
            
        }
    };

    public GameFunction DroneMoveFunction => new()
    {
        Delay = 30,
        DataAction = (vars) =>
        {
            ((GridEntity)vars[0]).SingleStepInDirection((AdjacentDirection)vars[1]);
        },
        VisualAction = (self, vars) =>
        {
            if (((GridEntity)vars[0]).View is DroneView droneView)
                droneView.MoveBetween(
                    ((GridEntity)vars[0]).Data.Position,
                    ((GridEntity)vars[0]).Data.Position + ((GridEntity)vars[0]).GetDirectionVector((AdjacentDirection)vars[1]),
                    self.Delay, 1f, Mathf.RoundToInt(((GridEntity)vars[0]).Height));
        }
    };

    //

    //example test
    IEnumerator Start()
    {
        yield return null;

        GridEntity gridEntity = GameGrid.I.Drones[0];
        IDE_Function(DroneMoveFunction, gridEntity, AdjacentDirection.East);
    }

    public void IDE_Function(GameFunction function, params object[] variables)
    {
        if (active != null) return;

        //execute visual action immediately, animates over Delay ticks
        function.VisualAction(function, variables);

        //execute data action after Delay ticks, so both complete at the same time
        active = StartCoroutine(RunAfterDelay(() => function.DataAction(variables), function.Delay));
    }

    //just testing with IEnumerator
    public IEnumerator RunAfterDelay(Action action, int delayTicks)
    {
        if (delayTicks > 0)
        {
            int startTick = Clock.I.tick;
            yield return new WaitUntil(() => Clock.I.tick >= startTick + delayTicks);
        }
        action();
        
        active = null;
    }
}