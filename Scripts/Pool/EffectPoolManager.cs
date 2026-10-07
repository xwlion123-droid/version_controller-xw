using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class EffectPoolManager : SingletonBase<EffectPoolManager>, IPersistent
{
    [Header("特效配置")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 50;

    // 存储不同特效的池（按 key 区分）
    private Dictionary<string, ObjectPool<GameObject>> _pools = new Dictionary<string, ObjectPool<GameObject>>();

    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    void InitializePools()
    {
        // ✅ 预创建三种特效池
        CreatePool("Hit", CreateHitEffect);
        CreatePool("Explosion", CreateExplosionEffect);
        CreatePool("MuzzleFlash", CreateMuzzleFlashEffect);

        Debug.Log($"🔥 特效池初始化完成，共 {_pools.Count} 种");
    }

    void CreatePool(string key, System.Func<GameObject> createFunc)
    {
        var pool = new ObjectPool<GameObject>(
            createFunc: createFunc,
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        _pools[key] = pool;

        // 预热
        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < defaultCapacity; i++)
        {
            temp.Add(pool.Get());
        }
        foreach (var obj in temp)
        {
            pool.Release(obj);
        }
    }

    // ========== 特效创建（程序化） ==========

    /// <summary>
    /// 命中特效：小范围黄色粒子爆发
    /// </summary>
    GameObject CreateHitEffect()
    {
        GameObject effect = new GameObject("HitEffect");
        ParticleSystem ps = effect.AddComponent<ParticleSystem>();

        // 主模块
        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.15f;
        main.startColor = Color.yellow;
        main.maxParticles = 20;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.None;

        // 发射模块
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 15)
        });

        // 形状模块
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // 颜色渐变
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(Color.red, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // 添加回池脚本
        PooledEffect pooled = effect.AddComponent<PooledEffect>();
        pooled.SetPool(_pools["Hit"]);

        effect.SetActive(false);
        return effect;
    }

    /// <summary>
    /// 爆炸特效：大范围橙红色粒子爆发
    /// </summary>
    GameObject CreateExplosionEffect()
    {
        GameObject effect = new GameObject("ExplosionEffect");
        ParticleSystem ps = effect.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.5f, 0f);  // 橙色
        main.maxParticles = 50;
        main.playOnAwake = false;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 30)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.8f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0f), 0.5f),
                new GradientColorKey(Color.black, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        PooledEffect pooled = effect.AddComponent<PooledEffect>();
        pooled.SetPool(_pools["Explosion"]);

        effect.SetActive(false);
        return effect;
    }

    /// <summary>
    /// 枪口闪光：小范围青色粒子
    /// </summary>
    GameObject CreateMuzzleFlashEffect()
    {
        GameObject effect = new GameObject("MuzzleFlash");
        ParticleSystem ps = effect.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false;
        main.startLifetime = 0.1f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.startColor = Color.cyan;
        main.maxParticles = 8;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 5)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.05f;

        PooledEffect pooled = effect.AddComponent<PooledEffect>();
        pooled.SetPool(_pools["MuzzleFlash"]);

        effect.SetActive(false);
        return effect;
    }

    // ========== 对外接口 ==========

    /// <summary>
    /// 在指定位置播放特效
    /// </summary>
    public GameObject PlayEffect(string effectKey, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(effectKey))
        {
            Debug.LogWarning($"⚠️ 特效 '{effectKey}' 不存在");
            return null;
        }

        GameObject effect = _pools[effectKey].Get();
        effect.transform.position = position;
        effect.transform.rotation = rotation;
        return effect;
    }

    /// <summary>
    /// 便捷方法：在位置播放命中特效
    /// </summary>
    public void PlayHitEffect(Vector3 position)
    {
        PlayEffect("Hit", position, Quaternion.identity);
    }

    /// <summary>
    /// 便捷方法：在位置播放爆炸特效
    /// </summary>
    public void PlayExplosionEffect(Vector3 position)
    {
        PlayEffect("Explosion", position, Quaternion.identity);
    }

    /// <summary>
    /// 便捷方法：在位置播放枪口闪光
    /// </summary>
    public void PlayMuzzleFlash(Vector3 position, Quaternion rotation)
    {
        PlayEffect("MuzzleFlash", position, rotation);
    }
}