using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeCountdown : MonoBehaviour
{
    public ViewPauseMenu pauseMenu;
    public void ResumeGame()
    {
        if(!pauseMenu.IsIntterupted())
        {
            pauseMenu.ResumeGame();
        }
    }

    public void PlaySFX()
    {
        SFXManager.Instance.Play(Define.SFX.UI_UNPAUSE_321);
    }
}
