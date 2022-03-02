using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightStageController : MonoBehaviour
{
    public GameObject[] LightGroupList;
    public float timeSwitchMin = 3f;
    public float timeSwitchMax = 8f;

    //private variables
    Timer changeTime;
    int curLightIndex;

    void Start()
    {
        changeTime = new Timer();
        changeTime.SetDuration(Random.Range(timeSwitchMin, timeSwitchMax));
        curLightIndex = 0;
        ActiveLightIndex();
    }

    void Update()
    {
        changeTime.Update(Time.deltaTime);
        if(changeTime.JustFinished())
        {
            changeTime.SetDuration(Random.Range(timeSwitchMin, timeSwitchMax));
            curLightIndex++;
            if(curLightIndex >= LightGroupList.Length)
            {
                curLightIndex = 0;
            }
            ActiveLightIndex();
        }
    }

    void ActiveLightIndex()
    {
        for(int i = 0; i < LightGroupList.Length; i++)
        {
            LightGroupList[i].SetActive(i==curLightIndex);
        }
    }
}
