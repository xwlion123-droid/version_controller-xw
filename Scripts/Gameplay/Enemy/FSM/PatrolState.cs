using UnityEngine;

public class PatrolState : EnemyStateBase
{
    private Vector3 _patrolTarget;
    private float _waitTimer;

    public PatrolState(EnemyAI enemy) : base(enemy) { }

    public override void OnEnter()
    {
        Debug.Log($"<color=green>🚶 {enemy.name} 进入巡逻状态</color>");
        PickNewPatrolTarget();
        _waitTimer = 0f;
    }

    public override void OnUpdate()
    {
        // 等待计时
        if (_waitTimer > 0)
        {
            _waitTimer -= Time.deltaTime;
        }
        else
        {
            // 移动到巡逻点
            MoveTowards(_patrolTarget, enemy.PatrolSpeed);
            LookAtSmooth(_patrolTarget);

            // 到达 → 选新目标 + 等待
            if (Vector3.Distance(transform.position, _patrolTarget) < 0.3f)
            {
                _waitTimer = enemy.PatrolWaitTime;
                PickNewPatrolTarget();
            }
        }
        // ✅ 调试日志
        Debug.Log($"[{enemy.name}] 距离玩家：{enemy.DistanceToPlayer:F2}，检测范围：{enemy.DetectionRange}");
        // 发现玩家 → 追击
        if (enemy.DistanceToPlayer < enemy.DetectionRange)
        {
            enemy.StateMachine.ChangeState<ChaseState>();
        }
    }

    public override void OnExit()
    {
        _waitTimer = 0f;
    }

    void PickNewPatrolTarget()
    {
        Vector2 random = Random.insideUnitCircle * enemy.PatrolRadius;
        _patrolTarget = enemy.SpawnPosition + new Vector3(random.x, 0, random.y);
    }
}