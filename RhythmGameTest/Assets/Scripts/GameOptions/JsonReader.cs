using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class JsonReader
{
    // Start is called before the first frame update
    public JSONNode jsonContent;
    public void LoadDataJson(string fileName)
    {
        TextAsset textData = Resources.Load(fileName) as TextAsset;
        jsonContent = JSON.Parse(textData.text);
    }
}
