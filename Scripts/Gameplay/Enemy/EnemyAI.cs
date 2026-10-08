using UnityEngine;
using UnityEditor;
using System.Linq;

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
    [SerializeField] private float loseTargetRange = 9f;//丢失玩家距离（缓冲，防止频繁切换

    [Header("追击")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("攻击")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int attackDamage = 1;

    //逃跑相关
    [Header("逃跑")]
    [SerializeField] private float fleeSpeed = 5f;//逃跑速度
    [SerializeField] private float fleeHealthThreshold = 0.3f;//血量低于30%
    [SerializeField] private float fleeDuration = 3f;//逃跑持续时间
    [SerializeField] private float safeDistance = 8f;//距离玩家超过这个距离

    // ===== 运行时状态 =====
    private EnemyState _currentState = EnemyState.Patrol;
    private Transform _player;
    private Vector3 _spawnPosition;
    private float _currentHealth;
    private MeshRenderer _renderer;

    //巡逻相关
    private Vector3 _patrolTarget;
    private float _patrolWaitTimer;

    //攻击相关
    private float _attackCooldown;

    //逃跑相关
    private float _fleeTimer;

    //公共属性 (供外部访问)
    public EnemyState CurrentState => _currentState;
    public float DistanceToPlayer => _player == null ? float.MaxValue : Vector3.Distance(transform.position, _player.position);

    //==== Unity 生命周期====
    void Awake()
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
        gameObject.tag = "Enemy";
        _spawnPosition = transform.position;
    }

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        _currentHealth = enemyData?.Health ?? 1f;
        _spawnPosition = transform.position;

        if (_renderer != null && enemyData != null)
        {
            _renderer.material.color = enemyData.Color;
        }

        float size = enemyData?.Size ?? 1f;
        transform.localScale = Vector3.one * size;
    }

    void Start()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            _player = playerController.transform;
        }
        //初始状态
        ChangeState(EnemyState.Patrol);
    }

    void Update()
    {
        //游戏结束时不更新
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        //核心：状态驱动的更新
        switch (_currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Flee:
                UpdateFlee();
                break;
        }
    }

    //==========================================
    //状态切换
    //==========================================
    void ChangeState(EnemyState newState)
    {
        if (_currentState == newState) return;
        //退出旧状态
        OnExitState(_currentState);
        //更新状态
        _currentState = newState;
        //进入新状态
        OnEnterState(newState);
    }

    void OnExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:
                //清理巡逻相关
                break;
            case EnemyState.Chase:
                //清理追击相关
                break;
            case EnemyState.Attack:
                //清理攻击相关
                _attackCooldown = 0f;
                break;
            case EnemyState.Flee:
                _fleeTimer = 0f;
                break;
        }
    }

    void OnEnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:
                Debug.Log($"<color=green> {name} 进入巡逻状态");
                PickNewPatrolTarget();
                break;
            case EnemyState.Chase:
                Debug.Log($"<color=yellow>{name}进入追击状态</color>");
                break;
            case EnemyState.Attack:
                Debug.Log($"<color=red> {name} 进入攻击状态</color>");
                _attackCooldown = 0f;
                break;
            case EnemyState.Flee:
                Debug.Log($"<color=cyan> {name} 进入逃跑状态</color>");
                _fleeTimer = fleeDuration;
                break;
        }
    }
    //=================================================
    //Patrol:巡逻状态
    //=================================================
    void UpdatePatrol()
    {
        //有巡逻目标-》移动过去
        if (_patrolWaitTimer > 0)
        {
            _patrolWaitTimer -= Time.deltaTime;
        }
        else
        {
            float speed = enemyData?.Speed ?? patrolSpeed;
            transform.position = Vector3.MoveTowards(
                transform.position,
                _patrolTarget,
                speed * Time.deltaTime
            );
            //面向移动方向
            Vector3 direction = (_patrolTarget - transform.position);
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 5f
                );
            }
            //到达巡逻点->等待一会再选新目标
            if (Vector3.Distance(transform.position, _patrolTarget) < 0.3f)
            {
                _patrolWaitTimer = patrolWaitTime;
                PickNewPatrolTarget();
            }
            //状态转换条件：发现玩家
            if (DistanceToPlayer < detectionRange)
            {
                ChangeState(EnemyState.Chase);
            }
        }
    }

    void PickNewPatrolTarget()
    {
        Vector2 random = Random.insideUnitCircle * patrolRadius;
        _patrolTarget = _spawnPosition + new Vector3(random.x, 0, random.y);
    }

    //===================================================
    //Chase:追击状态
    //===================================================
    void UpdateChase()
    {
        if (_player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
        //追向玩家
        Vector3 direction = (_player.position - transform.position).normalized;
        float speed = enemyData?.Speed ?? chaseSpeed;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(_player);

        float distance = DistanceToPlayer;

        //状态转换：进入攻击范围
        if (distance < attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        //状态转换：玩家跑远
        else if (distance > loseTargetRange)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    //======================================================
    //Attack:攻击状态
    //======================================================
    void UpdateAttack()
    {
        if (_player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
        //面向玩家
        transform.LookAt(_player);
        //攻击冷却
        _attackCooldown -= Time.deltaTime;
        if (_attackCooldown <= 0f)
        {
            Attack();
            _attackCooldown = attackInterval;
        }
        // 状态转换：玩家跑远（加一点缓冲，防止频繁切换）
        if (DistanceToPlayer > attackRange * 1.3f)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    void Attack()
    {
        Debug.Log($"<color=red> {name}攻击玩家 </color>");
        GameManager.Instance?.TakeDamage(attackDamage);
        //播放攻击音效：用你已经先实现的AudioPoolManager
        AudioPoolManager.Instance?.PlayDamage();
    }

    //==================================================
    //Flee:逃跑状态(先留空，后面实现)
    //==================================================
    void UpdateFlee()
    {
        if (_player == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
        //倒计时
        _fleeTimer -= Time.deltaTime;
        //远离玩家：反方向移动
        Vector3 fleeDirection = (transform.position - _player.position).normalized;
        fleeDirection.y = 0;//保持在水平面
        //移动：用武力或者直接改变位置
        transform.position += fleeDirection * fleeSpeed * Time.deltaTime;

        //面朝逃跑方向
        if (fleeDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fleeDirection), Time.deltaTime * 5f);
        }

        //状态转换条件
        //1、逃跑时间结束
        //2、距离玩家足够远（安全）
        bool timeUp = _fleeTimer <= 0f;
        bool safeEnough = DistanceToPlayer > safeDistance;
        if (timeUp || safeEnough)
        {
            //如果血量恢复了 ->追击玩家
            //否则 -> 回到巡逻
            float healthPercent = _currentHealth / (enemyData?.Health ?? 1f);
            if (healthPercent > fleeHealthThreshold)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    //==================================================
    //伤害处理
    //=================================================
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        Debug.Log($"{name}收到{damage}伤害，剩余血量:{_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        // 检查是否需要逃跑
        float healthPercent = _currentHealth / (enemyData?.Health ?? 1f);
        bool shouldFlee = healthPercent <= fleeHealthThreshold;
        if (shouldFlee && _currentState != EnemyState.Flee)
        {
            //血量过低->逃跑
            ChangeState(EnemyState.Flee);
        }
        else if (!shouldFlee && _currentState != EnemyState.Chase && _currentState != EnemyState.Attack && _currentState != EnemyState.Flee)
        {
            //血量正常 + 不在追击/攻击/逃跑 -> 追击玩家（被打还手）
            ChangeState(EnemyState.Chase);
        }

        // else
        // {
        //     //受伤后进入追击状态（被打当然要还手）
        //     if (_currentState != EnemyState.Chase && _currentState != EnemyState.Attack)
        //     {
        //         ChangeState(EnemyState.Chase);
        //     }
        //     //TODO:血量低于30%时进入
        // }
    }

    void Die()
    {
        int score = enemyData?.ScoreValue ?? 10;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(score);
            Debug.Log($"<color=green>✅ 击杀敌人！获得 {score} 分</color>");
        }
        //播放爆炸音效
        AudioPoolManager.Instance?.PlayExplosion();
        //播放爆炸特效
        EffectPoolManager.Instance?.PlayExplosionEffect(transform.position);
        //回池
        ReturnPool();
    }

    void ReturnPool()
    {
        //如果有对象池->回池
        EnemyPoolManager.Instance?.ReleaseEnemy(gameObject);
    }

    //=====================================================
    //供对象池调用 ：重置状态
    //=====================================================
    public void ResetState()
    {
        _spawnPosition = transform.position;
        _currentHealth = enemyData?.Health ?? 1f;
        _currentState = EnemyState.Patrol;
        _patrolWaitTimer = 0f;
        _attackCooldown = 0f;
        _fleeTimer = 0f;//重置逃跑计划

        //重新初始化外观
        if (_renderer != null && enemyData != null)
        {
            _renderer.material.color = enemyData.Color;
        }
    }

    //========================================================
    //调试：再Scene视图中显示当前状态
    //=======================================================
    void OnDrawGizmosSelected()
    {
        //显示检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        //显示丢失范围
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, loseTargetRange);
        //显示攻击范围
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        //显示巡逻范围
        Gizmos.color = Color.green;
        Vector3 spawn = Application.isPlaying ? _spawnPosition : transform.position;

        Gizmos.DrawWireSphere(spawn, patrolRadius);

        //逃跑安全距离
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);//半透明青色
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        //显示巡逻
        if (Application.isPlaying && _currentState == EnemyState.Patrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_patrolTarget, 0.3f);
            Gizmos.DrawLine(transform.position, _patrolTarget);
        }
    }

#if UNITY_EDITOR
void OnGUI()
{
    if (!Application.isPlaying) return;
    if (!Selection.gameObjects.Contains(gameObject)) return;

    // 在敌人头顶显示状态和血量
    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
    if (screenPos.z > 0)
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 20),
            $"{_currentState}");
        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y + 20, 100, 20),
            $"HP: {_currentHealth:F1}/{enemyData?.Health ?? 1f:F1}");
    }
}
#endif

}