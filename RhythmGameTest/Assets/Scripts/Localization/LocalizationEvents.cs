public static class LocalizationEvents
{
    public delegate void LanguageEvent(Language language);
    public static event LanguageEvent LOCALIZATION_SET_LANGUAGE;
    public static void CallChangeLanguage(Language newLanguage)
    {
        LOCALIZATION_SET_LANGUAGE?.Invoke(newLanguage);
    }
}