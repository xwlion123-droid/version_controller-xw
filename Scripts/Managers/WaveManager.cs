using System.Collections;
using UnityEngine;

/// <summary>
/// 波次管理器 - 工业级
/// 负责：波次切换、敌人生成、进度管理
/// </summary>
public class WaveManager : SingletonBase<WaveManager>, IPersistent
{
    [Header("波次配置")]
    [SerializeField] private WaveCollection waveCollection;

    [Header("生成器")]
    [SerializeField] private TestSpawner enemySpawner;  // 兼容你的现有代码

    [Header("事件通道")]
    [SerializeField] private IntEventChannel onWaveStart;
    [SerializeField] private IntEventChannel onWaveComplete;
    [SerializeField] private IntEventChannel onWavePrepare;
    [SerializeField] private VoidEventChannel onAllWavesComplete;

    [Header("调试")]
    [SerializeField] private bool debugMode = false;

    // ===== 运行时状态 =====
    private WaveState _currentState = WaveState.Idle;
    private int _currentWaveIndex = -1;
    private WaveData _currentWave;
    private float _stateTimer;
    private int _enemiesSpawnedThisWave = 0;
    private int _enemiesKilledThisWave = 0;
    private int _totalEnemiesThisWave = 0;
    private Coroutine _waveRoutine;

    // ===== 公共属性 =====
    public WaveState CurrentState => _currentState;
    public int CurrentWaveIndex => _currentWaveIndex;
    public int CurrentWaveNumber => _currentWaveIndex + 1;
    public WaveData CurrentWave => _currentWave;
    public float StateTimer => _stateTimer;
    public int EnemiesKilled => _enemiesKilledThisWave;
    public int TotalEnemies => _totalEnemiesThisWave;

    // ===== Unity 生命周期 =====
    void Start()
    {
        // 延迟一帧启动，确保所有单例初始化完成
        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        StartWaves();
    }

    // ===== 波次流程控制 =====

    /// <summary>
    /// 开始所有波次
    /// </summary>
    public void StartWaves()
    {
        if (waveCollection == null)
        {
            Debug.LogError("⚠️ WaveCollection 未配置！");
            return;
        }

        _currentWaveIndex = -1;
        StartNextWave();
    }

    /// <summary>
    /// 开始下一波
    /// </summary>
    void StartNextWave()
    {
        _currentWaveIndex++;

        if (_currentWaveIndex >= waveCollection.TotalWaves)
        {
            // 所有波次完成
            _currentState = WaveState.AllCompleted;
            Debug.Log("<color=cyan>🏁 所有波次完成！</color>");
            onAllWavesComplete?.RaiseEvent();
            return;
        }

        // 获取带难度曲线的波次
        _currentWave = waveCollection.GetWaveWithDifficulty(_currentWaveIndex);

        if (_currentWave == null)
        {
            Debug.LogError($"⚠️ 波次 {_currentWaveIndex} 配置为空！");
            return;
        }

        // 重置统计
        _enemiesSpawnedThisWave = 0;
        _enemiesKilledThisWave = 0;
        _totalEnemiesThisWave = _currentWave.GetTotalEnemyCount();

        // 进入准备阶段
        ChangeState(WaveState.Preparing);
    }

    /// <summary>
    /// 强制结束当前波（调试用）
    /// </summary>
    public void ForceCompleteWave()
    {
        if (_currentState == WaveState.Spawning || _currentState == WaveState.Fighting)
        {
            CompleteWave();
        }
    }

    // ===== 状态切换 =====

    void ChangeState(WaveState newState)
    {
        _currentState = newState;

        if (debugMode)
            Debug.Log($"<color=yellow>🌊 波次状态：{newState}</color>");

        switch (newState)
        {
            case WaveState.Preparing:
                OnEnterPreparing();
                break;
            case WaveState.Spawning:
                OnEnterSpawning();
                break;
            case WaveState.Fighting:
                OnEnterFighting();
                break;
            case WaveState.Completed:
                OnEnterCompleted();
                break;
        }
    }

    void OnEnterPreparing()
    {
        _stateTimer = _currentWave.PrepareDuration;

        Debug.Log($"<color=cyan>⏳ 波次 {CurrentWaveNumber} 准备中... {_stateTimer} 秒</color>");

        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(PrepareRoutine());
    }

    IEnumerator PrepareRoutine()
    {
        while (_stateTimer > 0)
        {
            _stateTimer -= Time.deltaTime;

            // 每整数秒广播一次
            int secondsLeft = Mathf.CeilToInt(_stateTimer);
            onWavePrepare?.RaiseEvent(secondsLeft);

            yield return null;
        }

        // 准备结束，开始生成
        ChangeState(WaveState.Spawning);
    }

