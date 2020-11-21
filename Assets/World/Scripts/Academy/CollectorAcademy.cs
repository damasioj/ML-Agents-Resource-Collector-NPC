using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class CollectorAcademy : MonoBehaviour
{
    private BaseGoal goal;
    private CollectorAgent agent;
    private List<BaseTarget> targets;

    private void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        agent = gameObject.GetComponentInChildren<CollectorAgent>();
        targets = gameObject.GetComponentsInChildren<BaseTarget>().ToList();
        goal = gameObject.GetComponentInChildren<BaseGoal>();

        SetResourceRequirements();
        SetAgentTarget();
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

    /// <summary>
    /// Sets target resource amount based on goal requirements.
    /// </summary>
    private void SetResourceRequirements()
    {
        var requirements = goal.GetResourceRequirements();
        targets.ForEach(t => t.SetResourceAmount(requirements));        
    }
}
