using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;

public class Reward : MonoBehaviour
{
    [SerializeField] Image imgIcon;
    [SerializeField] TextMeshProUGUI textTitle;
    [SerializeField] private UIParticle particle;
    
    enum REWARD_TYPE : int
    {
        ARCHIVE = 0,
        SONG,
        COINS,
        VENUE,
        COSTUME,
        SPECIAL_MOVE,
    }

    public void Init(int rewardType, int coins = 0)
    {
        switch(rewardType)
        {
            case (int)REWARD_TYPE.ARCHIVE:
                imgIcon.sprite = SpriteManager.Instance.GetRewardSprite("ARCHIVE");
                textTitle.text = Localization.Instance.GetString("STR_ARCHIVE_UNLOCK");
            break;
            case (int)REWARD_TYPE.SONG:
                imgIcon.sprite = SpriteManager.Instance.GetRewardSprite("SONG");
                textTitle.text = Localization.Instance.GetString("STR_DIFFICULTY_UNLOCK");
            break;
            case (int)REWARD_TYPE.COINS:
                imgIcon.sprite = SpriteManager.Instance.GetRewardSprite("COINS");
                SetCoins(coins);
            break;
            case (int)REWARD_TYPE.VENUE:
                imgIcon.sprite = SpriteManager.Instance.GetRewardSprite("VENUE");
                textTitle.text = Localization.Instance.GetString("STR_NEW_TOUR");
            break;
            case (int)REWARD_TYPE.COSTUME:
                imgIcon.sprite = SpriteManager.Instance.GetRewardSprite("COSTUME");
                textTitle.text = Localization.Instance.GetString("STR_NEW_TOUR");
            break;
            case (int)REWARD_TYPE.SPECIAL_MOVE:
                imgIcon.sprite = SpriteManager.Instance.GetRewardSprite("SPECIAL_MOVE");
                textTitle.text = Localization.Instance.GetString("STR_NEW_TOUR");
            break;
        }
    }

    public void SetCoins(int coins)
    {
        string strCoins = Localization.Instance.GetString("STR_COINS").Replace("%d", "" + coins);
        textTitle.text = strCoins;
    }

    public void PlayEffect()
    {
        particle.Play();
    }
}
