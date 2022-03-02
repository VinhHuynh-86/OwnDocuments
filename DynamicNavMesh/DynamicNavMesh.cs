using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class DynamicNavMesh : MonoBehaviour
{
    NavMeshData m_NavMesh;
    AsyncOperation m_Operation = null;
    MeshFilter mesh;
    List<NavMeshBuildSource> m_Sources = new List<NavMeshBuildSource>();
    NavMeshDataInstance m_Instance;
    NavMeshBuildSettings defaultBuildSettings;
    Bounds bounds;
    public NavMeshLink[] navMeshLinkList;
    NavMeshLinkInstance m_LinkInstance = new NavMeshLinkInstance();
    public Vector3 m_Size = new Vector3(1.0f, 1.0f, 1.0f);
    private void Start() {
        m_NavMesh = new NavMeshData();
        m_Instance = NavMesh.AddNavMeshData(m_NavMesh);
        mesh = GetComponent<MeshFilter>();
        
        defaultBuildSettings = NavMesh.GetSettingsByID(0);
        bounds = QuantizedBounds();

        // navMeshLink = GetComponent<NavMeshLink>();
    }
    private void Update() {
        if(m_Operation == null || m_Operation.isDone)
        {
            UpdateNavMesh(true);
        }
    }
    void UpdateNavMesh(bool asyncUpdate = false)
    {
        var m = mesh.sharedMesh;
        var s = new NavMeshBuildSource();
        s.shape = NavMeshBuildSourceShape.Mesh;
        s.sourceObject = m;
        s.transform = mesh.transform.localToWorldMatrix;
        s.area = 0;
        m_Sources.Clear();
        m_Sources.Add(s);
        bounds = QuantizedBounds();

        if (asyncUpdate)
            m_Operation = NavMeshBuilder.UpdateNavMeshDataAsync(m_NavMesh, defaultBuildSettings, m_Sources, bounds);
        else
            NavMeshBuilder.UpdateNavMeshData(m_NavMesh, defaultBuildSettings, m_Sources, bounds);

        if(navMeshLinkList != null)
        {
            for(int v = 0; v < navMeshLinkList.Length; ++v)
            {
                navMeshLinkList[v].UpdateLink();
            }
        }
    }


    static Vector3 Quantize(Vector3 v, Vector3 quant)
    {
        float x = quant.x * Mathf.Floor(v.x / quant.x);
        float y = quant.y * Mathf.Floor(v.y / quant.y);
        float z = quant.z * Mathf.Floor(v.z / quant.z);
        return new Vector3(x, y, z);
    }

    Bounds QuantizedBounds()
    {
        // Quantize the bounds to update only when theres a 100% change in size
        var center = transform.position;
        return new Bounds(Quantize(center, 0.01f * m_Size), m_Size);
    }

    void OnDisable()
    {
        // Unload navmesh and clear handle
        m_Instance.Remove();
    }

    void OnDrawGizmosSelected()
    {
        if (m_NavMesh)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(m_NavMesh.sourceBounds.center, m_NavMesh.sourceBounds.size);
        }

        Gizmos.color = Color.yellow;
        var bounds = QuantizedBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.green;
        var center = transform.position;
        Gizmos.DrawWireCube(center, m_Size);
    }
}
