using UnityEngine;

/// <summary>
/// Factory design pattern with generic twist!
/// </summary>
public class GenericFactory<T> : MonoBehaviour where T : MonoBehaviour
{
    // Reference to prefab of whatever type.
    [SerializeField]
    private T prefab;

    public T GetNewInstance()
    {
        GameObject obj = prefab.gameObject.Spawn();
        return obj.GetComponent<T>();
    }

    public T GetNewInstance(Vector3 worldPosition)
    {
        GameObject obj = prefab.gameObject.Spawn(worldPosition);
        return obj.GetComponent<T>();
    }

    public T GetNewInstance(Transform parent)
    {
        GameObject obj = prefab.gameObject.Spawn(parent);
        return obj.GetComponent<T>();
    }

    public T GetNewInstance(Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null, bool useLocalPosition = false, bool useLocalRotation = false)
    {
        GameObject obj = prefab.gameObject.Spawn(position, rotation, scale, parent, useLocalPosition, useLocalRotation);
        return obj.GetComponent<T>();
    }
}