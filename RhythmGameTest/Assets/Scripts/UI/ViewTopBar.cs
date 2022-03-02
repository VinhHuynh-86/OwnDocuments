using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ViewTopBar : MonoBehaviour
{
    public TextMeshProUGUI textCoin;
    public TextMeshProUGUI textDisc;

    void Update()
    {
        textCoin.text = ProfileMgr.Instance.Coin.ToString();
        textDisc.text = Database.GetCurrentDisc().ToString();
    }
}
