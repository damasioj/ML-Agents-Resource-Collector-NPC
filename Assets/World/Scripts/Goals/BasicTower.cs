using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicTower : BaseGoal
{
    private ResourceCollection<WoodResource> woodResources;
    private ResourceCollection<StoneResource> stoneResources;

    [SerializeField] private int woodRequired;
    [SerializeField] private int stoneRequired;

    public override void Reset()
    {
        woodResources = new ResourceCollection<WoodResource>();
        stoneResources = new ResourceCollection<StoneResource>();
        //base.Reset();
    }

    public override bool IsComplete
    {
        get
        {
            return woodResources.Count >= woodRequired
                && stoneResources.Count >= stoneRequired;
        }
    }

    // TODO : refactor
    public override IDictionary<Type, int> GetResourcesRequired()
    {
        return new Dictionary<Type, int>
        {
            [typeof(WoodResource)] = woodRequired - woodResources.Count,
            [typeof(StoneResource)] = stoneRequired - stoneResources.Count
        };
    }

    // TODO : refactor
    public override void SetResourceRequirements(IDictionary<Type, int> requirements)
    {
        if (requirements.ContainsKey(typeof(WoodResource)))
        {
            woodRequired = requirements[typeof(WoodResource)];
        }

        if (requirements.ContainsKey(typeof(StoneResource)))
        {
            stoneRequired = requirements[typeof(StoneResource)];
        }
    }

    private void Awake()
    {
        woodResources = new ResourceCollection<WoodResource>();
        stoneResources = new ResourceCollection<StoneResource>();
    }

    public override void AddResource(ref BaseResource resource) // TODO : REFACTOR
    {        
        if (resource is WoodResource wood)
        {
            AddResource(ref wood);
        }
        else if (resource is StoneResource stone)
        {
            AddResource(ref stone);
        }

        // ensure the source object is null to avoid duplication
        // need to find better solution
        if (resource is object)
        {
            resource = null;
        }
    }

    private void AddResource(ref WoodResource wood)
    {
        woodResources.Add(ref wood);
    }

    private void AddResource(ref StoneResource stone)
    {
        stoneResources.Add(ref stone);
    }
}
