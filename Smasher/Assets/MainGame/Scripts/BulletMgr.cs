using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defines;

public class BulletMgr : MonoBehaviour
{
    [SerializeField] private GameObject BulletPrefab;
    List<GameObject> mBullets = new List<GameObject>();

    void Start()
    {

    }

    GameObject GetInactiveBullet()
    {
        List<GameObject> bullets = mBullets.FindAll(bullet => !bullet.activeSelf);
        if (bullets.Count > 0)
        {
            return bullets[0];
        }

        return null;
    }

    public void TargetShot(Transform owner, Transform target, BulletType type)
    {
        GameObject bullet = GetInactiveBullet();
        if (bullet == null)
        {
            bullet = Instantiate(BulletPrefab, transform);
            mBullets.Add(bullet);
        }

        bullet.tag = owner.tag;
        bullet.transform.position = owner.position;
        // bullet.transform.rotation = owner.rotation;
        bullet.transform.LookAt(target);
        bullet.GetComponent<Bullet>().Fire(target.position);
        bullet.GetComponent<Bullet>().SetType(type);
    }
}
