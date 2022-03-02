using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TopBarUI : MonoBehaviour
{
    public TextMeshProUGUI textCoin;
    public Animator animatorCoin;
    
    const float TIME_DURATION = 0.5f;

    void OnEnable()
    {
        if(Game.Instance.GetViewState() != Define.VIEW.RESULT)
            UpdateResources();
    }

    public void UpdateResources()
    {
        textCoin.text = ProfileMgr.Instance.Coin.ToString();
    }

    public void UpdateFakeResourcesEndGame(int oldDisc, int oldCoins)
    {
        textCoin.text = oldCoins.ToString();
    }

    public void UpdateCoin(int coin)
    {
        UpdateCoin(coin, TIME_DURATION);
    }

    public void UpdateCoin(int coin, float duration)
    {
        GameUtils.Instance.UpdateAddValue(textCoin, coin, duration);
    }

    public void PlayCoinLose()
    {
        animatorCoin.Play("Coin_Lose");
    }
}
