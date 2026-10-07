using UnityEngine;

public class GameManager : SingletonBase<GameManager>, IPersistent
{
    [Header("玩家数据")]
    [SerializeField] private PlayerData playerData;

    [Header("事件通道")]
    [SerializeField] private IntEventChannel onScoreChanged;
    [SerializeField] private IntEventChannel onHealthChanged;
    [SerializeField] private VoidEventChannel onGameOver;
    [SerializeField] private VoidEventChannel onWaveStart;

    [Header("游戏状态")]
    [SerializeField] private int score = 0;
    [SerializeField] private int health = 3;
    [SerializeField] private int waveNumber = 1;
    [SerializeField] private bool isGameOver = false;

    //公共属性
    public int Score => score;
    public int Health => health;
    public int WaveNumber => waveNumber;
    public bool IsGameOver => isGameOver;
    public PlayerData PlayerData => playerData;

    //事件 
    // public System.Action<int> OnScoreChanged;
    // public System.Action<int> OnHealthChanged;
    // public System.Action OnGameOver;
    // public System.Action OnWaveStart;

    protected override void Awake()
    {
        base.Awake();
        //如果没有配置PlayerData,用默认值
        if (playerData == null)
        {
            Debug.LogWarning("未配置PlayerData,使用默认值");
        }
    }

    void Start()
    {
        ResetGame();
    }

    //========公共方法=======
    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        onScoreChanged?.RaiseEvent(score);

        // ✅ 播放得分音效
        // AudioPoolManager.Instance?.PlayScore();
        Debug.Log($"<color=green>得分：+{amount},总分：{score}</color>");
    }

    public void TakeDamage(int damage)
    {
        if (isGameOver) return;
        health -= damage;
        // OnHealthChanged?.Invoke(health);
        onHealthChanged?.RaiseEvent(health);
        Debug.Log($"<color=red>受到{damage}伤害，剩余生命：{health}</color>");

        if (health <= 0)
        {
            health = 0;
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (isGameOver) return;

        int maxHealth = playerData?.MaxHealth ?? 3;
        health = Mathf.Min(health + amount, maxHealth);
        // OnHealthChanged?.Invoke(health);
        onHealthChanged?.RaiseEvent(health);
        Debug.Log($"<color=green>恢复{amount}生命，当前：{health}</color>");
    }

    public void StartWave()
    {
        if (isGameOver) return;
        // OnWaveStart?.Invoke();
        onWaveStart?.RaiseEvent();
        Debug.Log($"<color=cyan>第 {waveNumber} 波开始！</color>");
    }

    public void ComplateWave()
    {
        waveNumber++;
        Debug.Log($"<color=cyan> 第 {waveNumber - 1}波完成！</color>");
        //可以再这里添加奖励
        AddScore(50);
    }

    public void ResetGame()
    {
        score = 0;
        health = playerData?.MaxHealth ?? 3;
        waveNumber = 1;
        isGameOver = false;


        onScoreChanged?.RaiseEvent(score);
        onHealthChanged?.RaiseEvent(health);
        Debug.Log("<color=yellow>游戏已经重置</color>");
    }

    void GameOver()
    {
        isGameOver = true;
        // OnGameOver?.Invoke();
        onGameOver?.RaiseEvent();
        Debug.Log("<color=red>游戏结束，最终分数" + score + "</color>");
    }
}