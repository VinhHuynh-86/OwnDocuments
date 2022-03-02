using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SwipeRotate
{
    public Define.INPUT_STATUS swipe;
    public float angle;
}

public class SwipeNote : MonoBehaviour
{
    public Note parentNote;
    [SerializeField]
    public Vector3 rotateAxis = Vector3.forward;
    // public Define.INPUT_STATUS swipe = Define.INPUT_STATUS.NONE;
    [SerializeField]
    private List<SwipeRotate> swipeConfigs;
    [SerializeField]
    private GameObject megaSwipeArrow_L;
    [SerializeField]
    private GameObject megaSwipeArrow_R;
    [SerializeField]
    private GameObject normalSwipeNote_L;
    [SerializeField]
    private GameObject normalSwipeNote_R;
    public Dictionary<Define.INPUT_STATUS, float> dictionary;

    void Awake()
    {
        if (parentNote == null)
        {
            throw new System.MissingFieldException("parent note must not be null");
        }

        dictionary = new Dictionary<Define.INPUT_STATUS, float>();
        foreach (SwipeRotate config in swipeConfigs)
        {
            dictionary.Add(config.swipe, config.angle);
        }
    }

    void Start()
    {
        Rotate();
    }

    void OnEnable()
    {
        Rotate();
    }

    public void Rotate()
    {
        // Define.LANE_ON_SIDE isOnSide = parentNote.isOnSide;
        
        if (parentNote.SwipeRequire != Define.INPUT_STATUS.NONE)
        {
            if(parentNote.type == Define.NOTE_TYPE.MIGHTY_SWIPE)
            {
                normalSwipeNote_R.SetActive(false);
                normalSwipeNote_L.SetActive(false);

                megaSwipeArrow_L.gameObject.SetActive(false);
                megaSwipeArrow_R.gameObject.SetActive(false);

                switch(parentNote.SwipeRequire)
                {
                    case Define.INPUT_STATUS.SWIPE_LEFT:
                        megaSwipeArrow_L.gameObject.SetActive(true);
                    break;
                    case Define.INPUT_STATUS.SWIPE_RIGHT:
                        megaSwipeArrow_R.gameObject.SetActive(true);
                    break;
                }
            }
            else
            {
                normalSwipeNote_R.SetActive(false);
                normalSwipeNote_L.SetActive(false);

                megaSwipeArrow_L.gameObject.SetActive(false);
                megaSwipeArrow_R.gameObject.SetActive(false);

                switch(parentNote.SwipeRequire)
                {
                    case Define.INPUT_STATUS.SWIPE_LEFT:
                        normalSwipeNote_L.gameObject.SetActive(true);
                    break;
                    case Define.INPUT_STATUS.SWIPE_RIGHT:
                        normalSwipeNote_R.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
}
