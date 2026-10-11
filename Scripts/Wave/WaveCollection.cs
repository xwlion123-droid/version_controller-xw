using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 波次集合 - 包含一个关卡的所有波次
/// </summary>
[CreateAssetMenu(fileName = "WaveCollection_", menuName = "Game/Wave Collection")]
public class WaveCollection : ScriptableObject
{
    [Header("波次列表")]
    [SerializeField] private List<WaveData> waves = new List<WaveData>();

    [Header("难度曲线")]
    [Tooltip("每波自动增加的难度百分比（0.1 = 10%）")]
    [Range(0f, 0.5f)] public float difficultyGrowthPerWave = 0.15f;

    [Tooltip("是否自动应用难度曲线")]
    public bool applyDifficultyCurve = true;

    public int TotalWaves => waves.Count;

    public WaveData GetWave(int index)
    {
        if (index < 0 || index >= waves.Count) return null;
        return waves[index];
    }

    /// <summary>
    /// 获取应用难度曲线后的波次
    /// </summary>
    public WaveData GetWaveWithDifficulty(int index)
    {
        var baseWave = GetWave(index);
        if (baseWave == null) return null;

        if (!applyDifficultyCurve)
            return baseWave;

        // 创建运行时副本，不影响原始 SO
        WaveData runtimeWave = Instantiate(baseWave);

        // 应用难度曲线
        float multiplier = 1f + difficultyGrowthPerWave * index;
        runtimeWave.healthMultiplier = multiplier;
        runtimeWave.speedMultiplier = 1f + difficultyGrowthPerWave * index * 0.5f;
        runtimeWave.scoreMultiplier = multiplier;

        return runtimeWave;
    }
}