using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Camera shakeCamera;
    public float shakeDuration;
    public float shakeDecreaseFactor;
    public float shakeAmount;
    public bool useVibration;
    public bool isDebug;
    public KeyCode key;

    private Vector3 oldPosition;
    private bool isShaking;
    private float shakeTimeRemaing;

    void Start()
    {
        isShaking = false;
        if (shakeCamera != null)
        {
            oldPosition = shakeCamera.transform.localPosition;
        }
    }

    void Update()
    {
        // for testing
        if (isDebug)
        {
            if (Input.GetKeyDown(key))
            {
                Shake();
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isShaking)
        {
            shakeCamera.transform.localPosition = oldPosition + Random.insideUnitSphere * shakeAmount;
            shakeTimeRemaing -= Time.deltaTime * shakeDecreaseFactor;

            if (shakeTimeRemaing < 0 || GameManager.Instance.IsGamePaused())
            {
                shakeCamera.transform.localPosition = oldPosition;
                isShaking = false;
            }
        }
    }

    public void Shake()
    {
        isShaking = true;
        shakeTimeRemaing = shakeDuration;

        // if (useVibration)
        // {
        //     Handheld.Vibrate();
        // }
    }
}
