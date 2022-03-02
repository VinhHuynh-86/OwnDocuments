using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasResolution : MonoBehaviour
{
    public CanvasScaler canvasScaler;
    // Start is called before the first frame update
    void Start()
    { 
        canvasScaler = GetComponent<CanvasScaler>();       
        float raitoAspect = (float)Screen.width / (float)Screen.height;
        float ratioDefault = 16f/9f;
        
        if(raitoAspect > ratioDefault)
        {
            canvasScaler.matchWidthOrHeight = 1;            
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 0;            
        }
    }
}
