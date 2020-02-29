using System;

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

    public override void SetResourceAmount(Type resource, int amount)
    {
        if (resource == typeof(StoneResource))
        {
            stoneAmount = amount;
            ResetCollection();
        }
    }

    private void ResetCollection()
    {
        StoneResources = new ResourceCollection<StoneResource>(stoneAmount);
    }
}
