using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public float speed = 20f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0, Space.Self);

        // switch(rotateType)
        // {
        //     case ROTATE_TYPE.AXIS_X:
        //         transform.Rotate(speed * Time.deltaTime, 0, 0, Space.Self);
        //         break;
        //     case ROTATE_TYPE.AXIS_Y:
        //         transform.Rotate(0, speed * Time.deltaTime, 0, Space.Self);
        //         break;
        //     case ROTATE_TYPE.AXIS_Z:
		        
        //     break;
        // }        
	}
}
