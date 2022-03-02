using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BG_Control : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Canvas masterCanvas;
    void Start()
    {
        RectTransform rectBG = gameObject.GetComponent<RectTransform>();
        RectTransform rectCV = masterCanvas.gameObject.GetComponent<RectTransform>();
        float scale = rectCV.sizeDelta.x / rectBG.sizeDelta.x;
        rectBG.sizeDelta = new Vector2(rectBG.sizeDelta.x * scale, rectBG.sizeDelta.y * scale);
    }
}
