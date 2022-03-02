using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBar : MonoBehaviour
{
    public GameObject HorizontalBarPrefab;

    // Start is called before the first frame update
    void Start()
    {
        float verticalLength = Board.instance.GetSquareVerticalLength();
        int rowNum = Board.instance.GetRowNumber();

        //Instantiate(HorizontalBarPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity)
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
