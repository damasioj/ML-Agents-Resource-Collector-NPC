using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTarget : MonoBehaviour
{
    public bool TargetHit { get; protected set; }
    public Dictionary<string, float> boundaryLimits { get; set; }

    private void Awake()
    {
        TargetHit = false;
    }

    public virtual void Reset()
    {
        gameObject.transform.localPosition =
            new Vector3
            (
                Random.Range(boundaryLimits["-X"], boundaryLimits["X"]),
                1f,
                Random.Range(boundaryLimits["-Z"], boundaryLimits["Z"])
            );

        TargetHit = false;
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
}
