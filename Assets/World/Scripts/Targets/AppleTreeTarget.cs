using System;
using System.Collections.Generic;

public class AppleTreeTarget : TreeTarget
{
    protected ResourceCollection<AppleResource> AppleResources { get; set; }

    private void Awake()
    {
        ResetCollection(0);
    }

    public override void Reset()
    {
        base.Reset();
        ResetCollection(0);
    }

    public override Type GetResourceType()
    {
        return typeof(AppleResource);
    }

    public override BaseResource GetResource()
    {
        return AppleResources.Take();
    }

    public override void SetResourceAmount(Dictionary<Type, int> resourceData)
    {
        if (resourceData.ContainsKey(typeof(AppleResource)))
        {
            ResetCollection(resourceData[typeof(AppleResource)]);
        }

        if (resourceData.ContainsKey(typeof(WoodResource)))
        {
            base.ResetCollection(resourceData[typeof(WoodResource)]);
        }
    }

    protected override void ResetCollection(int amount)
    {
        AppleResources = new ResourceCollection<AppleResource>(amount);
    }
}
