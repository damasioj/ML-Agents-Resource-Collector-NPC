﻿using MLAgents;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorAcademy : MonoBehaviour
{
    private Academy collectorAcademy;
    private Dictionary<string, float> boundaryLimits;
    private BaseGoal goal;
    private List<CollectorAgent> agents;
    private List<BaseTarget> targets;

    private void Awake()
    {
        collectorAcademy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        boundaryLimits = GetBoundaryLimits();
        agents = gameObject.GetComponentsInChildren<CollectorAgent>().ToList();
        targets = gameObject.GetComponentsInChildren<BaseTarget>().ToList();
        goal = gameObject.GetComponentInChildren<BaseGoal>();

        agents.ForEach(a => a.boundaryLimits = boundaryLimits);
        targets.ForEach(t => t.boundaryLimits = boundaryLimits);
        goal.goalLimits = GetGoalLimits();

        //collectorAcademy.AutomaticSteppingEnabled = true;
    }

    private void FixedUpdate()
    {
        if (collectorAcademy.GetStepCount() % 3 == 0)
        {
            agents.ForEach(a => a.RequestDecision());
        }
    }

    private void EnvironmentReset()
    {
        goal.Reset();
        targets.Where(x => x.TargetHit).ToList().ForEach(t => t.Reset());
    }

    private Dictionary<string, float> GetBoundaryLimits()
    {
        var limits = new Dictionary<string, float>()
        {
            ["-X"] = 0,
            ["X"] = 0,
            ["-Z"] = 0,
            ["Z"] = 0
        };

        var boundaries = GameObject.FindGameObjectsWithTag("boundary");
        foreach (var boundary in boundaries)
        {
            float x = boundary.gameObject.transform.position.x;
            float z = boundary.gameObject.transform.position.z;
            float lengthAdjust = (float)Math.Sqrt(Math.Pow(boundary.gameObject.transform.position.y * 2, 2));

            // set X boundary
            if (x > limits["X"])
            {
                limits["X"] = x - lengthAdjust;
            }
            else if (x < limits["-X"])
            {
                limits["-X"] = x + lengthAdjust;
            }

            // set Z boundary
            if (z > limits["Z"])
            {
                limits["Z"] = z - lengthAdjust;
            }
            else if (z < limits["-Z"])
            {
                limits["-Z"] = z + lengthAdjust;
            }
        }

        return limits;
    }
    private Dictionary<string, float> GetGoalLimits()
    {
        var limits = new Dictionary<string, float>()
        {
            ["-X"] = -5f,
            ["X"] = 10f,
            ["-Z"] = -5f,
            ["Z"] = 5f
        };

        return limits;
    }
}