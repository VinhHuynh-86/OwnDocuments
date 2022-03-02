using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FontManager : MonoBehaviour
{
    public static FontManager Instance = null;
    private TMP_FontAsset fontBold = null;
    private TMP_FontAsset fontItalic = null;
    public TMP_FontAsset fontBoldEN;
    public TMP_FontAsset fontItalicEN;
    private Language prevLang = Language.English;

    string nameFontENBold = "Poppins-Bold SDF";
    string nameFontENItalic = "Poppins-ExtraBoldItalic SDF";

    string nameFontJPBold = "NotoSansJP-Bold SDF";
    string nameFontJPItalic = "NotoSansJP-Medium SDF";

    string nameFontKRBold = "NotoSansKR-Bold SDF";
    string nameFontKRItalic = "NotoSansKR-Medium SDF";

    string nameFontRUBold = "Ubuntu-Bold SDF";
    string nameFontRUItalic = "Ubuntu-BoldItalic SDF";

    string nameFontTHBold = "THSarabunNew Bold SDF";
    string nameFontTHItalic = "THSarabunNew BoldItalic SDF";

    string nameFontSCBold = "NotoSansSC-Bold SDF";
    string nameFontSCItalic = "NotoSansSC-Medium SDF";

    string nameFontTCBold = "NotoSansTC-Bold SDF";
    string nameFontTCItalic = "NotoSansTC-Medium SDF";

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void LoadFont(Language lang)
    { 
        if(fontBold == null)
        {
            fontBold = fontBoldEN;
        }
        if(fontItalic == null)
        {
            fontItalic = fontItalicEN;
        }
        
        if(!IsChangeFont(lang))       
        {
            return;
        }

        if(!IsEnglish(prevLang))
        {
            Resources.UnloadAsset(fontBold);
            Resources.UnloadAsset(fontItalic);
        }
        prevLang = lang;
        string nameFontBold = nameFontENBold;
        string nameFontItalic = nameFontENItalic;
        switch(lang)
        {
            case Language.Japanese:
                nameFontBold = nameFontJPBold;
                nameFontItalic = nameFontJPItalic;
            break;
            case Language.Korean:
                nameFontBold = nameFontKRBold;
                nameFontItalic = nameFontKRItalic;
            break;
            case Language.Russian:
                nameFontBold = nameFontRUBold;
                nameFontItalic = nameFontRUItalic;
            break;
            case Language.ChineseSimplified:
                nameFontBold = nameFontSCBold;
                nameFontItalic = nameFontSCItalic;
            break;
            case Language.Thai:
                nameFontBold = nameFontTHBold;
                nameFontItalic = nameFontTHItalic;
            break;
            case Language.ChineseTraditional:
                nameFontBold = nameFontTCBold;
                nameFontItalic = nameFontTCItalic;
            break;
        }
        if(!IsEnglish(prevLang))
        {
            string pathFontBold = "Fonts & Materials/" + nameFontBold;
            string pathFontItalic = "Fonts & Materials/" + nameFontItalic;
            fontBold = Resources.Load<TMP_FontAsset>(pathFontBold);
            fontItalic = Resources.Load<TMP_FontAsset>(pathFontItalic);
        }
        else
        {
            fontBold = fontBoldEN;
            fontItalic = fontItalicEN;
        }
    }

    public bool IsEnglish(Language lang)
    {
        return (lang == Language.English
            || lang == Language.French
            || lang == Language.German
            || lang == Language.Indonesian
            || lang == Language.Italian
            || lang == Language.Portuguese
            || lang == Language.Turkish
            || lang == Language.Spanish
            );
    }

    public bool IsChangeFont(Language curLang)
    {
        if((IsEnglish(curLang) && !IsEnglish(prevLang))
            || (!IsEnglish(curLang) && curLang != prevLang)
        )
        {
            return true;
        }
        return false;
    }

    public TMP_FontAsset GetFont(Define.FONT font)
    {
        if(font == Define.FONT.BOLD)
        {
            return fontBold;
        }
        return fontItalic;
    }
}
