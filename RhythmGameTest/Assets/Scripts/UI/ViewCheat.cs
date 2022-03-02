using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ViewCheat : MonoBehaviour
{
    public Text showHideDebugInfoText;
    public Text autoplayText;

    private void Start()
    {
        if(Define.ENABLE_CHEAT)
        {
            if(Define.IsShowDebugInfo)
            {
                showHideDebugInfoText.text = "Hide debug info";
            }
            else 
            {
                showHideDebugInfoText.text = "Show debug info";
            }
        }
    }
    public void OnClickDebugInfo()
    {
        if(Define.ENABLE_CHEAT)
        {
            Define.IsShowDebugInfo = !Define.IsShowDebugInfo;
            if(Define.IsShowDebugInfo)
            {
                showHideDebugInfoText.text = "Hide debug info";
            }
            else 
            {
                showHideDebugInfoText.text = "Show debug info";
            }
        }
    }

    public void OnClickAddCoin()
    {
        if(Define.ENABLE_CHEAT)
        {            
            ProfileMgr.Instance.Coin += 1000;
        }
    }

    public void OnClickSubCoin()
    {
        if(Define.ENABLE_CHEAT)
        {
            ProfileMgr.Instance.Coin -= 1000;
        }
    }

    public void UnlockAllSong()
    {
        ProfileMgr.Instance.UnlockAllSong();
        // Scene scene = SceneManager.GetActiveScene();
        // SceneManager.LoadScene(scene.name);
    }

    public void ExitMenu()
    {
        if(Time.timeScale == 1)
        {
            GameEventMgr.SendEvent("back_to_main_menu");
        }
        else
        {
            Debug.Log("IIIIIIIIIIIIIIIIIIIIIIIIIIIIII");
            GameEventMgr.SendEvent("back_to_ingame_menu");
        }
    }

    public void EnableTutorial()
    {
        ProfileMgr.Instance.EnableTutorial();
        ProfileMgr.Instance.Save();
        Application.Quit();
    }

    public void EnableAutoplay()
    {
        if(Define.ENABLE_CHEAT)
        {
            if(Define.ENABLE_AUTOPLAY)
            {
                Define.ENABLE_AUTOPLAY = false;
                autoplayText.text = "Enable Autoplay";
            }
            else 
            {
                Define.ENABLE_AUTOPLAY = true;
                autoplayText.text = "Disable Autoplay";
            }
        }
    }

    public void UnlockGame()
    {
        ProfileMgr.Instance.IsGameUnlocked = true;
        ProfileMgr.Instance.Save();
    }

    public void OnClickGodEyes()
    {
        GodsEye godEyes = GameObject.FindObjectOfType<GodsEye>();
        if(godEyes != null)
        {
            godEyes.SetActive(!godEyes.isActive);
        }
    }


    public void OnClickGC()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
