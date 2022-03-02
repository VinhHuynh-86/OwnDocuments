using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using gameoptions;

public class BaseCamera : MonoBehaviour
{
    #region Instance
    private static BaseCamera instance;
    public static BaseCamera Instance
    {
        get
        {
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    public Camera ovlUiCam;
    Camera baseCamera;
    void Awake()
    {
        instance = this;
        baseCamera = gameObject.GetComponent<Camera>();
        ovlUiCam = GameObject.Find("GameUICamera").GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var cameraData = baseCamera.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(ovlUiCam);

        bool isUsingPostProcress = (GameOptions.GetQualityLevel() == Define.QUALITIES_LEVEL.HIGH || GameOptions.IsHighProfile());
        cameraData.renderPostProcessing = isUsingPostProcress;
    }

    public void InsertGameViewcameraInStack(Camera gameCamera)
    {
        var cameraData = baseCamera.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Insert(0, gameCamera);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeactiveBaseCamera() // to avoid drop FPS hwen it render from far way stadium in selectSong-menu
    {
        baseCamera.gameObject.SetActive(false);
    }
    public void ActiveBaseCamera()
    {
        baseCamera.gameObject.SetActive(true);
    }
}
