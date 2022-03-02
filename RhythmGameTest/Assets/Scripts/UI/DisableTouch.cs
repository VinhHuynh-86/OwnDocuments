using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableTouch : MonoBehaviour
{
    public GameObject loadingIcon;

    void Start()
    {
        loadingIcon.SetActive(false);
    }
    public void SetEnable(bool isEnable, bool hasLoadingIcon = false)
    {
        gameObject.SetActive(isEnable);
        loadingIcon.SetActive(isEnable? hasLoadingIcon : false);
    }

    public bool IsEnable()
    {
        return gameObject.activeSelf;
    }

    public bool HasAnyTouch()
    {
        return Input.GetMouseButtonDown(0);
    }
}
