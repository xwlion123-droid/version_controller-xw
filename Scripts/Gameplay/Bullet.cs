using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;

    private float spawnTime;
    private Rigidbody rb;
    private ObjectPool<GameObject> _pool;

    void Awake()
    {
        //添加Rigidbody并配置
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        //关键配置：让子弹能触发碰撞检测
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        //设置Tag
        gameObject.tag = "Bullet";
    }

    void OnEnable()
    {
        spawnTime = Time.time;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        // 使用 Rigidbody 移动（物理驱动，碰撞更准确）
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        //超时销毁
        if (Time.time - spawnTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //造成伤害
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            // ✅ 播放命中音效
            AudioPoolManager.Instance?.PlayHit();

            // ✅ 播放命中特效
            // EffectPoolManager.Instance?.PlayHitEffect(transform.position);
            //销毁子弹（实际应该收回池）
            ReturnToPool();//回池而非销毁
        }
        else if (!collision.gameObject.CompareTag("Player"))
        {
            //回池
            // ✅ 碰到墙壁也播放小特效（可选）
            // EffectPoolManager.Instance?.PlayHitEffect(transform.position);
            ReturnToPool();
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