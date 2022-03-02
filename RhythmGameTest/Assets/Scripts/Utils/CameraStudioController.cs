/*
Set this on an empty game object positioned at (0,0,0) and attach your active camera.
The script only runs on mobile devices or the remote app.
*/

using UnityEngine;
using UnityEngine.UI;

class CameraStudioController : MonoBehaviour
{
    // private Touch touch;
    // private float scaleSpeed = 0.1f;
    // private float speed = 0.01f;

    // private void Update()
    // {
    //     if(Input.touchCount == 2)
    //     {
    //         Touch firstTouch = Input.GetTouch(0);
    //         Touch secondTouch = Input.GetTouch(1);
    //         Vector2 firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
    //         Vector2 secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;
    //         float prevMagnitude = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
    //         float curMagnitude = (firstTouch.position - secondTouch.position).magnitude;
    //         float diff = curMagnitude - prevMagnitude;
    //         Zoom(diff*scaleSpeed);
    //         return;
    //     }

    //     if(Input.touchCount > 0)
    //     {
    //         touch = Input.GetTouch(0);
    //         if(touch.phase == TouchPhase.Moved)
    //         {
    //             transform.localPosition -= new Vector3(touch.deltaPosition.x * speed, touch.deltaPosition.y * speed, 0);
    //         }
    //     }
    // }

    // void Zoom(float increasement)
    // {
    //     GetComponent<Camera>().fieldOfView -= increasement;
    // }
}