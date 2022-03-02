using UnityEngine;

/// <summary>
/// Factory for prefabs of AudioObjects
/// </summary>
public class BeatFactory : GenericFactory<Note>
{
    [SerializeField]
    private Note template;

    public Note GetTemplate()
    {
        return template;
    }
}