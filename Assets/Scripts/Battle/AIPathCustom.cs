using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Events;

public class AIPathCustom : AIPath
{
    public UnityEvent OnDestinationReached = new ();

    public override void OnTargetReached()
    {
        base.OnTargetReached();
        OnDestinationReached.Invoke();
    }

    private void OnDestroy()
    {
        OnDestinationReached.RemoveAllListeners();
    }
}
