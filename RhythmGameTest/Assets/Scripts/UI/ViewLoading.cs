using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ViewLoading : MonoBehaviour
{
    public ItemSprites spritesLoading;
    public Image imgLoading;
    public Image imgLogo;

    public TextMeshProUGUI loadingProgressText;
    public Image progressBar;
    public Image fillProgressBar;
    public Animator loadingAnim;
    float cutOffValue;
    Texture2D bgTexture;
    public GameObject loadingUI;

    // private bool isDissolveEffectFinished;
    const float DISSOLVE_TIME = 2.5f;

    void Awake()
    {
        loadingProgressText.gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        if(!Game.Instance.m_IsInitData)
        {
            return;
        }
        
        if(Game.Instance)
        {
            Game.Instance.SetViewState(Define.VIEW.LOADING);
        }
        loadingUI.SetActive(true);
        // ResetDissolveEffect();

        loadingAnim.Play("UI_LoadingAnim");
        UpdateImageLoading();
    }

    // Update is called once per frame
    public void UpdateProgress(float percent)
    {
        if(loadingProgressText != null && Game.Instance != null)
        {
            //Game.Instance.GetLoadingProgress()
            // loadingProgressText.text = Mathf.RoundToInt(percent * 100) + "%";
            if(fillProgressBar != null)
            {
                fillProgressBar.fillAmount = percent;
            }
        }
    }

    public void HideProgessBar()
    {
        progressBar.gameObject.SetActive(false);
    }

    public void ShowProgessBar()
    {
        progressBar.gameObject.SetActive(true);
    }
    
    void Update()
    {
        UpdateBackKey();
    }

    public void UpdateBackKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AndroidToast.ShowCannotBack();
        }
    }

    void UpdateImageLoading()
    {
        switch(Game.Instance.GetPreViewState())
        {
            case Define.VIEW.INIT:
                imgLoading.sprite = spritesLoading.GetSprite("SPLASH");
            break;

            case Define.VIEW.SONG:
            case Define.VIEW.SETTING:
                if(Game.Instance.VenueID != null && Game.Instance.VenueID.Length > 0)
                {
                    imgLoading.sprite = spritesLoading.GetSprite(Game.Instance.VenueID);
                }
                else
                {
                    imgLoading.sprite = spritesLoading.GetSprite("BLANK");
                }
                imgLogo.gameObject.SetActive(false);
            break;

            default:                
                imgLoading.sprite = spritesLoading.GetSprite("BLANK");
                imgLogo.gameObject.SetActive(false);
            break;
        }
    }

    void OnDisable()
    {
        loadingUI.SetActive(false);   
    }
}
