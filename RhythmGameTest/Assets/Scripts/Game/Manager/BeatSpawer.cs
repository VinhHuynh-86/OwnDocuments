using System.Collections.Generic;
using UnityEngine;

public class BeatSpawer : MonoBehaviour
{
    [SerializeField]
    public BeatFactory beatFactory;
    public Note GenerateBeat(float duration)
    {
        // Get all values from template
        Note template = beatFactory.GetTemplate();
        // List<CameraLookAt> templateCameraLookAtResult = new List<CameraLookAt>();
        // template.GetComponentsInChildren<CameraLookAt>(true, templateCameraLookAtResult);

        // Get all values from instance
        Note instance = beatFactory.GetNewInstance(template.transform.position, template.transform.localRotation, template.transform.localScale, transform);
        // List<CameraLookAt> instanceCameraLookAtResult = new List<CameraLookAt>();
        // instance.GetComponentsInChildren<CameraLookAt>(true, instanceCameraLookAtResult);

        // copy value from template to instance
        instance.duration = duration;
        instance.target = template.target;
        // for (int i = 0; i < instanceCameraLookAtResult.Count; i++)
        // {
        //     instanceCameraLookAtResult[i].m_target = templateCameraLookAtResult[i].m_target;
        // }

        return instance;
    }

    public BeatFactory GetFactory()
    {
        return beatFactory;
    }
    
    public void ResetBeatInfo(Note note)
    {
        // Get all values from template
        // Note template = beatFactory.GetTemplate();
        // List<CameraLookAt> templateCameraLookAtResult = new List<CameraLookAt>();
        // template.GetComponentsInChildren<CameraLookAt>(true, templateCameraLookAtResult);

        // // Get all values from instance
        // List<CameraLookAt> instanceCameraLookAtResult = new List<CameraLookAt>();
        // note.GetComponentsInChildren<CameraLookAt>(true, instanceCameraLookAtResult);

        // // copy value from template to instance
        // note.target = template.target;
        // for (int i = 0; i < instanceCameraLookAtResult.Count; i++)
        // {
        //     instanceCameraLookAtResult[i].m_target = templateCameraLookAtResult[i].m_target;
        // }
    }
}