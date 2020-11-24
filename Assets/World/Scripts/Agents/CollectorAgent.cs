using System;
using System.Collections.Generic;
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
    private bool isAtResource;
    private int internalStepCount; // used in place of maxsteps
    private float y;
    private readonly object dataLock = new object();
    [SerializeField] private int maxInternalSteps;

    public float speed;
    public BaseGoal goal;

    #region properties
    private bool HasResource => resource is object;
    
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
            Debug.Log($"Current target: {target.gameObject.name}, Number required: {target.ResourceCount}");
        }
    }

    private States currentState;
    public States CurrentState // Refactored in EcoSystem project
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
        y = transform.position.y;
        internalStepCount = 0;
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
            return;
        }

        internalStepCount++;
        if (internalStepCount > maxInternalSteps)
        {
            SubtractReward(0.08f);
            EndEpisode();
            return;
        }

        stateDictionary[CurrentState].OnFixedUpdate(this);        
    }

    void AssignStateDictionary() // TODO :maybe refactor to scriptable object
    {
        stateDictionary = new Dictionary<States, AgentState>()
        {
            [States.Idle] = new IdleState(),
            [States.Moving] = new MoveState(),
            [States.Collecting] = new CollectingState()
        };
    }

    private void OnTriggerEnter(Collider other) 
    {
        switch (other.tag)
        {
            case "goal":
                if (HasResource)
                {
                    AddReward(0.2f);
                    var deposit = other.gameObject.GetComponent(typeof(BaseGoal)) as BaseGoal;
                    deposit.AddResource(ref resource);
                    ValidateJobComplete();
                    ValidateGoalComplete();
                    internalStepCount = 0;
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
            case "target":
                if (!HasResource && other.GetComponent<BaseTarget>().Equals(Target))
                {
                    isAtResource = true;
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
            isAtResource = false;
        }
    }

    public override void OnEpisodeBegin()
    {
        lock (dataLock)
        {
            SetReward(0f);
            internalStepCount = 0;

            if (!goal.IsComplete)
            {
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
                transform.position = new Vector3(0, y, 0);
                currentState = States.Idle;
            }

            collectorAcademy.EnvironmentReset(); // This is refactored on EcoSystem project
            resource = null;
            isDoneCalled = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        lock (dataLock)
        {
            // target location
            sensor.AddObservation(target.Location.x); //1
            sensor.AddObservation(target.Location.z); //1

            // goal info
            sensor.AddObservation(goal.Location.x); //1
            sensor.AddObservation(goal.Location.z); //1

            // Agent data
            sensor.AddObservation(HasResource); //1
            sensor.AddObservation(transform.position.x); //1
            sensor.AddObservation(transform.position.z); //1
            sensor.AddObservation(rBody.velocity.x); //1
            sensor.AddObservation(rBody.velocity.z); //1
            sensor.AddObservation((int)CurrentState); // 1
            sensor.AddObservation(isAtResource); // 1
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //refactor
        if (stateDictionary[CurrentState].IsFinished)
        {
            CollectResource();
        }
        
        if (stateDictionary[CurrentState].IsFinished)
        {
            Move(vectorAction);            
        }
    }

    private void Move(float[] vectorAction)
    {
        // agent is idle
        if (vectorAction[0] == 0 && vectorAction[1] == 0)
        {
            CurrentState = States.Idle;
            return;
        }

        // agent is moving
        CurrentState = States.Moving;
        stateDictionary[CurrentState].DoAction(this, vectorAction);
    }

    private void CollectResource()
    {
        if (isAtResource && !HasResource)
        {
            internalStepCount = 0;
            CurrentState = States.Collecting;
            stateDictionary[CurrentState].SetAction(TakeResource);
        }
    }

    private void TakeResource()
    {
        if (isAtResource && !HasResource)
        {
            resource = Target.GetResource();

            if (resource is object)
            {
                internalStepCount = 0;
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
        var isResourceRequired = goal.GetResourceRequirements().Any(g => g.Key == target.GetResourceType() && g.Value > 0);

        if (!isResourceRequired)
        {
            collectorAcademy.SetAgentTarget();
        }
    }

    protected void ValidateGoalComplete()
    {
        if(goal.IsComplete)
        {
            SubtractReward(GetCumulativeReward());
            AddReward(5.0f);
            Debug.Log($"Current Reward: {GetCumulativeReward()}");
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    private void SubtractReward(float value) 
    {
        AddReward(value * -1);
    }
}
