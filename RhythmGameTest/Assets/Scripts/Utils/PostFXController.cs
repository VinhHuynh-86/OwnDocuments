using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostFXController : MonoBehaviour
{
    [SerializeField]Volume _postFXVolume;
    Bloom _bloomMasterVolume; 
    // Start is called before the first frame update
    void Awake()
    {
        this._postFXVolume.enabled = false;
    }
    void OnEnable()
    {

        if(gameoptions.GameOptions.GetQualityLevel() == Define.QUALITIES_LEVEL.HIGH || gameoptions.GameOptions.IsHighProfile())
        {
            this._postFXVolume.enabled = true;
            FindMasterVolumeAndUpdate();
        }
    }
    // Update is called once per frame
    void FindMasterVolumeAndUpdate()
    {
        Volume masterVolume = null;
        var volumeArr = GameObject.FindObjectsOfType<Volume>();
        for(int i = 0; i < volumeArr.Length; i++)
        {
            Volume child = volumeArr[i];
            if(masterVolume == null)
            {
                masterVolume = child;
            }
            else
            {
                if(child.priority > masterVolume.priority)
                    masterVolume = child;
            }
        }
        //update volume config to master volume
        //this._bloomMasterVolume = VolumeManager.instance.stack.GetComponent<Bloom>();
        if(!masterVolume.profile.TryGet<Bloom>(out _bloomMasterVolume))
        {
            _bloomMasterVolume = masterVolume.profile.Add<Bloom>();
        }
        Bloom bloomVolume;
        if(this._postFXVolume.profile.TryGet<Bloom>(out bloomVolume))
        {
            this._bloomMasterVolume.intensity.value = bloomVolume.intensity.value;
            this._bloomMasterVolume.intensity.overrideState = bloomVolume.intensity.overrideState;

            this._bloomMasterVolume.threshold.value = bloomVolume.threshold.value;
            this._bloomMasterVolume.threshold.overrideState = bloomVolume.threshold.overrideState;

            this._bloomMasterVolume.scatter.value = bloomVolume.scatter.value;
            this._bloomMasterVolume.scatter.overrideState = bloomVolume.scatter.overrideState;

            this._bloomMasterVolume.tint.value = bloomVolume.tint.value;
            this._bloomMasterVolume.tint.overrideState = bloomVolume.tint.overrideState;

            this._bloomMasterVolume.active = true;
        }
    }
}
