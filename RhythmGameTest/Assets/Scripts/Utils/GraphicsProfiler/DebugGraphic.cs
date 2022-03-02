using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGraphic : MonoBehaviour
{
    public void SetFPS(int fps)
    {
        Application.targetFrameRate = fps;
    }
}
