using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTarget : MonoBehaviour
{
    public bool TargetHit { get; protected set; }
    public Dictionary<string, float> BoundaryLimits { get; set; }

    private void Awake()
    {
        TargetHit = false;
    }

    public virtual void Reset()
    {
        gameObject.transform.localPosition =
            new Vector3
            (
                UnityEngine.Random.Range(BoundaryLimits["-X"], BoundaryLimits["X"]),
                1f,
                UnityEngine.Random.Range(BoundaryLimits["-Z"], BoundaryLimits["Z"])
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

    public abstract BaseResource GetResource();
    public abstract void SetResourceAmount(Dictionary<Type, int> resourceData);
}
