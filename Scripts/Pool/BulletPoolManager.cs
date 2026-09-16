using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class BulletPoolManager : SingletonBase<BulletPoolManager>, IPersistent
{
    [Header("子弹配置")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;
    [SerializeField] private float bulletSize = 0.15f;

    private ObjectPool<GameObject> _pool;

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    void InitializePool()
    {
        _pool = new ObjectPool<GameObject>(
             createFunc: CreateBullet,
             actionOnGet: OnGetBullet,
             actionOnRelease: OnReleaseBullet,
             actionOnDestroy: OnDestroyBullet,
             collectionCheck: true,
             defaultCapacity: defaultCapacity,
             maxSize: maxSize
         );

        // 预热
        Prewarm();
    }

    GameObject CreateBullet()
    {
        //用几何体创建子弹（零素材）
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";
        bullet.transform.localScale = Vector3.one * bulletSize;
        bullet.GetComponent<MeshRenderer>().material.color = Color.cyan;

        //添加Bullet脚本
        Bullet bulletScript = bullet.AddComponent<Bullet>();
        bulletScript.SetPool(_pool);

        bullet.SetActive(false);
        return bullet;
    }

    void OnGetBullet(GameObject bullet)
    {
        bullet.SetActive(true);
    }

    void OnReleaseBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        bullet.transform.localPosition = Vector3.zero;
        bullet.transform.localRotation = Quaternion.identity;

        //重置刚体速度
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnDestroyBullet(GameObject bullet)
    {
        Destroy(bullet);
    }

    void Prewarm()
    {
        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < defaultCapacity; i++)
        {
            temp.Add(_pool.Get());
        }
        foreach (var obj in temp)
        {
            _pool.Release(obj);
        }
        Debug.Log($"子弹预热完成，容量：{defaultCapacity}");
    }

    //====对外接口====
    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = _pool.Get();
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        return bullet;
    }

    public void ReleaseBullet(GameObject bullet)
    {
        _pool.Release(bullet);
    }

}