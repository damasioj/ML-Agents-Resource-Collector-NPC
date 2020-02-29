using System;

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

    public override void SetResourceAmount(Type resource, int amount)
    {
        if (resource == typeof(WoodResource))
        {
            woodAmount = amount;
            ResetCollection();
        }
    }

    private void ResetCollection()
    {
        WoodResources = new ResourceCollection<WoodResource>(woodAmount);
    }
}
