using System;
using System.Collections.Generic;

// possibly could be refactored all into basetarget class
public class TreeTarget : BaseTarget
{
    protected ResourceCollection<WoodResource> WoodResources { get; set; }
    public int woodAmount;
    
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
        return WoodResources.Take();
    }

    public override void SetResourceAmount(Dictionary<Type, int> resourceData)
    {
        if (resourceData.ContainsKey(typeof(WoodResource)))
        {
            woodAmount = resourceData[typeof(WoodResource)];
            ResetCollection();
        }
    }

    private void ResetCollection()
    {
        WoodResources = new ResourceCollection<WoodResource>(woodAmount);
    }
}
