using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGoal : MonoBehaviour
{
    public Dictionary<string, float> goalLimits { get; set; }

    public virtual void Reset()
    {
        gameObject.transform.localPosition =
            new Vector3
            (
                Random.Range(goalLimits["-X"], goalLimits["X"]),
                -0.3f,
                Random.Range(goalLimits["-Z"], goalLimits["Z"])
            );
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
