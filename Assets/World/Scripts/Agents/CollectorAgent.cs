using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CollectorAgent : Agent
{
    private CollectorAcademy collectorAcademy;
    private Rigidbody rBody;
    private BaseResource resource;
    private Coroutines routine;
    private bool isDoneCalled;
    private bool atResource;
    private object actionLock;

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
        actionLock = new object();
        isDoneCalled = false;
        collectorAcademy = GetComponentInParent<CollectorAcademy>();
        rBody = GetComponent<Rigidbody>();
        routine = Coroutines.Instance;
    }

    private void FixedUpdate()
    {
        if (isDoneCalled)
        {
            isDoneCalled = false;
            EndEpisode();
        }
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
                    atResource = true;
                }
                break;
            case "boundary":
                if (!isDoneCalled)
                {
                    isDoneCalled = true;
                    SubtractReward(0.1f);
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("target"))
        {
            atResource = false;
        }
    }

    public override void OnEpisodeBegin()
    {
        SetReward(0f);
        
        if (!goal.IsComplete)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = new Vector3(0, 1, 0);
        }

        collectorAcademy.EnvironmentReset(); // TODO : find a way to refactor this ... agent shouldn't call academy functions
        resource = null;
        isDoneCalled = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (sensor is object && StepCount > 0) // guarantee we dont send NaN values
        {
            // boundaries
            //BoundaryLimits.Values.ToList().ForEach(b => sensor.AddObservation(b)); //4

            // target location
            sensor.AddObservation(target.transform.localPosition.x); //1
            sensor.AddObservation(target.transform.localPosition.z); //1

            // goal info
            sensor.AddObservation(goal.transform.localPosition.x); //1
            sensor.AddObservation(goal.transform.localPosition.z); //1
            sensor.AddObservation(goal.GetResourcesRequired().First(g => g.Key == target.GetResourceType()).Value); //1 -- # required of current resource

            // Agent data
            sensor.AddObservation(IsDoneJob); // 1
            sensor.AddObservation(HasResource); //1
            sensor.AddObservation(transform.localPosition.x); //1
            sensor.AddObservation(transform.localPosition.z); //1
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
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Move(vectorAction);
        DoAction(vectorAction);
    }

    private void Move(float[] vectorAction)
    {
        // Move Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        if (controlSignal.x == 0 && controlSignal.z == 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            return;
        }

        if (rBody.velocity.x > speed)
        {
            controlSignal.x = 0;
        }
        if (rBody.velocity.z > speed)
        {
            controlSignal.z = 0;
        }

        rBody.AddForce(new Vector3(controlSignal.x * 750, 0, controlSignal.z * 750));        
    }

    private void DoAction(float[] vectorAction)
    {

        if (vectorAction[2] == 1f && atResource && !HasResource)
        {
            // do action
            //StartCoroutine(routine.BasicWaiter(5f));
            routine.BasicWaiter(5000f);
            resource = Target.GetResource();

            if (resource is object)
            {
                AddReward(0.1f);
                Debug.Log($"Current Reward: {GetCumulativeReward()}");
            }
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
            collectorAcademy.SetAgentTarget();
        }
    }

    protected void ValidateGoalComplete()
    {
        if(goal.IsComplete)
        {
            AddReward(2.0f);
            Debug.Log($"Current Reward: {GetCumulativeReward()}");
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        actions[2] = Convert.ToSingle(Input.GetKey(KeyCode.E));
    }

    private void SubtractReward(float value)
    {
        AddReward(value * -1);
    }
}
