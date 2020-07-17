using Unity.MLAgents;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorAcademy : MonoBehaviour
{
    private Academy collectorAcademy;
    private Dictionary<string, float> boundaryLimits;
    private BaseGoal goal;
    private CollectorAgent agent;
    private List<BaseTarget> targets;

    private void Awake()
    {
        collectorAcademy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        //boundaryLimits = GetBoundaryLimits();
        agent = gameObject.GetComponentInChildren<CollectorAgent>();
        targets = gameObject.GetComponentsInChildren<BaseTarget>().ToList();
        goal = gameObject.GetComponentInChildren<BaseGoal>();

        agent.BoundaryLimits = boundaryLimits;
        //targets.ForEach(t => t.BoundaryLimits = boundaryLimits);
    }

    public void EnvironmentReset()
    {
        targets.Where(t => t.TargetHit).ToList().ForEach(t => t.Reset());
        goal.Reset();
        SetResourceRequirements();
        SetAgentTarget();
    }

    /// <summary>
    /// Sets a random valid target for the agent.
    /// </summary>
    public void SetAgentTarget()
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

    //private Dictionary<string, float> GetBoundaryLimits()
    //{
    //    var limits = new Dictionary<string, float>()
    //    {
    //        ["-X"] = 0,
    //        ["X"] = 0,
    //        ["-Z"] = 0,
    //        ["Z"] = 0
    //    };

    //    var boundaries = gameObject.GetComponentsInChildren(typeof(BoxCollider)).Where(x => x.CompareTag("boundary")).ToList();
    //    foreach (var boundary in boundaries)
    //    {
    //        float x = boundary.gameObject.transform.localPosition.x;
    //        float z = boundary.gameObject.transform.localPosition.z;
    //        float lengthAdjust = boundary.gameObject.transform.localScale.y * 2;

    //        // set X boundary
    //        if (x > limits["X"] || limits["X"] == 0)
    //        {
    //            limits["X"] = x - lengthAdjust;
    //        }
    //        if (x < limits["-X"] || limits["-X"] == 0)
    //        {
    //            limits["-X"] = x + lengthAdjust;
    //        }

    //        // set Z boundary
    //        if (z > limits["Z"] || limits["Z"] == 0)
    //        {
    //            limits["Z"] = z - lengthAdjust;
    //        }
    //        if (z < limits["-Z"] || limits["-Z"] == 0)
    //        {
    //            limits["-Z"] = z + lengthAdjust;
    //        }
    //    }

    //    return limits;
    //}

    // temporary setup used for training
    private void SetResourceRequirements()
    {
        int maxAmount = 4;//GetMaxResourceAmount();

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
}
