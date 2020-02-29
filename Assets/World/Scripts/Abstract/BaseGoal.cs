using System;
using System.Collections.Generic;
using UnityEngine;

// Purpose of this class is still to be fully defined 
public abstract class BaseGoal : MonoBehaviour
{
    public Dictionary<string, float> goalLimits { get; set; }

    public virtual void Reset()
    {
        gameObject.transform.localPosition =
            new Vector3
            (
                UnityEngine.Random.Range(goalLimits["-X"], goalLimits["X"]),
                -0.3f,
                UnityEngine.Random.Range(goalLimits["-Z"], goalLimits["Z"])
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

    /// <summary>
    /// Adds a resource to the goal and removes it from the source.
    /// </summary>
    /// <param name="resource"></param>
    public abstract void AddResource(ref BaseResource resource);

    /// <summary>
    /// Determines if the goal is complete/full/finished.
    /// </summary>
    public abstract bool IsComplete { get; }

    /// <summary>
    /// Returns a dictionary with the type of resource required and the amount necessary to finish completion.
    /// </summary>
    /// <returns>IDictionary<Type, int></returns>
    public abstract IDictionary<Type, int> GetResourcesRequired();
    public abstract void SetResourceRequirements(IDictionary<Type, int> requirements);
}
