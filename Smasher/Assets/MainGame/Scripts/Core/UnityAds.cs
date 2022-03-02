using System;
using UnityEngine;
using UnityEngine.Advertisements;

public enum AdsRewardType
{
    Head,
    Revive,
}

public class UnityAds : Singleton<UnityAds>, IUnityAdsListener
{
    [SerializeField] string AppleStoreGameId = "3208558";
    [SerializeField] string GoogleStoreGameId = "3208559";

    public static Action<AdsRewardType> OnRewardVideoWatched;
    public static Action<AdsRewardType> OnRewardVideoFailed;

    string mPlacementId = "rewardedVideo";
    AdsRewardType mRewardType;

    public bool RewardVideoAvailable
    {
        get { return Advertisement.IsReady(mPlacementId); }
    }

    void Start()
    {
        Advertisement.AddListener (this);
#if UNITY_ANDROID
        Advertisement.Initialize(GoogleStoreGameId);
#else
        Advertisement.Initialize(AppleStoreGameId);
#endif
    }

    public bool ShowInterstitial()
    {
        bool isReady = Advertisement.IsReady();
        if (isReady)
        {
            Advertisement.Show();
        }

        return isReady;
    }

    public void ShowRewardVideo(AdsRewardType type)
    {
        Advertisement.Show(mPlacementId);
        mRewardType = type;
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            if (OnRewardVideoWatched != null)
            {
                OnRewardVideoWatched(mRewardType);
            }
        }
        else if (showResult == ShowResult.Skipped)
        {
            if (OnRewardVideoFailed != null)
            {
                OnRewardVideoFailed(mRewardType);
            }
        }
        else if (showResult == ShowResult.Failed)
        {
            if (OnRewardVideoFailed != null)
            {
                OnRewardVideoFailed(mRewardType);
            }
        }
    }

    public void OnUnityAdsReady(string placementId)
    {

    }

    public void OnUnityAdsDidError(string message)
    {

    }

    public void OnUnityAdsDidStart(string placementId)
    {

    }

    public void OnDestroy()
    {
        Advertisement.RemoveListener(this);
    }
}
