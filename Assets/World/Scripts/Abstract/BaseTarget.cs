using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTarget : MonoBehaviour
{
    public float range;
    public bool TargetHit { get; protected set; }
    //public Dictionary<string, float> BoundaryLimits { get; set; }
    public abstract int ResourceCount { get; }
    private float y;

    private void Awake()
    {
        TargetHit = false;
        y = transform.position.y;
    }

    public virtual void Reset()
    {
        gameObject.transform.position =
            new Vector3
            (
                UnityEngine.Random.Range(range * - 1, range),
                y,
                UnityEngine.Random.Range(range * -1, range)
            );

        TargetHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TargetHit = true;
    }

    public virtual Vector3 Location
    {
        get
        {
            return gameObject.transform.position;
        }
        private set
        {
            gameObject.transform.position = value;
        }
    }

    public abstract Type GetResourceType();
    public abstract BaseResource GetResource();
    public abstract void SetResourceAmount(Dictionary<Type, int> resourceData);
}
