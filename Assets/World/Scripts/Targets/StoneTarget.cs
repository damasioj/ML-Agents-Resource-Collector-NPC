using System;
using System.Collections.Generic;

// possibly could be refactored all into basetarget class
public class StoneTarget : BaseTarget
{
    protected ResourceCollection<StoneResource> StoneResources { get; set; }
    public override int ResourceCount => StoneResources.Count;

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
        return typeof(StoneResource);
    }

    public override BaseResource GetResource()
    {
        return StoneResources.Take();
    }

    public override void SetResourceAmount(IDictionary<Type, int> resourceData)
    {
        if (resourceData.ContainsKey(typeof(StoneResource)))
        {
            ResetCollection(resourceData[typeof(StoneResource)]);
        }
    }

    private void ResetCollection(int amount)
    {
        StoneResources = new ResourceCollection<StoneResource>(amount);
    }
}
