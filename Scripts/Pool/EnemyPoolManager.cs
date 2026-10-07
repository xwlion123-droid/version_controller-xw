using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class EnemyPoolManager : SingletonBase<EnemyPoolManager>, IPersistent
{
    [Header("敌人配置")]
    [SerializeField] private int defaultCapacity = 15;
    [SerializeField] private int maxSize = 60;
    [SerializeField] private float enemySize = 1f;

    private ObjectPool<GameObject> _pool;

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    void InitializePool()
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        Prewarm();
    }

    GameObject CreateEnemy()
    {
        // ✅ 用几何体创建敌人
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "Enemy";
        enemy.transform.localScale = Vector3.one * enemySize;
        enemy.GetComponent<MeshRenderer>().material.color = Color.red;

        // 添加 Enemy 脚本
        Enemy enemyScript = enemy.AddComponent<Enemy>();
        enemyScript.SetPool(_pool);

        //改用EnemyAI
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        // ai.SetPool(_pool);//如果你需要池引用


        // ✅ 确保 Rigidbody 存在（OnCollisionEnter 需要）
        if (enemy.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        enemy.SetActive(false);
        return enemy;
    }

    void OnGetEnemy(GameObject enemy)
    {
        enemy.SetActive(true);
        //重置AI状态
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.ResetState();
        }
    }

    void OnReleaseEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(transform);
        enemy.transform.localPosition = Vector3.zero;
        enemy.transform.localRotation = Quaternion.identity;
        enemy.transform.localScale = Vector3.one * enemySize;
    }

    void OnDestroyEnemy(GameObject enemy)
    {
        Destroy(enemy);
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
        Debug.Log($"🔥 敌人池预热完成，容量：{defaultCapacity}");
    }

    // ===== 对外接口 =====
    public GameObject GetEnemy(Vector3 position, Quaternion rotation, EnemyData data)
    {
        GameObject enemy = _pool.Get();
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;

        // 传入敌人数据
        // Enemy enemyScript = enemy.GetComponent<Enemy>();
        // if (enemyScript != null && data != null)
        // {
        //     enemyScript.Initialize(data);
        // }

        //初始化EnemyAI
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null && data != null)
        {
            ai.Initialize(data);
        }

        return enemy;
    }

    public void ReleaseEnemy(GameObject enemy)
    {
        _pool.Release(enemy);
    }
}