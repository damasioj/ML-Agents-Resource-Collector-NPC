using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDepot : MonoBehaviour
{
    public int ResourceCount { get; private set; }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // later to add resource type etc...
    public void AddResource()
    {
        ResourceCount++;
    }
}
