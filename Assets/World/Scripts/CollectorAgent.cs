using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorAgent : Agent
{
    private Vector3 maxVelocity;
    private bool reachedBoundary;
    private Rigidbody rBody;
    //private BehaviorParameters policy;

    public float speed;

    public ITarget targets;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if in resource depot
        // check if in resource range
    }

    public override void AgentReset()
    {
        base.AgentReset();
    }

    private void ResetTargets()
    {

    }

    public override void CollectObservations()
    {
        base.CollectObservations();
    }

    public override void AgentAction(float[] vectorAction)
    {
        base.AgentAction(vectorAction);
    }

    private void Move()
    {

    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        //action[2] = Input.GetKey(KeyCode.E) ? 1.0f : 0.0f;
        return action;
    }

    private void SubtractReward(float value)
    {
        AddReward(value * -1);
    }
}
