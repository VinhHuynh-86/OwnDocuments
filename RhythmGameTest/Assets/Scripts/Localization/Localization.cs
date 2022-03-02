using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Language
{
    NONE = -1,
    English = 0,
    French,
    German,
    Indonesian,
    Italian,
    Japanese,
    Korean,
    Portuguese,
    Russian,
    ChineseSimplified,
    Spanish,
    Thai,
    ChineseTraditional,
    Turkish,
}

enum EParseFileSteps
{
    ReadID,
    ReadValue
}

public class Localization
{
    private const char DELIMITER = '=';
    private const char LINE_END = '\n';
    private Language usedLanguage;
    private Dictionary<string, string> localizedText;
    private static Localization instance;

    public static Localization Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Localization();
            }
            return instance;
        }
    }


    public void Init()
    {
        localizedText = new Dictionary<string, string>();
        if(ProfileMgr.Instance.Language >= 0)
            SetLanguage((Language)ProfileMgr.Instance.Language, false);
        else
            SetLanguage(GetLanguageIDFromSystemLanguage(), false);

        LocalizationEvents.LOCALIZATION_SET_LANGUAGE += SetLanguage;
    }

    public static Language GetLanguageIDFromSystemLanguage()
    {
        Language lang = Language.English;
        switch (Application.systemLanguage)
        {
            case SystemLanguage.English:
                lang = Language.English;
                break;
            case SystemLanguage.French:
                lang = Language.French;
                break;
            case SystemLanguage.German:
                lang = Language.German;
                break;
            case SystemLanguage.Spanish:
                lang = Language.Spanish;
                break;
            case SystemLanguage.Russian:
                lang = Language.Russian;
                break;
            case SystemLanguage.Indonesian:
                lang = Language.Indonesian;
                break;
            case SystemLanguage.Thai:
                lang = Language.Thai;
                break;
            case SystemLanguage.Korean:
                lang = Language.Korean;
                break;
            case SystemLanguage.Japanese:
                lang = Language.Japanese;
                break;

            case SystemLanguage.ChineseTraditional:
                lang = Language.ChineseTraditional;
                break;
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.Chinese:
                lang = Language.ChineseSimplified;
                break;

            case SystemLanguage.Italian:
                lang = Language.Italian;
                break;

            case SystemLanguage.Portuguese:            
                lang = Language.Portuguese;
                break;
            case SystemLanguage.Turkish:            
                lang = Language.Turkish;
                break;
        }

        // return Language.Japanese;
        return lang;
    }

    public void SetLanguage(Language lan)
    {
        SetLanguage(lan, true);
    }

    public void SetLanguage(Language lan, bool isReload)
    {
        localizedText.Clear();
        usedLanguage = lan;
        ReLoadData(lan);
        FontManager.Instance.LoadFont(lan);
        if (isReload)
            UpdateAllText();
        
        ProfileMgr.Instance.Language = (int)lan;
        ProfileMgr.Instance.SaveSetting();
    }

    private void ReLoadData(Language lan)
    {
        string pathLang = "Localizations/" + lan.ToString();
        TextAsset textAsset = Resources.Load<TextAsset>(pathLang);

        if (textAsset == null)
        {
            Debug.LogError("[Localization] Missing language pack " + lan); 
            return;
        }

        string buffer = textAsset.text;
        int len = buffer.Length;
        EParseFileSteps step = EParseFileSteps.ReadID;
        StringBuilder tmpBuffer = new StringBuilder(128);
        string id = string.Empty, val = string.Empty;
        for (int i = 0; i < buffer.Length; i++)
        {
            char token = buffer[i];
            switch (step)
            {
                case EParseFileSteps.ReadID:
                    if (token.Equals(DELIMITER))
                    {
                        id = tmpBuffer.ToString().Trim();
                        tmpBuffer.Length = 0;
                        step = EParseFileSteps.ReadValue;
                        break;
                    }
                    tmpBuffer.Append(token);
                    break;
                case EParseFileSteps.ReadValue:
                    {
                        if (token.Equals(LINE_END))
                        {
                            val = tmpBuffer.ToString().Trim();
                            tmpBuffer.Length = 0;
                            val = val.Replace("\\n", "\n");
                            if (GetCurrentLanguage() == Language.Thai
                                && !IsLocalPushText(id)
                            )
                            {
                                val = ThaiFontAdjuster.Adjust(val);
                            }
                            localizedText.Add(id, val);
                            step = EParseFileSteps.ReadID;
                        }
                        tmpBuffer.Append(token);
                    }
                    break;
                default:
                    break;
            }
        }

        tmpBuffer.Length = 0;
        Resources.UnloadAsset(textAsset);
    }

    private void UpdateAllText()
    {
        LocalizationText[] localizationTexts = Object.FindObjectsOfType<LocalizationText>();
        for (int i = 0; i < localizationTexts.Length; i++)
        {
            localizationTexts[i].UpdateText();
        }
    }

    public string GetString(string key)
    {
        if(localizedText == null)
        {
            return "";
        }
        if (localizedText.ContainsKey(key))
        {
            if(localizedText[key].Contains("|"))
            {
                localizedText[key] = localizedText[key].Replace("|", "\x200B");
            }
            return localizedText[key];
        }
        else
            return key;
    }

    public Language GetCurrentLanguage()
    {
        return usedLanguage;
    }

    private const int MAX_NUMBER_LENGTH = 20;
    private char[] numberCharArray = new char[MAX_NUMBER_LENGTH];

    public string GetLocalizeNumber(int number)
    {
        char seperationOfThousand = '?';

        switch (GetCurrentLanguage())
        {
            case Language.English:
            case Language.Korean:
            case Language.ChineseSimplified:
            case Language.ChineseTraditional:
            case Language.Thai:
                seperationOfThousand = ',';
                break;

            case Language.Indonesian:
            case Language.German:
            case Language.Spanish:
                seperationOfThousand = '.';
                break;

            case Language.French:
            case Language.Russian:
                if (number >= 10000) seperationOfThousand = '\u00A0'; // special non-breaking space 
                break;
        }

        if (seperationOfThousand == '?')
            return number.ToString();

        int pos = MAX_NUMBER_LENGTH;
        int cnt = 0;

        while (true)
        {
            numberCharArray[--pos] = (char)(number % 10 + 48);
            number /= 10;

            if (number == 0) break;

            cnt++;
            if (cnt == 3)
            {
                numberCharArray[--pos] = seperationOfThousand;
                cnt = 0;
            }
        }

        return new string(numberCharArray, pos, MAX_NUMBER_LENGTH - pos);
    }

    bool IsLocalPushText(string TextId)
    {
        for(int i = 0; i < 3/*mLocalPushDays.Length*/; i++)
        {
            string containTxt = Localization.Instance.GetString("STR_PN"+(i+1));
            string completedTxt = Localization.Instance.GetString("STR_COMPLETED_PN"+(i+1));
            string headerTxt = Localization.Instance.GetString("STR_PN"+(i+1)+"_HEADER");
            if(TextId == containTxt || TextId == completedTxt || TextId == headerTxt)
            {
                return true;
            }
        }

        return false;
    }

}
