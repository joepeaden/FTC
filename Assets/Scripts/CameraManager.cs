using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static Camera MainCamera { get; private set; }

    private void Awake()
    {
        MainCamera = Camera.main;
    }
}
