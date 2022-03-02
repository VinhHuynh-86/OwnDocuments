using UnityEngine;
using System.Collections.Generic;
public class CameraResolution : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {        
        float raitoAspect = (float)Screen.width / (float)Screen.height;

        // for <=4:3
        if(raitoAspect <= 1.4f)
        {
            Camera camera = GetComponent<Camera>();
            camera.fieldOfView = 45f;
        }
    }
}