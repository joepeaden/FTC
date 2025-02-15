using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that will disable a GameObject after a timeout, for things like returning to object pool
/// </summary>
public class DisableAfterTimeout : MonoBehaviour
{
    private const float TIMEOUT = 5;
    private float timer;

    private void OnEnable()
    {
        timer = TIMEOUT;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
