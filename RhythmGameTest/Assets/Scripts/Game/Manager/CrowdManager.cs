using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    public static CrowdManager Instance = null;
    public GameObject audience;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }       
    }

    public void ShowAudience()
    {
        audience.SetActive(true);
    }

    public void HideAudience()
    {
        audience.SetActive(false);
    }
}
