using System;
using Unity.MLAgents;
using UnityEngine;

public class CollectingState : AgentState
{
    private Action actionToExecute;
    private float counter = 0f;
    
    public override bool IsFinished { get; protected set; }

    public override void SetAction(Action action)
    {
        actionToExecute = action;
    }

    public override void DoAction(Agent owner)
    {
        return;
    }

    public override void DoAction(Agent owner, float[] vectorActions)
    {
        throw new NotImplementedException();
    }

    public override void OnEnter(Agent owner)
    {
        IsFinished = false;
        counter = owner.StepCount;
        // todo : start animation
    }

    public override void OnExit(Agent owner)
    {
        return;
    }

    public override void OnFixedUpdate(Agent owner)
    {
        return;
    }

    public override void OnUpdate(Agent owner)
    {
        if (owner.StepCount - counter >= 50)
        {
            actionToExecute();
            IsFinished = true;

            // TODO : refactor CurrentState location ... this will be huge problem.
            // State property needs to be added to Agent class
            if (owner is CollectorAgent collector)
            {
                collector.CurrentState = States.Idle;
            }
        }
    }
}
