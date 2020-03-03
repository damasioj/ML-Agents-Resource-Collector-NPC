using System;
using System.Collections.Generic;

// possibly could be refactored all into basetarget class
public class StoneTarget : BaseTarget
{
    protected ResourceCollection<StoneResource> StoneResources { get; set; }
    public int stoneAmount;

    private void Awake()
    {
        ResetCollection();
    }

    public override void Reset()
    {
        base.Reset();
        ResetCollection();
    }

    public override BaseResource GetResource()
    {
        return StoneResources.Take();
    }

    public override void SetResourceAmount(Dictionary<Type, int> resourceData)
    {
        if (resourceData.ContainsKey(typeof(StoneResource)))
        {
            stoneAmount = resourceData[typeof(StoneResource)];
            ResetCollection();
        }
    }

    private void ResetCollection()
    {
        StoneResources = new ResourceCollection<StoneResource>(stoneAmount);
    }
}
