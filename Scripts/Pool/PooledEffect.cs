using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 挂在特效对象上，自动检测粒子播放完毕并回池
/// </summary>
public class PooledEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1.5f;  // 最大存活时间
    [SerializeField] private float checkDelay = 0.3f; // 播放后多久开始检测

    private ParticleSystem[] _particleSystems;
    private ObjectPool<GameObject> _pool;
    private float _spawnTime;

    void Awake()
    {
        // 获取所有粒子系统（包括子物体）
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    void OnEnable()
    {
        _spawnTime = Time.time;

        // 播放所有粒子系统
        foreach (var ps in _particleSystems)
        {
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
            }
        }
    }

    void Update()
    {
        // 等 checkDelay 后再检测是否播放完毕
        if (Time.time - _spawnTime < checkDelay)
            return;

        // 检测所有粒子是否播放完毕
        bool allStopped = true;
        foreach (var ps in _particleSystems)
        {
            if (ps != null && ps.isPlaying)
            {
                allStopped = false;
                break;
            }
        }

        // 全部停止或超时 → 回池
        if (allStopped || Time.time - _spawnTime > lifeTime)
        {
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