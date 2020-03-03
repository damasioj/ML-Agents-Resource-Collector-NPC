using MLAgents;
using MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorAgent : Agent
{
    private Vector3 maxVelocity;
    private Rigidbody rBody;
    private BaseResource resource;
    //private BehaviorParameters policy;

    public float speed;
    public BaseTarget[] targets;
    public BaseGoal goal;

    #region properties
    private bool HasResource => resource is object;
    [HideInInspector] public Dictionary<string, float> BoundaryLimits { get; set; }
    #endregion

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        maxVelocity = new Vector3(speed, 0f, speed);
    }

    private void OnTriggerEnter(Collider other) // TODO : refactor
    {
        switch (other.tag)
        {
            case "goal":
                if (HasResource)
                {
                    AddReward(0.5f);
                    var deposit = other.gameObject.GetComponent(typeof(BaseGoal)) as BaseGoal;
                    deposit.AddResource(ref resource);
                    ValidateGoalComplete();
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
            case "target":
                if (!HasResource)
                {
                    var target = other.gameObject.GetComponent(typeof(BaseTarget)) as BaseTarget;
                    resource = target.GetResource();

                    if (resource is object)
                    {
                        AddReward(0.3f);
                        Debug.Log($"Current Reward: {GetCumulativeReward()}");
                    }
                }
                break;
            case "boundary":
                SubtractReward(0.5f);
                Done();
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

        resource = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // boundaries
        BoundaryLimits.Values.ToList().ForEach(b => sensor.AddObservation(b)); //4

        // target locations
        targets.ToList().ForEach(t => sensor.AddObservation(t.Location)); //3 * n

        // goal location
        sensor.AddObservation(goal.transform.position); //3
        goal.GetResourcesRequired().Values.ToList().ForEach(x => sensor.AddObservation(x)); // tower = 2, depot = 1

        // Agent data
        sensor.AddObservation(HasResource); //1
        sensor.AddObservation(transform.position); //3
        sensor.AddObservation(rBody.velocity.x); //1
        sensor.AddObservation(rBody.velocity.z); //1
    }

    //public override void CollectObservations()
    //{
    //    // boundaries
    //    BoundaryLimits.Values.ToList().ForEach(b => AddVectorObs(b)); //4

    //    // target locations
    //    targets.ToList().ForEach(t => AddVectorObs(t.Location)); //3 * n
    //    // TODO : add the type of resource each target holds ... how to associate ?

    //    // goal data
    //    AddVectorObs(goal.transform.position); //3
    //    goal.GetResourcesRequired().Values.ToList().ForEach(x => AddVectorObs(x)); // tower = 2, depot = 1
    //    // TODO : add the type of resource necessary

    //    // Agent data
    //    AddVectorObs(HasResource); //1
    //    AddVectorObs(transform.position); //3
    //    AddVectorObs(rBody.velocity.x); //1
    //    AddVectorObs(rBody.velocity.z); //1
    //}

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

    protected void ValidateGoalComplete()
    {
        if(goal.IsComplete)
        {
            AddReward(5.0f);            
            Debug.Log($"Current Reward: {GetCumulativeReward()}");
            Done();
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
