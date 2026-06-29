namespace Regulas.MauiApp.Services;

public static class ApiBaseUrl
{
    private const string PreferenceKey = "RegulasApiBaseUrl";

    public static string Current
    {
        get => Preferences.Get(PreferenceKey, PlatformDefault());
        set => Preferences.Set(PreferenceKey, value.Trim());
    }

    public static string Default => PlatformDefault();

    public static void Reset()
    {
        Preferences.Remove(PreferenceKey);
    }

    private static string PlatformDefault()
    {
#if ANDROID
        return "http://10.0.2.2:5052";
#else
        return "http://localhost:5052";
#endif
    }
}
