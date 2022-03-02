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

public static class GameObjectExtensions
{
    public static GameObject Spawn(this GameObject prefab, Vector3 worldPosition)
    {
        return PoolManager.Spawn(prefab,
            worldPosition, Quaternion.identity, prefab.transform.localScale,
            null);
    }

    public static GameObject Spawn(this GameObject prefab)
    {
        return PoolManager.Spawn(prefab, 
            Vector3.zero, Quaternion.identity, prefab.transform.localScale,
            null);
    }

    public static GameObject Spawn(this GameObject prefab, Transform parent)
    {
        return PoolManager.Spawn(prefab,
            Vector3.zero, Quaternion.identity, prefab.transform.localScale,
            parent, true, true);
    }

    public static GameObject Spawn(this GameObject prefab,
        Vector3 position, Quaternion rotation, Vector3 scale,
        Transform parent = null,
        bool useLocalPosition = false, bool useLocalRotation = false)
    {
        return PoolManager.Spawn(prefab, position, rotation, scale,
            parent, useLocalPosition, useLocalRotation);
    }

    public static void Kill(this GameObject obj, bool surpassWarning = false)
    {
        PoolManager.Kill(obj, surpassWarning);
    }
}
