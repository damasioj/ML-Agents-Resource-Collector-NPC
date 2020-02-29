using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDepot : BaseGoal
{
    [SerializeField] private int maxCapacity;
    [HideInInspector] public List<BaseResource> Resources;

    void Start()
    {
        Reset();
        Resources = new List<BaseResource>();
    }

    public override bool IsComplete
    {
        get
        {
            return Resources.Count >= maxCapacity;
        }
    }

    public override void AddResource(ref BaseResource resource)
    {
        if (Resources.Count < maxCapacity)
        {
            Resources.Add(resource);
        }
    }

    public override IDictionary<Type, int> GetResourcesRequired()
    {
        return new Dictionary<Type, int>
        {
            [typeof(BaseResource)] = maxCapacity - Resources.Count
        };
    }

    public override void SetResourceRequirements(IDictionary<Type, int> requirements)
    {
        if (requirements.ContainsKey(typeof(BaseResource)))
        {
            maxCapacity = requirements[typeof(BaseResource)];
        }
    }
}
