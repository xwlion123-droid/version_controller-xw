using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    [Header("数据")]
    [SerializeField] private EnemyData enemyData;

    private float currentHealth;
    private MeshRenderer renderer;
    private ObjectPool<GameObject> _pool;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = GetComponentInChildren<MeshRenderer>();
        gameObject.tag = "Enemy";

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = false;
    }

    // ✅ 新增：从池获取时调用，初始化数据
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHealth = enemyData?.Health ?? 1f;

        if (renderer != null && enemyData != null)
        {
            renderer.material.color = enemyData.Color;
        }

        float size = enemyData?.Size ?? 1f;
        transform.localScale = Vector3.one * size;
    }

    void OnEnable()
    {
        //初始化
        currentHealth = enemyData?.Health ?? 1f;
        if (renderer != null && enemyData != null)
        {
            renderer.material.color = enemyData.Color;
        }
        //设置大小
        float size = enemyData?.Size ?? 1f;
        transform.localScale = Vector3.one * size;
    }

    void Update()
    {
        //向玩家移动
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            Transform player = FindFirstObjectByType<PlayerController>()?.transform;
            if (player != null)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                float speed = enemyData?.Speed ?? 3f;
                transform.position += direction * speed * Time.deltaTime;
                //面向玩家
                transform.LookAt(player);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name}受到{damage}伤害，剩余血量：{currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        int score = enemyData?.ScoreValue ?? 10;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(score);
            Debug.Log($"✅ 击杀敌人！获得 {score} 分");
        }
        // ✅ 播放爆炸特效
        // EffectPoolManager.Instance?.PlayExplosionEffect(transform.position);

        // ✅ 播放爆炸音效
        AudioPoolManager.Instance?.PlayExplosion();

        Debug.Log($"{name}死亡");
        // ✅ 回池而非销毁
        ReturnToPool();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //玩家会受到伤害（由PlayerController处理）
            Die();
        }
    }

    public void SetPool(ObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}