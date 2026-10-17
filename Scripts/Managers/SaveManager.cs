using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档管理器 - 工业级
/// 负责：读取、保存、删除存档，支持自动存档
/// </summary>
public class SaveManager : SingletonBase<SaveManager>, IPersistent
{
    [Header("配置")]
    [Tooltip("存档文件名（不含扩展名）")]
    [SerializeField] private string saveFileName = "savegame";

    [Tooltip("是否启用自动存档")]
    [SerializeField] private bool enableAutoSave = true;

    [Tooltip("自动存档间隔（秒）")]
    [SerializeField] private float autoSaveInterval = 30f;

    [Header("调试")]
    [SerializeField] private bool debugMode = true;

    // ===== 运行时 =====
    private SaveData _currentData;
    private float _autoSaveTimer;

    /// <summary>
    /// 存档文件完整路径
    /// </summary>
    private string SavePath => Path.Combine(Application.persistentDataPath, $"{saveFileName}.json");

    /// <summary>
    /// 当前存档数据（只读）
    /// </summary>
    public SaveData Data => _currentData;

    // ===== Unity 生命周期 =====
    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    void Update()
    {
        // 自动存档计时
        if (enableAutoSave && _currentData != null)
        {
            _autoSaveTimer += Time.unscaledDeltaTime;

            if (_autoSaveTimer >= autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                Save();
            }
        }
    }

    void OnApplicationQuit()
    {
        // 游戏退出时自动存档
        Save();
    }

    // ===== 核心方法 =====

    /// <summary>
    /// 加载存档
    /// </summary>
    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                _currentData = JsonUtility.FromJson<SaveData>(json);

                if (_currentData == null)
                {
                    Debug.LogWarning("⚠️ 存档解析失败，使用默认数据");
                    _currentData = new SaveData();
                }

                if (debugMode)
                    Debug.Log($"<color=green>📂 存档加载成功</color> | 最高分：{_currentData.highScore} | 总击杀：{_currentData.totalKills}");
            }
            else
            {
                _currentData = new SaveData();
                if (debugMode)
                    Debug.Log("<color=yellow>📂 无存档，创建新档</color>");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 存档加载失败：{e.Message}");
            _currentData = new SaveData();
        }

        // 更新最后游玩时间
        _currentData.lastPlayTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _currentData.lastPlayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _currentData.totalPlayCount++;
    }

    /// <summary>
    /// 保存存档
    /// </summary>
    public void Save()
    {
        if (_currentData == null)
        {
            Debug.LogWarning("⚠️ 无存档数据，跳过保存");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(_currentData, true);

            // ✅ 关键：使用临时文件 + 替换策略，防止保存时崩溃导致存档损坏
            string tempPath = SavePath + ".tmp";
            File.WriteAllText(tempPath, json);

            if (File.Exists(SavePath))
                File.Delete(SavePath);

            File.Move(tempPath, SavePath);

            if (debugMode)
                Debug.Log($"<color=cyan>💾 存档保存成功</color> | 路径：{SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 存档保存失败：{e.Message}");
        }
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public void Delete()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("<color=orange>🗑️ 存档已删除</color>");
            }
            _currentData = new SaveData();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 存档删除失败：{e.Message}");
        }
    }

    // ===== 便捷方法 =====

    /// <summary>
    /// 更新最高分
    /// </summary>
    public void UpdateHighScore(int newScore)
    {
        if (_currentData == null) return;

        if (newScore > _currentData.highScore)
        {
            _currentData.highScore = newScore;
            Save();

            if (debugMode)
                Debug.Log($"<color=yellow>🏆 新纪录！最高分：{newScore}</color>");
        }
    }

    /// <summary>
    /// 增加击杀数
    /// </summary>
    public void AddKill()
    {
        if (_currentData == null) return;
        _currentData.totalKills++;
    }

    /// <summary>
    /// 标记为通关
    /// </summary>
    public void MarkCompleted()
    {
        if (_currentData == null) return;
        _currentData.hasCompletedGame = true;
        Save();
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    public void SetVolume(float master, float music, float sfx)
    {
        if (_currentData == null) return;
        _currentData.masterVolume = master;
        _currentData.musicVolume = music;
        _currentData.sfxVolume = sfx;
        Save();
    }

    /// <summary>
    /// 调试：打印存档路径
    /// </summary>
    [ContextMenu("打印存档路径")]
    public void PrintSavePath()
    {
        Debug.Log($"存档路径：{SavePath}");
    }
}