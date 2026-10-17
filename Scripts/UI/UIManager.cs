using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>, IPersistent
{
    [Header("流程事件（订阅）")]
    [SerializeField] private VoidEventChannel onGamePause;
    [SerializeField] private VoidEventChannel onGameResume;

    [Header("事件通道（订阅）")]
    [SerializeField] private IntEventChannel onScoreChanged;
    [SerializeField] private IntEventChannel onHealthChanged;
    [SerializeField] private VoidEventChannel onGameOver;
    [SerializeField] private IntEventChannel onWaveStart;
    [SerializeField] private IntEventChannel onWaveComplete;
    [SerializeField] private IntEventChannel onWavePrepare;
    [SerializeField] private VoidEventChannel onAllWavesComplete;

    [Header("UI 引用")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    [Header("暂停 UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button menuButton;

    [Header("胜利 UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryMenuButton;

    [Header("存档 UI")]
    [SerializeField] private TextMeshProUGUI highScoreText;  // ✅ 新增：显示最高分
    [SerializeField] private TextMeshProUGUI totalKillsText; // ✅ 新增：显示总击杀数
    // ===== 生命周期 =====
    void OnEnable()
    {
        // 订阅事件
        onGamePause?.Subscribe(OnGamePause);
        onGameResume?.Subscribe(OnGameResume);
        onScoreChanged?.Subscribe(OnScoreChanged);
        onHealthChanged?.Subscribe(OnHealthChanged);
        onGameOver?.Subscribe(OnGameOver);
        onWaveStart?.Subscribe(OnWaveStart);
        onWaveComplete?.Subscribe(OnWaveComplete);
        onWavePrepare?.Subscribe(OnWavePrepare);
        onAllWavesComplete?.Subscribe(OnAllWavesComplete);
        // ✅ 更新最高分显示
        UpdateHighScoreDisplay();
        //暂停按钮
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(OnPauseRestartClicked);
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        // 胜利按钮
        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(OnVictoryRestartClicked);
        if (victoryMenuButton != null)
            victoryMenuButton.onClick.AddListener(OnMenuClicked);

        // 重启按钮
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        // 初始更新
        RefreshUI();

    }

    /// <summary>
    /// 更新最高分显示
    /// </summary>
    public void UpdateHighScoreDisplay()
    {
        if (SaveManager.Instance == null) return;

        if (highScoreText != null)
        {
            int highScore = SaveManager.Instance.Data.highScore;
            highScoreText.text = $"Highest Score: {highScore}";
        }

        if (totalKillsText != null)
        {
            int totalKills = SaveManager.Instance.Data.totalKills;
            totalKillsText.text = $"Total Kill: {totalKills}";
        }
    }
    void OnDisable()
    {
        // 取消订阅
        onGamePause?.Unsubscribe(OnGamePause);
        onGameResume?.Unsubscribe(OnGameResume);
        onScoreChanged?.Unsubscribe(OnScoreChanged);
        onHealthChanged?.Unsubscribe(OnHealthChanged);
        onGameOver?.Unsubscribe(OnGameOver);
        onWaveStart?.Unsubscribe(OnWaveStart);
        onWaveComplete?.Unsubscribe(OnWaveComplete);
        onWavePrepare?.Unsubscribe(OnWavePrepare);
        onAllWavesComplete?.Unsubscribe(OnAllWavesComplete);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.RemoveListener(OnPauseRestartClicked);
        if (menuButton != null)
            menuButton.onClick.RemoveListener(OnMenuClicked);
        if (victoryRestartButton != null)
            victoryRestartButton.onClick.RemoveListener(OnVictoryRestartClicked);
        if (victoryMenuButton != null)
            victoryMenuButton.onClick.RemoveListener(OnMenuClicked);
    }

    // ===== 初始 UI =====
    void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        if (scoreText != null)
            scoreText.text = $"Score:{GameManager.Instance?.Score}";

        if (healthText != null)
            healthText.text = BuildHealthText(GameManager.Instance.Health);

        if (waveText != null)
        {
            if (WaveManager.Instance != null)
                waveText.text = $"The {WaveManager.Instance.CurrentWaveNumber} Wave";
            else
                waveText.text = "Parparing...";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        // ✅ 确保所有面板初始隐藏（除了主菜单）
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        // ⚠️ MainMenuPanel 不要隐藏，因为 MainMenuUI.Start() 会显示它
    }

    // ===== 事件回调：分数 =====
    void OnScoreChanged(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
        Debug.Log($"📱 UI 更新：分数 {score}");
    }

    // ===== 事件回调：生命 =====
    void OnHealthChanged(int health)
    {
        if (healthText != null)
            healthText.text = BuildHealthText(health);
        Debug.Log($"📱 UI 更新：生命 {health}");
    }

    // ===== 事件回调：游戏结束 =====
    void OnGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = $"Final Score: {GameManager.Instance.Score}";

        // ✅ 更新最高分显示
        UpdateHighScoreDisplay();

        Debug.Log($"📱 UI 显示：游戏结束");
    }

    // ===== 事件回调：波次开始 =====
    void OnWaveStart(int waveNumber)
    {
        if (waveText != null)
            waveText.text = $"The {waveNumber} Wave";

        Debug.Log($"📱 UI 更新：波次 {waveNumber} 开始");
    }

    // ===== 事件回调：波次准备中 =====
    void OnWavePrepare(int secondsLeft)
    {
        if (waveText != null)
        {
            int waveNumber = WaveManager.Instance != null
                ? WaveManager.Instance.CurrentWaveNumber
                : 1;

            waveText.text = $"The {waveNumber} Wave Parparing... {secondsLeft}";
        }
    }

    // ===== 事件回调：波次完成 =====
    void OnWaveComplete(int waveNumber)
    {
        Debug.Log($"📱 UI 更新：波次 {waveNumber} 完成");

        if (waveText != null)
            waveText.text = $"The {waveNumber} Wave Finish!";
    }

    // ===== 事件回调：所有波次完成 =====
    void OnAllWavesComplete()
    {
        Debug.Log($"📱 UI 更新：所有波次完成！");

        if (waveText != null)
            waveText.text = " Pass The Task!";

        //新增：显示胜利面板
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryScoreText != null && GameManager.Instance != null)
                victoryScoreText.text = $"Final Score:{GameManager.Instance.Score}";
        }
    }
    void OnGamePause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    void OnGameResume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    // ===== 按钮回调 =====
    void OnRestartButtonClicked()
    {
        Debug.Log("🔄 重新开始游戏");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        //老的
        //GameManager.Instance?.ResetGame();
        //新的
        GameFlowManager.Instance?.RestartGame();

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.transform.position = Vector3.zero;
        }

        // 清理旧敌人
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            EnemyPoolManager.Instance?.ReleaseEnemy(enemy.gameObject);
        }

        // ✅ 重启波次系统
        WaveManager.Instance?.StartWaves();
    }
    //暂停按钮回调
    void OnResumeClicked()
    {
        Debug.Log("继续游戏");
        if (pausePanel != null) pausePanel.SetActive(false);
        GameFlowManager.Instance?.ResumeGame();
    }
    void OnPauseRestartClicked()
    {
        Debug.Log("暂停中启动");
        if (pausePanel != null) pausePanel.SetActive(false);
        GameFlowManager.Instance?.RestartGame();
    }
    void OnMenuClicked()
    {
        Debug.Log("返回主菜单");
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        GameFlowManager.Instance?.ReturnToMainMenu();
    }
    void OnVictoryRestartClicked()
    {
        Debug.Log("胜利后重新启动");
        if (victoryPanel != null) victoryPanel.SetActive(false);
        GameFlowManager.Instance?.RestartGame();
    }

    // ===== 工具方法 =====
    string BuildHealthText(int health)
    {
        if (health <= 0) return "-1";

        int hearts = 0;
        for (int i = 0; i < health; i++)
            hearts += 1;

        return hearts + "";
    }
}

