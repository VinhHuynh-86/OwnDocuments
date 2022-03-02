
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using Defines;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject ButtonWatch;

    bool mIsStarted = false;

    void Start()
    {

    }

    void OnEnable()
    {
        if (!mIsStarted)
        {
            mIsStarted = true;
            return;
        }

        ButtonWatch.SetActive(UnityAds.Instance.RewardVideoAvailable);
        GameMgr.Instance.SetCameraDriven("Menu");
    }

    void OnDisable()
    {

    }

    void Update()
    {

    }
}
