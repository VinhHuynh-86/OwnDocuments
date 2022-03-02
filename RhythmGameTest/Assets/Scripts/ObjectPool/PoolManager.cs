/**
 *    Copyright (C) 2017 tongtunggiang.com
 *
 *    This program is free software: you can redistribute it and/or  modify
 *    it under the terms of the GNU Affero General Public License, version 3,
 *    as published by the Free Software Foundation.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU Affero General Public License for more details.
 *
 *    You should have received a copy of the GNU Affero General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private static Dictionary<GameObject, Pool> currentPools;

    private void Awake()
    {
        currentPools = new Dictionary<GameObject, Pool>();

        Pool[] childrenPools = gameObject.GetComponentsInChildren<Pool>();
        for (int i = 0; i < childrenPools.Length; i++)
        {
            currentPools[childrenPools[i].Prefab] = childrenPools[i];
        }
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null, bool useLocalPosition = false, bool useLocalRotation = false)
    {
        // Locate the pool that provides the prefab
        if (!currentPools.ContainsKey(prefab))
        {
            return SpawnNonPooledObject(prefab, position, rotation, scale, parent, useLocalPosition, useLocalRotation);
        }

        return currentPools[prefab].Spawn(position, rotation, scale, parent, useLocalPosition, useLocalRotation);
    }

    private static GameObject SpawnNonPooledObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null, bool useLocalPosition = false, bool useLocalRotation = false)
    {
        // Debug.LogWarning("You are spawning a non-pooled prefab \"" + prefab.name + "\"."
        //     + " The object will be instantiated normally."
        //     + " Check your scene setup.\n"
        //     + "- From Giang with love.");

        GameObject obj = Instantiate(prefab);

        obj.transform.SetParent(parent);

        if (useLocalPosition)
            obj.transform.localPosition = position;
        else
            obj.transform.position = position;

        if (useLocalRotation)
            obj.transform.localRotation = rotation;
        else
            obj.transform.rotation = rotation;
        obj.transform.localScale = scale;

        return obj;
    }

    public static void Kill(GameObject obj, bool surpassWarning = false)
    {
        foreach (KeyValuePair<GameObject, Pool> pool in currentPools)
        {
            if (pool.Value.IsResponsibleForObject(obj))
            {
                pool.Value.Kill(obj);
                return;
            }
        }

        if (!surpassWarning)
        {
            // Debug.LogWarning("You are killing an object with non-pooled prefab."
            //     + " The object will be destroyed normally."
            //     + " Check your scene setup.\n"
            //     + "- From Giang with love.");
        }
        Destroy(obj);
    }
}
