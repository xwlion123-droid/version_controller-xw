using UnityEngine;

/// <summary>
/// 游戏流程管理器 - 工业级
/// 负责：主菜单 / 游戏 / 暂停 / 结束 / 胜利 的状态切换
/// 兼容现有 GameManager / WaveManager / UIManager
/// </summary>
public class GameFlowManager : SingletonBase<GameFlowManager>, IPersistent
{
    [Header("流程事件（广播）")]
    [SerializeField] private VoidEventChannel onGameStart;
    [SerializeField] private VoidEventChannel onGamePause;
    [SerializeField] private VoidEventChannel onGameResume;
    [SerializeField] private VoidEventChannel onGameRestart;

    [Header("流程事件（订阅）")]
    [SerializeField] private VoidEventChannel onGameOver;
    [SerializeField] private VoidEventChannel onAllWavesComplete;

    [Header("调试")]
    [SerializeField] private bool debugMode = true;

    // ===== 运行时状态 =====
    private GameState _currentState = GameState.MainMenu;
    private GameState _previousState = GameState.MainMenu;

    // ===== 公共属性 =====
    public GameState CurrentState => _currentState;
    public GameState PreviousState => _previousState;
    public bool IsPlaying => _currentState == GameState.Playing;
    public bool IsPaused => _currentState == GameState.Paused;
    public bool IsGameEnded => _currentState == GameState.GameOver || _currentState == GameState.Victory;

    // ===== Unity 生命周期 =====
    void OnEnable()
    {
        onGameOver?.Subscribe(OnGameOverHandler);
        onAllWavesComplete?.Subscribe(OnVictoryHandler);
    }

    void OnDisable()
    {
        onGameOver?.Unsubscribe(OnGameOverHandler);
        onAllWavesComplete?.Unsubscribe(OnVictoryHandler);
    }

    void Start()
    {
        // 默认进入主菜单（游戏暂停）
        if (_currentState == GameState.MainMenu)
        {
            Time.timeScale = 0f;
            Debug.Log("🏠 游戏启动，等待玩家点击开始");
        }
    }

    void Update()
    {
        // Esc 暂停/恢复
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == GameState.Playing)
                PauseGame();
            else if (_currentState == GameState.Paused)
                ResumeGame();
        }
    }

    // ===== 状态切换 =====
    void ChangeState(GameState newState)
    {
        _previousState = _currentState;
        _currentState = newState;

        if (debugMode)
            Debug.Log($"<color=yellow>🎮 游戏状态：{_previousState} → {_currentState}</color>");

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 0f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 1f;
                break;
            case GameState.Victory:
                Time.timeScale = 1f;
                break;
        }
    }

    // ===== 公共方法 =====

    /// <summary>
    /// 开始游戏（主菜单点击"开始"调用）
    /// </summary>
    public void StartGame()
    {
        Debug.Log("🎬 开始游戏");

        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
        onGameStart?.RaiseEvent();

        // ✅ 兼容：波次系统自动启动
        //WaveManager.Instance?.StartWaves();
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (_currentState != GameState.Playing) return;
        Debug.Log("⏸️ 暂停游戏");
        ChangeState(GameState.Paused);
        onGamePause?.RaiseEvent();
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        if (_currentState != GameState.Paused) return;
        Debug.Log("▶️ 恢复游戏");
        ChangeState(GameState.Playing);
        onGameResume?.RaiseEvent();
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("🔄 重启游戏");
        Time.timeScale = 1f;
        onGameRestart?.RaiseEvent();

        // ✅ 兼容：重置游戏数据
        GameManager.Instance?.ResetGame();

        // ✅ 兼容：清理所有敌人
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            EnemyPoolManager.Instance?.ReleaseEnemy(enemy.gameObject);
        }

        // ✅ 兼容：重置玩家位置
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.transform.position = Vector3.zero;

        // ✅ 兼容：重启波次
        WaveManager.Instance?.StartWaves();

        ChangeState(GameState.Playing);
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("🏠 返回主菜单");

        // ✅ 兼容：清理敌人
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            EnemyPoolManager.Instance?.ReleaseEnemy(enemy.gameObject);
        }

        // ✅ 重置游戏状态
        GameManager.Instance?.ResetGame();

        ChangeState(GameState.MainMenu);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("🚪 退出游戏");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ===== 事件回调 =====
    void OnGameOverHandler()
    {
        if (_currentState == GameState.Playing)
        {
            Debug.Log("💀 游戏失败");
            ChangeState(GameState.GameOver);
            // ✅ 直接通知 UIManager（因为 GameManager 已经广播了 onGameOver，UIManager 已订阅）
        }
    }

    void OnVictoryHandler()
    {
        if (_currentState == GameState.Playing)
        {
            Debug.Log("🏆 游戏胜利");
            ChangeState(GameState.Victory);
        }
    }
}