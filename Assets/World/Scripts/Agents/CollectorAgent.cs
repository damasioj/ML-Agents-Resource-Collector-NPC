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
    private bool atResource;
    private int internalStepCount; // used in place of maxsteps
    [SerializeField] private int maxInternalSteps;

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
    public States CurrentState // TODO :  refactor this property to agent class
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
                    AddReward(0.5f);
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
        internalStepCount = 0;
        
        if (!goal.IsComplete)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = new Vector3(0, 1, 0);
            currentState = States.Idle;
        }
        
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
        sensor.AddObservation(atResource); // 1

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
        if (atResource && !HasResource)
        {
            internalStepCount = 0;
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
    }

    private void SubtractReward(float value) // TODO : add to agent class
    {
        AddReward(value * -1);
    }
}
