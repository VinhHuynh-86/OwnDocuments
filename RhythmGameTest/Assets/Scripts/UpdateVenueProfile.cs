using UnityEngine;
using gameoptions;
using System.Collections.Generic;

public class UpdateVenueProfile : MonoBehaviour
{
    #region Instance
    private static UpdateVenueProfile instance;
    public static UpdateVenueProfile Instance
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

    public GameObject directLight;
    public GameObject pointLight;
    public GameObject spotLight;
    public Renderer[] objectMaterialList;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Debug.Log("GameOptions.GetQualityLevel(): "+GameOptions.GetQualityLevel());
        
        switch(GameOptions.GetQualityLevel())
        {
            case Define.QUALITIES_LEVEL.VERY_LOW:
            case Define.QUALITIES_LEVEL.LOW:
                // if(directLight) directLight.SetActive(true);
                // if(pointLight) pointLight.SetActive(false);
                // if(spotLight) spotLight.SetActive(false);
                // RenderSettings.fog = false;
                
                for(int i = 0; i < objectMaterialList.Length; i++)
                {
                    if(objectMaterialList[i] != null)
                    {
                        Material tempMaterial = new Material(objectMaterialList[i].material);
                        if(tempMaterial != null)
                        {
                            tempMaterial.shader = Shader.Find("Universal Render Pipeline/Unlit");
                            objectMaterialList[i].material = tempMaterial;
                        }
                    }
                }
                break;
            case Define.QUALITIES_LEVEL.VERY_HIGH:
            case Define.QUALITIES_LEVEL.ULTRA:
                // if(directLight) directLight.SetActive(false);
                // if(pointLight) pointLight.SetActive(true);
                // if(spotLight) spotLight.SetActive(true);
                break;
            default:
                // if(directLight) directLight.SetActive(false);
                // if(pointLight) pointLight.SetActive(true);
                // if(spotLight) spotLight.SetActive(false);
                break;
        }
    }
}
