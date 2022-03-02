using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Chocolate : MonoBehaviour
{
    [SerializeField] AudioSource AudioBite;
    [SerializeField] AudioSource AudioPop;
    public static Action OnCollect;

    int mHp = 5;
    bool mIsHurt = false;

    void Start()
    {

    }

    void Update()
    {

    }

    void RandomDrop()
    {

    }

    public void Hurt(int damage)
    {
        if (!mIsHurt)
        {
            Material m = GetComponentInChildren<MeshRenderer>().material;
            m.DOKill(true);
            m.DOColor(Color.white, "_EmissionColor", 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                mHp -= damage;
                if (mHp <= 0)
                {
                    AudioPop.Play();
                    gameObject.SetActive(false);
                    OnCollect();
                }
                mIsHurt = false;
            });

            AudioBite.Play();
            mIsHurt = true;
        }
    }
}
