using System;
using UnityEngine;

/// <summary>
/// 存档数据 - 所有需要持久化的数据放这里
/// ⚠️ 必须是 [Serializable]，且字段必须是 public 或 [SerializeField]
/// </summary>
[Serializable]
public class SaveData
{
    [Header("游戏进度")]
    public int highScore = 0;          // 最高分
    public int totalKills = 0;         // 总击杀数
    public bool hasCompletedGame = false; // 是否通关

    [Header("游戏设置")]
    public float masterVolume = 1f;   // 主音量
    public float musicVolume = 1f;    // 音乐音量
    public float sfxVolume = 1f;      // 音效音量

    [Header("其他统计")]
    public int totalPlayCount = 0;     // 总游玩次数
    public long lastPlayTime = 0;      // 最后游玩时间（时间戳）
    public string lastPlayDate = "";   // 最后游玩日期

    /// <summary>
    /// 重置为默认值
    /// </summary>
    public void Reset()
    {
        highScore = 0;
        totalKills = 0;
        hasCompletedGame = false;
        masterVolume = 1f;
        musicVolume = 1f;
        sfxVolume = 1f;
        totalPlayCount = 0;
        lastPlayTime = 0;
        lastPlayDate = "";
    }
}