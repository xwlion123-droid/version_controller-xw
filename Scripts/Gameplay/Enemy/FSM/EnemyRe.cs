/// <summary>
/// 重构后：
/// EnemyAI.cs（主控，100行）
/// IState.cs（接口）
/// StateMachine.cs（状态机）
/// PatrolState.cs（巡逻）
/// ChaseState.cs（追击）
/// AttackState.cs（攻击）
/// FleeState.cs（逃跑）
/// </summary>
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("数据")]
    [SerializeField] private EnemyData enemyData;

    [Header("巡逻")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float patrolWaitTime = 1.5f;

    [Header("感知")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float loseTargetRange = 9f;

    [Header("追击")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("攻击")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int attackDamage = 1;

    [Header("逃跑")]
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float fleeHealthThreshold = 0.3f;
    [SerializeField] private float fleeDuration = 3f;
    [SerializeField] private float safeDistance = 8f;

    // ===== 运行时 =====
    private StateMachine _stateMachine;
    private Transform _player;
    private Vector3 _spawnPosition;
    private float _currentHealth;
    private MeshRenderer _renderer;

    // ===== 公共属性 =====
    public StateMachine StateMachine => _stateMachine;
    public Transform Player => _player;
    public Vector3 SpawnPosition => _spawnPosition;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => enemyData?.Health ?? 1f;

    public float DistanceToPlayer => _player == null ? float.MaxValue : Vector3.Distance(transform.position, _player.position);

    // 供状态类访问的参数
    public float PatrolSpeed => enemyData?.Speed ?? patrolSpeed;
    public float ChaseSpeed => enemyData?.Speed ?? chaseSpeed;
    public float PatrolRadius => patrolRadius;
    public float PatrolWaitTime => patrolWaitTime;
    public float DetectionRange => detectionRange;
    public float LoseTargetRange => loseTargetRange;
    public float AttackRange => attackRange;
    public float AttackInterval => attackInterval;
    public float FleeSpeed => fleeSpeed;
    public float FleeHealthThreshold => fleeHealthThreshold;
    public float FleeDuration => fleeDuration;
    public float SafeDistance => safeDistance;

    // ============================================================
    // Unity 生命周期
    // ============================================================
    void Awake()
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
        gameObject.tag = "Enemy";
        _spawnPosition = transform.position;

        // ✅ 初始化状态机
        _stateMachine = new StateMachine();
        _stateMachine.AddState(new PatrolState(this));
        _stateMachine.AddState(new ChaseState(this));
        _stateMachine.AddState(new AttackState(this));
        _stateMachine.AddState(new FleeState(this));
    }

    void Start()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
            _player = playerController.transform;

        _stateMachine.ChangeState<PatrolState>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // ✅ 统一玩家查找
        if (_player == null)
        {
            TryFindPlayer();
            if (_player == null) return;
        }

        // ✅ 只调用状态机
        _stateMachine.Update();
    }

    void TryFindPlayer()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
            _player = playerController.transform;
    }

    // ============================================================
    // 公共方法
    // ============================================================
    public void Initialize(EnemyData data)
    {
        enemyData = data;
        _currentHealth = data?.Health ?? 1f;
        _spawnPosition = transform.position;

        if (_renderer != null && data != null)
            _renderer.material.color = data.Color;

        float size = data?.Size ?? 1f;
        transform.localScale = Vector3.one * size;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        Debug.Log($"💥 {name} 受到 {damage} 伤害，剩余血量：{_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        // 逃跑判断
        float healthPercent = _currentHealth / MaxHealth;
        bool shouldFlee = healthPercent <= fleeHealthThreshold;

        if (shouldFlee && _stateMachine.CurrentState is not FleeState)
        {
            _stateMachine.ChangeState<FleeState>();
        }
        else if (!shouldFlee && _stateMachine.CurrentState is not ChaseState
                 && _stateMachine.CurrentState is not AttackState
                 && _stateMachine.CurrentState is not FleeState)
        {
            _stateMachine.ChangeState<ChaseState>();
        }
    }

    public void Attack()
    {
        Debug.Log($"<color=red>💥 {name} 攻击玩家！</color>");
        GameManager.Instance?.TakeDamage(attackDamage);
        AudioPoolManager.Instance?.PlayDamage();
    }

    void Die()
    {
        int score = enemyData?.ScoreValue ?? 10;
        GameManager.Instance?.AddScore(score);
        Debug.Log($"<color=green>✅ 击杀敌人！获得 {score} 分</color>");

        AudioPoolManager.Instance?.PlayExplosion();
        EffectPoolManager.Instance?.PlayExplosionEffect(transform.position);

        EnemyPoolManager.Instance?.ReleaseEnemy(gameObject);
    }

    public void ResetState()
    {
        _spawnPosition = transform.position;
        _currentHealth = enemyData?.Health ?? 1f;

        if (_renderer != null && enemyData != null)
            _renderer.material.color = enemyData.Color;

        _stateMachine.ChangeState<PatrolState>();
    }

    // ============================================================
    // 调试
    // ============================================================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? _spawnPosition : transform.position, patrolRadius);

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }
}