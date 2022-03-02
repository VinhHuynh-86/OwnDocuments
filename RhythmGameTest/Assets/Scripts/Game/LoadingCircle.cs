using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    float angle = -10;
    float time = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time < 1)
        {
            angle -= 0.25f;
        }
        else if(time < 2)
        {
            angle += 0.5f;
            if(angle >= 0)
            {
                angle = -0.1f;
            }
        }
        else
        {
            angle = -10;
            time = 0;
        }
        this.transform.RotateAround(this.transform.transform.localPosition, new Vector3(0, 0, 1), angle);
    }
}
