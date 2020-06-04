using System;
using System.Collections.Generic;

// possibly could be refactored all into basetarget class
public class TreeTarget : BaseTarget
{
    protected ResourceCollection<WoodResource> WoodResources { get; set; }
    public override int ResourceCount => WoodResources.Count;

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
        return typeof(WoodResource);
    }

    public override BaseResource GetResource()
    {
        return WoodResources.Take();
    }

    public override void SetResourceAmount(Dictionary<Type, int> resourceData)
    {
        if (resourceData.ContainsKey(typeof(WoodResource)))
        {
            ResetCollection(resourceData[typeof(WoodResource)]);
        }
    }

    protected virtual void ResetCollection(int amount)
    {
        WoodResources = new ResourceCollection<WoodResource>(amount);
    }
}
