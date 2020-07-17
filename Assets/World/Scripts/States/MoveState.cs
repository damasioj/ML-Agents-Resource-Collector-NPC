using System;
using Unity.MLAgents;
using UnityEngine;

public class MoveState : AgentState
{
    private Vector3 _lastPosition = Vector3.zero;
    public override bool IsFinished { get; protected set; }

    public override void DoAction(Agent owner)
    {
        return;
    }

    public override void DoAction(Agent owner, float[] vectorAction)
    {
        if (owner is CollectorAgent collector)
        {
            var rBody = owner.GetComponent<Rigidbody>();
            var scale = collector.gameObject.transform.localScale.x;

            if (rBody is object)
            {
                Vector3 controlSignal = Vector3.zero;
                controlSignal.x = vectorAction[0];
                controlSignal.z = vectorAction[1];

                if (rBody.velocity.x > collector.speed * scale)
                {
                    controlSignal.x = 0;
                }
                if (rBody.velocity.z > collector.speed * scale)
                {
                    controlSignal.z = 0;
                }

                rBody.AddForce(new Vector3(controlSignal.x * collector.speed * scale, 0, controlSignal.z * collector.speed * scale));
            }

            SetDirection(owner);
            _lastPosition = owner.transform.position;

            IsFinished = true;
        }
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

    private void SetDirection(Agent owner)
    {
        var direction = (owner.transform.position - _lastPosition).normalized;

        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(-direction), 0.15F);
    }
}
