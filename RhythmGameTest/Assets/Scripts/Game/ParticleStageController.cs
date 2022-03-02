using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleStageController : MonoBehaviour
{
    public ParticleSystem specialmoveStartParticle;
    public VFXStageSparks specialmoveLoopParticle;

    public void StartFireParticle()
    {
        specialmoveStartParticle.Play();
    }

    public void StartSpecialMoveParticle()
    {
        StartCoroutine(PlayBeginningSMParticle());
        StartCoroutine(PlaySecondSMParticle());
    }
    public void StopSpecialMoveParticle(bool isStopImmediately = false)
    {
        if(isStopImmediately)
        {
            specialmoveLoopParticle.Stop();
        }
        else
        {
            specialmoveLoopParticle.EndSlow();
        }
    }

    public IEnumerator PlayBeginningSMParticle()
    {
        yield return null;
        specialmoveStartParticle.Play();
        yield return null;
    }
    public IEnumerator PlaySecondSMParticle()
    {
        yield return null;
        specialmoveLoopParticle.PlaySlow();
        yield return null;
    }
}