//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class UIManager : SingletonBase<UIManager>, IPersistent
//{
//    [Header("事件通道，（订阅）")]
//    [SerializeField] private IntEventChannel onScoreChanged;
//    [SerializeField] private IntEventChannel onHealthChanged;
//    [SerializeField] private VoidEventChannel onGameOver;
//    [SerializeField] private VoidEventChannel onWaveStart;

//    [Header("UI 引用")]
//    [SerializeField] private TextMeshProUGUI scoreText;
//    [SerializeField] private TextMeshProUGUI healthText;
//    [SerializeField] private TextMeshProUGUI waveText;
//    [SerializeField] private GameObject gameOverPanel;
//    [SerializeField] private TextMeshProUGUI finalScoreText;
//    [SerializeField] private Button restartButton;

//    void OnEnable()
//    {
//        //订阅事件
//        onScoreChanged?.Subscribe(OnScoreChanged);
//        onHealthChanged?.Subscribe(OnHealthChanged);
//        onGameOver?.Subscribe(OnGameOver);
//        onWaveStart?.Subscribe(OnWaveStart);

//        //绑定按钮
//        if (restartButton != null)
//        {
//            restartButton.onClick.AddListener(OnRestartButtonClicked);
//        }
//        //初始更新
//        RefreshUI();
//    }

