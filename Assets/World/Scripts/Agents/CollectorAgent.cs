using Google.Protobuf.WellKnownTypes;
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

    public float speed;
    public BaseGoal goal;

    #region properties
    private bool HasResource => resource is object;

    [HideInInspector] public bool IsDoneJob { get; private set; }    

    [HideInInspector] public Dictionary<string, float> BoundaryLimits { get; set; }
    
    private BaseTarget target;
    [HideInInspector] public BaseTarget Target
    {
        get
        {
            return target;
        }
        set
        {
            target = value;
            IsDoneJob = false;
            Debug.Log($"Current target: {target.gameObject.name}, Number required: {target.ResourceCount}");
        }
    }
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
                    ValidateJobComplete();
                    ValidateGoalComplete();
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
            case "target":
                if (!HasResource)
                {
                    var target = other.gameObject.GetComponent(typeof(BaseTarget)) as BaseTarget;
                    resource = target.GetResource();

                    //if (resource is object)
                    //{
                    //    AddReward(0.3f);
                    //    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                    //}
                }
                break;
            case "boundary":
                if (!IsDone)
                {
                    SubtractReward(0.1f);
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                    Done();
                }
                break;
        }
    }

    public override void AgentReset()
    {
        Debug.Log($"Reward: {GetCumulativeReward()}");

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 1, 0);

        resource = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // boundaries
        //BoundaryLimits.Values.ToList().ForEach(b => sensor.AddObservation(b)); //4

        // target location
        sensor.AddObservation(target.transform.position.x); //1
        sensor.AddObservation(target.transform.position.z); //1

        // goal info
        sensor.AddObservation(goal.transform.position.x); //1
        sensor.AddObservation(goal.transform.position.z); //1
        sensor.AddObservation(goal.GetResourcesRequired().First(g => g.Key == target.GetResourceType()).Value); //1 -- # required of current resource

        // Agent data
        sensor.AddObservation(IsDoneJob);
        sensor.AddObservation(HasResource); //1
        sensor.AddObservation(transform.position.x); //1
        sensor.AddObservation(transform.position.z); //1
        sensor.AddObservation(rBody.velocity.x); //1
        sensor.AddObservation(rBody.velocity.z); //1

        // for debugging only
        //if (this.StepCount % 300 == 0)
        //{
        //    Debug.Log($"TARGET || Name: {target.name} X: {target.transform.position.x}, Z: {target.transform.position.z}");
        //    Debug.Log($"GOAL || X: {goal.transform.position.x}, Z: {goal.transform.position.z}");
        //    Debug.Log($"GOAL || Resources Required: {goal.GetResourcesRequired().First(g => g.Key == target.GetResourceType()).Value}");
        //    Debug.Log($"AGENT || IsDoneJob: {IsDoneJob}, HasResource: {HasResource}");
        //    Debug.Log($"AGENT || X: {transform.position.x}, Z: {transform.position.z}, Velocity X: {rBody.velocity.x}, Velocity Z: {rBody.velocity.z}");
        //}
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

        if (controlSignal.x + controlSignal.z == 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Checks if the goal has the required number of resources from current target.
    /// </summary>
    protected void ValidateJobComplete()
    {
        var isResourceRequired = goal.GetResourcesRequired().Any(g => g.Key == target.GetResourceType() && g.Value > 0);

        if (!isResourceRequired)
        {
            IsDoneJob = true;
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
