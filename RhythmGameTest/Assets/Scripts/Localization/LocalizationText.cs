using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//[RequireComponent(typeof(Text))]
public class LocalizationText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Text normalText;
    public Define.FONT font;
    public bool isUpdateLoc = true;

    // Use this for initialization
    private void Start()
    {
        this.textMesh = GetComponent<TextMeshProUGUI>();
        if (this.textMesh == null)
        {
            this.normalText = GetComponent<Text>();
            if (this.normalText == null)
                Debug.LogError("Can't find Text Component in " + gameObject.transform.parent.name + "/" + name);
        }

        UpdateText();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (this.textMesh == null)
        {
            if (this.normalText != null)
            {
                this.normalText.text = Localization.Instance.GetString(name);
            }
        }
        else
        {
            if(isUpdateLoc)            
            {
                this.textMesh.text = Localization.Instance.GetString(name);
            }
            this.textMesh.font = FontManager.Instance.GetFont(font);
           
        }
    }
}