using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDepot : BaseGoal
{
    public int ResourceCount { get; private set; }
    
    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    // later to add resource type etc...
    public void AddResource()
    {
        ResourceCount++;
    }
}
