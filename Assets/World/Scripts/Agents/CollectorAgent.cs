using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CollectorAgent : Agent
{
    private Dictionary<States, AgentState> stateDictionary;    
    private CollectorAcademy collectorAcademy;
    private Rigidbody rBody;
    private BaseResource resource;
    private bool isDoneCalled;
    private bool atResource;
    private int stepLimiter; // used in place of maxsteps

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

    private States currentState;
    public States CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (value != currentState)
            {
                stateDictionary[currentState].OnExit(this);
                currentState = value;
                stateDictionary[currentState].OnEnter(this);
            }
        }
    }
    #endregion

    void Start()
    {
        stepLimiter = 0;
        isDoneCalled = false;
        collectorAcademy = GetComponentInParent<CollectorAcademy>();
        rBody = GetComponent<Rigidbody>();
        
        // assign states to dictionary and set
        AssignStateDictionary();
        CurrentState = States.Idle;
    }

    void Update()
    {
        stateDictionary[CurrentState].OnUpdate(this);
    }

    void FixedUpdate()
    {
        if (isDoneCalled)
        {
            isDoneCalled = false;
            EndEpisode();
        }

        stateDictionary[CurrentState].OnFixedUpdate(this);

        stepLimiter++;
        if (stepLimiter > 1500)
        {
            SubtractReward(0.1f);
            EndEpisode();
        }
    }

    void AssignStateDictionary()
    {
        stateDictionary = new Dictionary<States, AgentState>()
        {
            [States.Idle] = new IdleState(),
            [States.Moving] = new MoveState(),
            [States.Collecting] = new CollectingState()
        };
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
                    stepLimiter = 0;
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
        stepLimiter = 0;
        
        if (!goal.IsComplete)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = new Vector3(0, 1, 0);
        }

        currentState = States.Idle;
        collectorAcademy.EnvironmentReset(); // TODO : find a way to refactor this ... agent shouldn't call academy functions
        resource = null;
        isDoneCalled = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // boundaries
        //BoundaryLimits.Values.ToList().ForEach(b => sensor.AddObservation(b)); //4

        // target location
        sensor.AddObservation(target.transform.localPosition.x); //1
        sensor.AddObservation(target.transform.localPosition.z); //1

        // goal info
        sensor.AddObservation(goal.transform.localPosition.x); //1
        sensor.AddObservation(goal.transform.localPosition.z); //1

        // Agent data
        sensor.AddObservation(HasResource); //1
        sensor.AddObservation(transform.localPosition.x); //1
        sensor.AddObservation(transform.localPosition.z); //1
        sensor.AddObservation(rBody.velocity.x); //1
        sensor.AddObservation(rBody.velocity.z); //1
        sensor.AddObservation((int)CurrentState); // 1

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

    public override void OnActionReceived(float[] vectorAction)
    {
        if (stateDictionary[currentState].IsFinished)
        {
            Move(vectorAction);
            CollectResource(vectorAction);
        }
    }

    private void Move(float[] vectorAction)
    {
        // Move Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        // agent is idle
        if (controlSignal.x == 0 && controlSignal.z == 0)
        {
            CurrentState = States.Idle;
            return;
        }

        // agent is moving
        CurrentState = States.Moving;
        stateDictionary[CurrentState].DoAction(this, vectorAction);
    }

    private void CollectResource(float[] vectorAction)
    {
        if (vectorAction[2] == 1f && atResource && !HasResource)
        {
            stepLimiter = 0;
            CurrentState = States.Collecting;
            stateDictionary[CurrentState].SetAction(TakeResource);
        }
    }

    private void TakeResource()
    {
        if (atResource && !HasResource)
        {
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
