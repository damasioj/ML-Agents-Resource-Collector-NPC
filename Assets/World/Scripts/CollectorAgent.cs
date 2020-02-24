using MLAgents;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorAgent : Agent
{
    private Vector3 maxVelocity;
    private bool reachedBoundary;
    private Rigidbody rBody;
    //private BehaviorParameters policy;

    public float speed;
    public BaseTarget[] targets;
    public ResourceDepot goal;

    #region properties
    private bool hasResource { get; set; }
    public Dictionary<string, float> boundaryLimits { get; set; }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        hasResource = false;
        reachedBoundary = false;
        rBody = GetComponent<Rigidbody>();
        maxVelocity = new Vector3(speed, 0f, speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "goal":
                if (hasResource)
                {
                    AddReward(1.0f);
                    hasResource = false;
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
            case "target":
                if (!hasResource)
                {
                    hasResource = true;
                    AddReward(0.3f);
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
            case "boundary":
                if (true)
                {
                    reachedBoundary = true;
                    SubtractReward(0.6f);
                    Done();
                }
                break;
        }
    }

    public override void AgentReset()
    {
        Debug.Log($"Reward: {GetCumulativeReward()}");

        // If the Agent fell, zero its momentum
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 1, 0);

        hasResource = false;
        reachedBoundary = false;

        //ResetTargets();
    }

    private void ResetTargets() // refactor to academy
    {
        targets.Where(x => x.TargetHit).ToList().ForEach(t => t.Reset());
    }

    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    // boundaries
    //    boundaryLimits.Values.ToList().ForEach(b => sensor.AddObservation(b)); //4

    //    // target locations
    //    targets.ToList().ForEach(t => sensor.AddObservation(t.Location)); //3 * n

    //    // goal location
    //    sensor.AddObservation(goal.transform.position); //3

    //    // Agent data
    //    sensor.AddObservation(hasResource); //1
    //    sensor.AddObservation(transform.position); //3
    //    sensor.AddObservation(rBody.velocity.x); //1
    //    sensor.AddObservation(rBody.velocity.z); //1
    //}

    public override void CollectObservations()
    {
        // boundaries
        boundaryLimits.Values.ToList().ForEach(b => AddVectorObs(b)); //4

        // target locations
        targets.ToList().ForEach(t => AddVectorObs(t.Location)); //3 * n

        // goal location
        AddVectorObs(goal.transform.position); //3

        // Agent data
        AddVectorObs(hasResource); //1
        AddVectorObs(transform.position); //3
        AddVectorObs(rBody.velocity.x); //1
        AddVectorObs(rBody.velocity.z); //1
    }

    public override void AgentAction(float[] vectorAction)
    {
        Move(vectorAction);
    }

    private void Move(float[] vectorAction)
    {
        // Move Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        if (rBody.velocity.magnitude < maxVelocity.magnitude)
        {
            rBody.velocity += controlSignal * speed;
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }

    private void SubtractReward(float value)
    {
        AddReward(value * -1);
    }
}
