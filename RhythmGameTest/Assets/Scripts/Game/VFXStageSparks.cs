using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VFXStageSparks : MonoBehaviour
{
    public ParticleSystem[] childParticles;
    private float defaultEmission = 25f;
    private ParticleSystem curPS;

    void Start()
    {
        curPS = GetComponent<ParticleSystem>();
    }

    public void PlaySlow()
    {
        curPS.Play();
        float emissionVal = 0f;
        DOTween.To(()=> emissionVal, x => emissionVal = x, defaultEmission, 1f).OnUpdate(() => {
            foreach(ParticleSystem ps in childParticles)
            {
                ParticleSystem.EmissionModule psEmission = ps.emission;
                psEmission.rateOverTime = emissionVal;
            }
        });
    }
    
    public void EndSlow()
    {
        float emissionVal = defaultEmission;
        DOTween.To(()=> emissionVal, x => emissionVal = x, 0f, 2f).OnUpdate(() => {
            foreach(ParticleSystem ps in childParticles)
            {
                ParticleSystem.EmissionModule psEmission = ps.emission;
                psEmission.rateOverTime = emissionVal;
            }
        }).OnComplete(() => {
            foreach(ParticleSystem ps in childParticles)
            {
                ParticleSystem.EmissionModule psEmission = ps.emission;
                psEmission.rateOverTime = defaultEmission;
            }
            curPS.Stop();
        });
    }

    public void Stop()
    {
        curPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
