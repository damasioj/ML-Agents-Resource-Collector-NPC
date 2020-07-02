using System;
using System.Collections.Generic;
using UnityEngine;

// Purpose of this class is still to be fully defined 
public abstract class BaseGoal : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private float maxSize;

    public virtual void Reset()
    {
        gameObject.transform.position =
            new Vector3
            (
                UnityEngine.Random.Range(range * -1, range),
                transform.position.y,
                UnityEngine.Random.Range(range * -1, range)
            );

        float size = UnityEngine.Random.Range(1f, maxSize);
        gameObject.transform.localScale = new Vector3(size, size, size);
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
