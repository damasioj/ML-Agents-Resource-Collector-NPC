using System;
using Unity.MLAgents;
using UnityEngine;

public class MoveState : AgentState
{
    public override bool IsFinished { get; protected set; }

    public override void DoAction(Agent owner)
    {
        return;
    }

    public override void DoAction(Agent owner, float[] vectorAction)
    {
        var rBody = owner.GetComponent<Rigidbody>();

        if (rBody is object)
        {
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];

            if (rBody.velocity.x > 30)
            {
                controlSignal.x = 0;
            }
            if (rBody.velocity.z > 30)
            {
                controlSignal.z = 0;
            }

            rBody.AddForce(new Vector3(controlSignal.x * 750, 0, controlSignal.z * 750));
        }

        IsFinished = true;
    }

    public override void OnEnter(Agent owner)
    {
        IsFinished = false;
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
        return;
    }

    public override void SetAction(Action action)
    {
        throw new NotImplementedException();
    }
}
