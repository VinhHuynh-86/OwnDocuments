
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;

public class Lose : MonoBehaviour
{
    [SerializeField] private Text Coin;
    [SerializeField] private Text CoinWatch;
    [SerializeField] private UIButton ButtonWatch;
    [SerializeField] private UIButton ButtonClaim;

    Timer mTimerDelay = new Timer();
    bool mIsStarted = false;

    void Start()
    {
        mTimerDelay.SetDuration(3);

        ButtonWatch.OnClick.OnTrigger.Event.AddListener(OnWatch);
        ButtonClaim.OnClick.OnTrigger.Event.AddListener(OnClaim);
    }

    void OnEnable()
    {
        if (!mIsStarted)
        {
            mIsStarted = true;
            return;
        }

        int coin = 0;//GameMgr.Instance.GetCurrentCoin();
        Coin.text = "+" + coin;
        CoinWatch.text = "+" + (coin * 2);

        mTimerDelay.Reset();
        ButtonClaim.gameObject.SetActive(false);
    }

    void OnDisable()
    {

    }

    void Update()
    {
        mTimerDelay.Update(Time.deltaTime);
        if (mTimerDelay.JustFinished())
        {
            ButtonClaim.gameObject.SetActive(true);
        }
    }

    void OnWatch()
    {
        // UnityAds.Instance.ShowRewardVideo(AdsRewardType.DoubleCoin);
    }

    void OnClaim()
    {
        // UnityAds.Instance.ShowInterstitial();
    }
}
