using UnityEngine;

// Short phone vibration (when Larry bumps into a bush or stone). Can be turned off in Settings
// (PlayerPrefs "Vibration", the second row of RestartOfferToggle). Does nothing in the Editor.
public static class Haptics
{
    public const string Key = "Vibration";
    private const long BumpMilliseconds = 40;

    public static bool Enabled
    {
        get => PlayerPrefs.GetInt(Key, 1) == 1;
        set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static void Bump()
    {
        if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // A short pulse; Handheld.Vibrate is about half a second, too long for a bump.
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                if (version.GetStatic<int>("SDK_INT") >= 26)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", BumpMilliseconds, -1))
                    {
                        vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", BumpMilliseconds);
                }
            }
        }
        catch (System.Exception)
        {
            Handheld.Vibrate(); // also makes Unity add the VIBRATE permission to the manifest
        }
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}
