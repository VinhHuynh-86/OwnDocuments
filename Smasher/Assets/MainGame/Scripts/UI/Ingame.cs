
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using DG.Tweening;
using Defines;

public class Ingame : MonoBehaviour
{
    [SerializeField] private Text TextFood;
    [SerializeField] private Text TextQuestDescription;
    [SerializeField] private Text TextQuestCount;

    bool mIsStarted = false;


    void Awake()
    {
        Player.OnFoodCollected += OnFoodCollected;
        QuestMgr.OnStart += OnQuestStart;
        QuestMgr.OnCheck += OnQuestCheck;
        QuestMgr.OnCompleted += OnQuestCompleted;
    }

    void OnEnable()
    {
        if (!mIsStarted)
        {
            mIsStarted = true;
            return;
        }

        GameMgr.Instance.SetCameraDriven("Ingame");
    }

    void OnDisable()
    {

    }

    void Update()
    {

    }

    void OnFoodCollected(int current, int total)
    {
        TextFood.text = current + "/" + total;
        TextFood.transform.DOKill(true);
        TextFood.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 10, 0).SetEase(Ease.OutSine);
    }

    void OnQuestStart(QuestInfo quest)
    {
        TextQuestDescription.text = quest.Description.Replace("%d", "" + quest.Count);
        TextQuestCount.text = "0" + "/" + quest.Count;

        // UIPopup popup = UIPopup.GetPopup("Quest");
        // popup.Data.SetLabelsTexts("MISSION", TextQuestDescription.text);
        // popup.Show();
    }

    void OnQuestCheck(QuestInfo quest, int count)
    {
        TextQuestCount.text = "" + count + "/" + quest.Count;
        TextQuestCount.transform.DOKill(true);
        TextQuestCount.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 10, 0).SetEase(Ease.OutSine);
    }

    void OnQuestCompleted(int zone)
    {
        TextQuestDescription.text = "Mission Completed";
        TextQuestDescription.transform.DOKill(true);
        TextQuestDescription.transform.DOShakeRotation(1);
    }
}
