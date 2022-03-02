using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;

public class UI_Languages : MonoBehaviour
{
    [SerializeField] GameObject UI_LanguagesGameObject;
    [SerializeField] Transform  transLangGroup;
    [SerializeField] UIButtonLang  uiBtnLangPrefab;
    private bool isClickButton = false;

    string[] ARRAY_LANGUAGE_IN_ENGLISH = new string[14]
    {
        "English",
        "French",
        "German",
        "Indonesian",
        "Italian",
        "Japanese",
        "Korean",
        "Portuguese-BR",
        "Russian",
        "Simplified Chinese",
        "Spanish",
        "Thai",
        "Traditional Chinese",
        "Turkish",
    };

    string[] ARRAY_LANG_SPRITE_ID = new string[14]
    {
        "EN",
        "FR",
        "GE",
        "IND",
        "IT",
        "JP",
        "KR",
        "PT",
        "RU",
        "SC",
        "SP",
        "TH",
        "TC",
        "TUR",
    };

    void OnEnable()
    {
        if(transLangGroup.childCount == 0)
        {
            for(int i = 0; i < ARRAY_LANGUAGE_IN_ENGLISH.Length; i++)
            {
                UIButtonLang uiButtonLang = Instantiate(uiBtnLangPrefab, transLangGroup);
                uiButtonLang.SetTextLang(ARRAY_LANGUAGE_IN_ENGLISH[i]);
                uiButtonLang.SetSpriteLang(SpriteManager.Instance.GetLanguageSprite(ARRAY_LANG_SPRITE_ID[i]));
                if(ProfileMgr.Instance.Language == i)
                {
                    uiButtonLang.SetSelected();
                }
                else
                {
                    if(i == (int)Language.English)
                    {
                        uiButtonLang.SetUnselectedLangEN();
                    }
                    else
                    {
                        uiButtonLang.SetUnselected();
                    }
                }
                int index = i;
                uiButtonLang.GetComponent<Button>().onClick.AddListener(()=> SetLanguages(index));
            }
        }
        else
        {
            for(int i = 0; i < transLangGroup.childCount; i++)
            {
                if(ProfileMgr.Instance.Language == i)
                {
                    transLangGroup.GetChild(i).GetComponent<UIButtonLang>().SetSelected();
                }
                else
                {
                    if(i == (int)Language.English)
                    {
                        transLangGroup.GetChild(i).GetComponent<UIButtonLang>().SetUnselectedLangEN();
                    }
                    else
                    {
                        transLangGroup.GetChild(i).GetComponent<UIButtonLang>().SetUnselected();
                    }
                }
            }
        }
        isClickButton = true;
    }

    public void SetLanguages(int newLang)
    {  
        if(isClickButton)      
        {
            isClickButton = false;
            for(int i = 0; i < transLangGroup.childCount; i++)
            {
                if(newLang == i)
                {
                    transLangGroup.GetChild(i).GetComponent<UIButtonLang>().SetSelected();
                }
                else
                {
                    if(i == (int)Language.English)
                    {
                        transLangGroup.GetChild(i).GetComponent<UIButtonLang>().SetUnselectedLangEN();
                    }
                    else
                    {
                        transLangGroup.GetChild(i).GetComponent<UIButtonLang>().SetUnselected();
                    }
                }
            }
            Localization.Instance.SetLanguage((Language)newLang);
            Invoke("HideLanguageScreen", 0.4f);
        }
    }

    public void HideLanguageScreen()
    {
        UI_LanguagesGameObject.SetActive(false);
    }
}
