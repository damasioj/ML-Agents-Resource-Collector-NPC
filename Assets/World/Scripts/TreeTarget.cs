using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTarget : BaseTarget
{
    private void OnTriggerEnter(Collider other)
    {
        TargetHit = true;
    }
}