    void OnEnterSpawning()
    {
        Debug.Log($"<color=green>🚀 波次 {CurrentWaveNumber} 开始！共 {_totalEnemiesThisWave} 个敌人</color>");

        // 广播波次开始事件
        onWaveStart?.RaiseEvent(CurrentWaveNumber);

        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 逐组生成敌人
        foreach (var config in _currentWave.EnemiesToSpawn)
        {
            if (config.enemyData == null) continue;

            for (int i = 0; i < config.count; i++)
            {
                SpawnSingleEnemy(config);
                _enemiesSpawnedThisWave++;

                yield return new WaitForSeconds(config.spawnInterval);
            }
        }

        Debug.Log($"<color=green>✅ 波次 {CurrentWaveNumber} 所有敌人已生成，进入战斗阶段</color>");

        // 进入战斗阶段（等待玩家清完）
        ChangeState(WaveState.Fighting);
    }

    void SpawnSingleEnemy(EnemySpawnConfig config)
    {
        if (EnemyPoolManager.Instance == null) return;

        // 计算生成位置
        Vector3 spawnPos = GetSpawnPosition(config);

        // 从池中获取敌人
        GameObject enemy = EnemyPoolManager.Instance.GetEnemy(
            spawnPos,
            Quaternion.identity,
            config.enemyData
        );

        // 应用难度倍率（可选）
        ApplyDifficulty(enemy, config.enemyData);
    }

    Vector3 GetSpawnPosition(EnemySpawnConfig config)
    {
        switch (config.positionMode)
        {
            case SpawnPositionMode.RandomAround:
                // 玩家周围
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    Vector2 offset = Random.insideUnitCircle.normalized * config.spawnRadius;
                    return player.transform.position + new Vector3(offset.x, 0, offset.y);
                }
                break;

            case SpawnPositionMode.RandomEdge:
                // 屏幕边缘（相机视口）
                if (Camera.main != null)
                {
                    float edge = Random.Range(0, 4);
                    float x = 0, z = 0;
                    switch ((int)edge)
                    {
                        case 0: x = -1; z = Random.Range(-1f, 1f); break;  // 左
                        case 1: x = 1; z = Random.Range(-1f, 1f); break;   // 右
                        case 2: x = Random.Range(-1f, 1f); z = 1; break;   // 上
                        case 3: x = Random.Range(-1f, 1f); z = -1; break;  // 下
                    }
                    Vector3 viewportPos = new Vector3(x, z, 0);
                    Vector3 worldPos = Camera.main.ViewportToWorldPoint(
                        new Vector3(0.5f + x * 0.6f, 0.5f + z * 0.6f, 10f)
                    );
                    worldPos.y = 0;
                    return worldPos;
                }
                break;
        }

        // 默认：原点
        return Vector3.zero;
    }

    void ApplyDifficulty(GameObject enemy, EnemyData data)
    {
        // TODO: 如果你的 EnemyAI 支持难度倍率，在这里应用
        // 目前暂不处理
    }

    void OnEnterFighting()
    {
        // 战斗阶段：等待所有敌人被击杀
        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(FightingRoutine());
    }

    IEnumerator FightingRoutine()
    {
        // 等待所有敌人被击杀
        while (_enemiesKilledThisWave < _totalEnemiesThisWave)
        {
            yield return new WaitForSeconds(0.5f);

            // 统计当前存活敌人数
            int alive = GetAliveEnemyCount();

            if (debugMode)
                Debug.Log($"⚔️ 战斗中 | 已击杀：{_enemiesKilledThisWave}/{_totalEnemiesThisWave} | 存活：{alive}");
        }

        // 所有敌人被击杀，波次完成
        CompleteWave();
    }

    int GetAliveEnemyCount()
    {
        return FindObjectsByType<EnemyAI>(FindObjectsSortMode.None).Length;
    }

    void CompleteWave()
    {
        Debug.Log($"<color=cyan>🎉 波次 {CurrentWaveNumber} 完成！</color>");

        // 广播波次完成事件
        onWaveComplete?.RaiseEvent(CurrentWaveNumber);

        ChangeState(WaveState.Completed);
    }

    void OnEnterCompleted()
    {
        _stateTimer = _currentWave.NextWaveDelay;

        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(NextWaveRoutine());
    }

    IEnumerator NextWaveRoutine()
    {
        Debug.Log($"<color=yellow>💤 休息 {_stateTimer} 秒后进入下一波...</color>");

        yield return new WaitForSeconds(_stateTimer);

        // 开始下一波
        StartNextWave();
    }

    // ===== 回调：敌人被击杀时调用 =====

    /// <summary>
    /// 敌人被击杀时调用（由 EnemyAI.Die 调用）
    /// </summary>
    public void OnEnemyKilled()
    {
        _enemiesKilledThisWave++;
    }
}