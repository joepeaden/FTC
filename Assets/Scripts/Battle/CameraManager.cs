using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance => _instance;
    private static CameraManager _instance;
    public static Camera MainCamera { get; private set; }
    [SerializeField] private CinemachineVirtualCamera _vCam;
    private float shakeTimerDuration = .1f;  
    private float shakeTimer;              
    private bool shakeTimerRunning = false;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("Too many - destroying one camera manager!");
            Destroy(gameObject);
        }

        _instance = this;

        MainCamera = Camera.main;
    }

    public void ShakeCamera()
    {
        _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 2;

        shakeTimer = shakeTimerDuration;
        shakeTimerRunning = true;
    }

    private void Update()
    {
        if (shakeTimerRunning)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                shakeTimerRunning = false;
                _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
            }
        }
    }
}
