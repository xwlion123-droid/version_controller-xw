using UnityEngine;

/// <summary>
/// 程序化音效生成工具
/// 不需要任何音频文件，用代码生成音效
/// </summary>
public static class ProceduralAudio
{
    private const int SampleRate = 44100;

    /// <summary>
    /// 生成射击音效（短促的高频"哔"声）
    /// </summary>
    public static AudioClip CreateShootSound()
    {
        float duration = 0.1f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float progress = (float)i / sampleCount;

            // 频率从 1200Hz 降到 600Hz（短促的"哔"声）
            float frequency = Mathf.Lerp(1200f, 600f, progress);
            float value = Mathf.Sin(2f * Mathf.PI * frequency * t);

            // 音量衰减（指数衰减）
            float envelope = Mathf.Exp(-progress * 8f);

            samples[i] = value * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("ShootSound", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 生成命中音效（短促的"咔"声）
    /// </summary>
    public static AudioClip CreateHitSound()
    {
        float duration = 0.08f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = (float)i / sampleCount;

            // 白噪声 + 快速衰减
            float noise = Random.Range(-1f, 1f);
            float envelope = Mathf.Exp(-progress * 15f);

            samples[i] = noise * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("HitSound", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 生成爆炸音效（低频噪声 + 长衰减）
    /// </summary>
    public static AudioClip CreateExplosionSound()
    {
        float duration = 0.5f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float progress = (float)i / sampleCount;

            // 低频噪声
            float noise = Random.Range(-1f, 1f);
            float rumble = Mathf.Sin(2f * Mathf.PI * 80f * t);  // 80Hz 低频

            // 混合 + 慢衰减
            float envelope = Mathf.Exp(-progress * 4f);
            samples[i] = (noise * 0.6f + rumble * 0.4f) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("ExplosionSound", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 生成得分音效（上升的两个音）
    /// </summary>
    public static AudioClip CreateScoreSound()
    {
        float duration = 0.2f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float progress = (float)i / sampleCount;

            // 前一半 600Hz，后一半 900Hz
            float frequency = progress < 0.5f ? 600f : 900f;
            float value = Mathf.Sin(2f * Mathf.PI * frequency * t);

            // 平滑的包络（避免爆音）
            float envelope = Mathf.Sin(progress * Mathf.PI);

            samples[i] = value * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("ScoreSound", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 生成受伤音效（下降的音）
    /// </summary>
    public static AudioClip CreateDamageSound()
    {
        float duration = 0.3f;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float progress = (float)i / sampleCount;

            // 频率从 400Hz 降到 150Hz
            float frequency = Mathf.Lerp(400f, 150f, progress);
            float value = Mathf.Sin(2f * Mathf.PI * frequency * t);

            // 包络
            float envelope = Mathf.Exp(-progress * 3f);

            samples[i] = value * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("DamageSound", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}