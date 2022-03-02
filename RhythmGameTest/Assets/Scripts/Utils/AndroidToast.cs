using UnityEngine;
using Doozy.Engine.UI;

public static class AndroidToast
{
    private static string _toastString;
    private static AndroidJavaClass _unityPlayer;
    private static AndroidJavaClass UnityPlayer
    {
        get
        {
            if (_unityPlayer == null)
            {
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            }
            return _unityPlayer;
        }
    }

    const float TIME_SHOW_POPUP = 2f;

    public static void ShowToast(string toastString)
    {
        _toastString = toastString;
        var currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(ShowToast));
    }

    private static void ShowToast()
    {
        var currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", _toastString);
        AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
        toast.Call("show");
    }

    public static void ShowCannotBack()
    {
        if(GameUtils.Instance.isShowCannotBack)
        {
            return;
        }

        GameUtils.Instance.isShowCannotBack = true;
        GameUtils.Instance.timerControl.SetDuration(TIME_SHOW_POPUP);
        // #if UNITY_EDITOR
        // Debug.Log("Can not back this state");
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_CANNOT_BACK);
        popup.Show();
        // #else
        // string text = Localization.Instance.GetString("STR_EXIT_ANDROID");
        // AndroidToast.ShowToast(text);
        // #endif
    }
}