//    void OnDisable()
//    {
//        //取消订阅
//        onScoreChanged?.Unsubscribe(OnScoreChanged);
//        onHealthChanged?.Unsubscribe(OnHealthChanged);
//        onGameOver?.Unsubscribe(OnGameOver);
//        onWaveStart?.Unsubscribe(OnWaveStart);

//        if (restartButton != null)
//        {
//            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
//        }
//    }

//    // 初始更新UI
//    void RefreshUI()
//    {
//        if (GameManager.Instance == null) return;
//        if (scoreText != null)
//            scoreText.text = $"Score:{GameManager.Instance.Score}";
//        if (healthText != null)
//            healthText.text = $"Blood: {GameManager.Instance.Health}";
//        if (waveText != null)
//            waveText.text = $"The {GameManager.Instance.WaveNumber}";

//        //隐藏游戏结束面板
//        if (gameOverPanel != null)
//            gameOverPanel.SetActive(false);
//    }


//    // ==== 事件回调 ====
//    void OnScoreChanged(int score)
//    {
//        if (scoreText != null)
//        {
//            scoreText.text = $"Score:{score}";
//        }
//        Debug.Log($"📱 UI 更新：分数 {score}");
//    }

//    void OnHealthChanged(int health)
//    {
//        if (healthText != null)
//        {
//            healthText.text = "Blood: " + health;
//        }
//        //if (healthText != null)
//        //{
//        //    // 用 ❤️ 显示生命值
//        //    int hearts = 1;
//        //    for (int i = 0; i < health; i++)
//        //        hearts += 1;
//        //    healthText.text = "Blood:" + hearts;
//        //}
//        //Debug.Log($"📱 UI 更新：生命 {health}");
//    }

//    void OnWaveStart()
//    {
//        if (waveText != null && GameManager.Instance != null)
//            waveText.text = $"The {GameManager.Instance.WaveNumber}";
//        Debug.Log($"📱 UI 更新：波次开始");
//    }

//    void OnGameOver()
//    {
//        if (gameOverPanel != null)
//            gameOverPanel.SetActive(true);
//        if (finalScoreText != null && GameManager.Instance != null)
//            finalScoreText.text = $"Last Score:{GameManager.Instance.Score}";
//        Debug.Log($"📱 UI 显示：游戏结束");
//    }

//    // ==== 按钮回调 ====
//    void OnRestartButtonClicked()
//    {
//        Debug.Log("🔄 重新开始游戏");
//        //重置游戏
//        GameManager.Instance?.ResetGame();
//        //隐藏面板
//        if (gameOverPanel != null)
//            gameOverPanel.SetActive(false);
//        //重新启用玩家
//        PlayerController player = FindFirstObjectByType<PlayerController>();
//        if (player != null)
//        {
//            // player.enabled = true;
//            player.transform.position = Vector3.zero;
//        }
//        // 清理旧敌人
//        // ✅ 改成回池
//        // foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
//        // {
//        //     EnemyPoolManager.Instance?.ReleaseEnemy(enemy.gameObject);
//        // }

//        //清理敌人的代码
//        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
//        {
//            EnemyPoolManager.Instance?.ReleaseEnemy(enemy.gameObject);
//        }
//    }

//}