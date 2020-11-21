using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDepot : BaseGoal
{
    // These can be changed to scriptable objects to be more dynamic
    private ResourceCollection<WoodResource> woodResources;
    private ResourceCollection<StoneResource> stoneResources;

    // Defines the maximum materials required.
    // This is randomized only for training.
    [SerializeField] private int maxWoodRequired;
    [SerializeField] private int maxStoneRequired;

    private int woodRequired;
    private int stoneRequired;

    public void Start()
    {
        woodRequired = UnityEngine.Random.Range(1, maxWoodRequired);
        stoneRequired = UnityEngine.Random.Range(1, maxStoneRequired);
    }
    
    public override void Reset()
    {
        base.Reset();
        woodResources = new ResourceCollection<WoodResource>();
        stoneResources = new ResourceCollection<StoneResource>();

        woodRequired = UnityEngine.Random.Range(1, maxWoodRequired);
        stoneRequired = UnityEngine.Random.Range(1, maxStoneRequired);
    }

    public override bool IsComplete
    {
        get
        {
            return woodResources.Count >= woodRequired
                && stoneResources.Count >= stoneRequired;
        }
    }

    public override IDictionary<Type, int> GetResourceRequirements()
    {
        return new Dictionary<Type, int>
        {
            [typeof(WoodResource)] = woodRequired - woodResources.Count,
            [typeof(StoneResource)] = stoneRequired - stoneResources.Count
        };
    }

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

    public override void AddResource(ref BaseResource resource)
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
