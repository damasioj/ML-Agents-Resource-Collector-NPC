using MLAgents;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorAcademy : MonoBehaviour
{
    private bool isFirstRun;
    private Academy collectorAcademy;
    private Dictionary<string, float> boundaryLimits;
    private BaseGoal goal;
    private CollectorAgent agent;
    private List<BaseTarget> targets;
    private Queue<float> runningRewardTotals;

    private void Awake()
    {
        collectorAcademy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        isFirstRun = true;
        boundaryLimits = GetBoundaryLimits();
        agent = gameObject.GetComponentInChildren<CollectorAgent>();
        targets = gameObject.GetComponentsInChildren<BaseTarget>().ToList();
        goal = gameObject.GetComponentInChildren<BaseGoal>();

        agent.BoundaryLimits = boundaryLimits;
        targets.ForEach(t => t.BoundaryLimits = boundaryLimits);
        goal.goalLimits = GetGoalLimits();
        runningRewardTotals = new Queue<float>(3);
    }

    private void FixedUpdate()
    {
        agent.RequestDecision();

        if (agent.IsDoneJob)
        {
            SetAgentTarget();
        }
    }

    private void EnvironmentReset()
    {
        if (agent.IsDone || isFirstRun)
        {
            targets.Where(t => t.TargetHit).ToList().ForEach(t => t.Reset());
            goal.Reset();
            SetResourceRequirements();
            SetAgentTarget();
            isFirstRun = false;
        }
    }

    /// <summary>
    /// Sets a random valid target for the agent.
    /// </summary>
    private void SetAgentTarget()
    {
        var validTargets = GetValidTargets().ToList();

        if (validTargets.Count > 0)
        {
            agent.Target = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
        }
    }

    /// <summary>
    /// Returns the targets that still contain resources.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<BaseTarget> GetValidTargets()
    {
        return targets.Where(t => t.ResourceCount > 0);
    }

    private Dictionary<string, float> GetBoundaryLimits()
    {
        var limits = new Dictionary<string, float>()
        {
            ["-X"] = 0,
            ["X"] = 0,
            ["-Z"] = 0,
            ["Z"] = 0
        };

        var boundaries = gameObject.GetComponentsInChildren(typeof(BoxCollider)).Where(x => x.CompareTag("boundary")).ToList();
        foreach (var boundary in boundaries)
        {
            float x = boundary.gameObject.transform.localPosition.x;
            float z = boundary.gameObject.transform.localPosition.z;
            float lengthAdjust = boundary.gameObject.transform.localScale.y * 2;

            // set X boundary
            if (x > limits["X"] || limits["X"] == 0)
            {
                limits["X"] = x - lengthAdjust;
            }
            if (x < limits["-X"] || limits["-X"] == 0)
            {
                limits["-X"] = x + lengthAdjust;
            }

            // set Z boundary
            if (z > limits["Z"] || limits["Z"] == 0)
            {
                limits["Z"] = z - lengthAdjust;
            }
            if (z < limits["-Z"] || limits["-Z"] == 0)
            {
                limits["-Z"] = z + lengthAdjust;
            }
        }

        return limits;
    }
    private Dictionary<string, float> GetGoalLimits()
    {
        var limits = new Dictionary<string, float>()
        {
            ["-X"] = -5f,
            ["X"] = 10f,
            ["-Z"] = -5f,
            ["Z"] = 5f
        };

        return limits;
    }

    // temporary setup used for training
    private void SetResourceRequirements()
    {
        int maxAmount = 2;//GetMaxResourceAmount();

        int woodAmount = UnityEngine.Random.Range(1, maxAmount);
        int stoneAmount = UnityEngine.Random.Range(1, maxAmount);

        var requirements = new Dictionary<Type, int>
        {
            [typeof(WoodResource)] = woodAmount,
            [typeof(StoneResource)] = stoneAmount
        };

        goal.SetResourceRequirements(requirements);
        targets.ForEach(t => t.SetResourceAmount(requirements));
    }

    // TODO : for curriculum learning
    private int GetMaxResourceAmount()
    {
        if (runningRewardTotals.Count < 3)
        {
            return 2;
        }

        float average = runningRewardTotals.Sum() / 3;
        if (average > 3)
        {
            return 4;
        }
        else if (average > 2)
        {
            return 3;
        }
        else
        {
            return 2;
        }
    }
}
