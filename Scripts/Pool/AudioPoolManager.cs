using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class AudioPoolManager : SingletonBase<AudioPoolManager>, IPersistent
{
    [Header("音源池配置")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 30;
    [SerializeField] private float sfxVolume = 0.5f;

    // AudioSource 池
    private ObjectPool<AudioSource> _pool;

    // 预生成的音效
    private Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

    protected override void Awake()
    {
        base.Awake();
        InitializeClips();
        InitializePool();
    }

    void InitializeClips()
    {
        // ✅ 程序化生成音效
        _clips["Shoot"] = ProceduralAudio.CreateShootSound();
        _clips["Hit"] = ProceduralAudio.CreateHitSound();
        _clips["Explosion"] = ProceduralAudio.CreateExplosionSound();
        _clips["Score"] = ProceduralAudio.CreateScoreSound();
        _clips["Damage"] = ProceduralAudio.CreateDamageSound();

        Debug.Log($"🔊 程序化音效生成完成，共 {_clips.Count} 种");
    }

    void InitializePool()
    {
        _pool = new ObjectPool<AudioSource>(
            createFunc: CreateAudioSource,
            actionOnGet: (source) => source.gameObject.SetActive(true),
            actionOnRelease: (source) => source.gameObject.SetActive(false),
            actionOnDestroy: (source) => Destroy(source.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // 预热
        List<AudioSource> temp = new List<AudioSource>();
        for (int i = 0; i < defaultCapacity; i++)
        {
            temp.Add(_pool.Get());
        }
        foreach (var source in temp)
        {
            _pool.Release(source);
        }

        Debug.Log($"🔥 音源池预热完成，容量：{defaultCapacity}");
    }

    AudioSource CreateAudioSource()
    {
        GameObject go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = sfxVolume;
        go.SetActive(false);
        return source;
    }

    // ===== 对外接口 =====

    /// <summary>
    /// 播放音效（一次性）
    /// </summary>
    public void PlaySFX(string soundKey, float volume = 1f, float pitch = 1f)
    {
        if (!_clips.ContainsKey(soundKey))
        {
            Debug.LogWarning($"⚠️ 音效 '{soundKey}' 不存在");
            return;
        }

        AudioSource source = _pool.Get();
        source.clip = _clips[soundKey];
        source.volume = sfxVolume * volume;
        source.pitch = pitch;
        source.Play();

        // 启动协程：播放完毕后回池
        StartCoroutine(ReturnAfterPlay(source));
    }

    System.Collections.IEnumerator ReturnAfterPlay(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        source.clip = null;
        _pool.Release(source);
    }

    // ===== 便捷方法 =====

    public void PlayShoot() => PlaySFX("Shoot", 0.5f);
    public void PlayHit() => PlaySFX("Hit", 0.6f);
    public void PlayExplosion() => PlaySFX("Explosion", 0.8f);
    public void PlayScore() => PlaySFX("Score", 0.4f);
    public void PlayDamage() => PlaySFX("Damage", 0.7f);
}