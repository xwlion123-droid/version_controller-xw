using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>, IPersistent
{
    [Header("事件通道，（订阅）")]
    [SerializeField] private IntEventChannel onScoreChanged;
    [SerializeField] private IntEventChannel onHealthChanged;
    [SerializeField] private VoidEventChannel onGameOver;
    [SerializeField] private VoidEventChannel onWaveStart;

    [Header("UI 引用")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    void OnEnable()
    {
        //订阅事件
        onScoreChanged?.Subscribe(OnScoreChanged);
        onHealthChanged?.Subscribe(OnHealthChanged);
        onGameOver?.Subscribe(OnGameOver);
        onWaveStart?.Subscribe(OnWaveStart);

        //绑定按钮
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        //初始更新
        RefreshUI();
    }

    void OnDisable()
    {
        //取消订阅
        onScoreChanged?.Unsubscribe(OnScoreChanged);
        onHealthChanged?.Unsubscribe(OnHealthChanged);
        onGameOver?.Unsubscribe(OnGameOver);
        onWaveStart?.Unsubscribe(OnWaveStart);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
    }

    // 初始更新UI
    void RefreshUI()
    {
        if (GameManager.Instance == null) return;
        if (scoreText != null)
            scoreText.text = $"Score:{GameManager.Instance.Score}";
        if (healthText != null)
            healthText.text = $"Blood: {GameManager.Instance.Health}";
        if (waveText != null)
            waveText.text = $"The {GameManager.Instance.WaveNumber}";

        //隐藏游戏结束面板
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }


    // ==== 事件回调 ====
    void OnScoreChanged(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score:{score}";
        }
        Debug.Log($"📱 UI 更新：分数 {score}");
    }

    void OnHealthChanged(int health)
    {
        if (healthText != null)
        {
            // 用 ❤️ 显示生命值
            int hearts = 1;
            for (int i = 0; i < health; i++)
                hearts += 1;
            healthText.text = "Blood:" + hearts;
        }
        Debug.Log($"📱 UI 更新：生命 {health}");
    }

    void OnWaveStart()
    {
        if (waveText != null && GameManager.Instance != null)
            waveText.text = $"The {GameManager.Instance.WaveNumber}";
        Debug.Log($"📱 UI 更新：波次开始");
    }

    void OnGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = $"Last Score:{GameManager.Instance.Score}";
        Debug.Log($"📱 UI 显示：游戏结束");
    }

    // ==== 按钮回调 ====
    void OnRestartButtonClicked()
    {
        Debug.Log("🔄 重新开始游戏");
        //重置游戏
        GameManager.Instance?.ResetGame();
        //隐藏面板
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        //重新启用玩家
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = true;
            player.transform.position = Vector3.zero;
        }
        // 清理旧敌人
        // ✅ 改成回池
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            EnemyPoolManager.Instance?.ReleaseEnemy(enemy.gameObject);
        }
    }

}