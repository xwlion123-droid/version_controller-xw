using UnityEngine;

public class FleeState : EnemyStateBase
{
    private float _fleeTimer;

    public FleeState(EnemyAI enemy) : base(enemy) { }

    public override void OnEnter()
    {
        Debug.Log($"<color=cyan>🏃‍♂️ {enemy.name} 进入逃跑状态</color>");
        _fleeTimer = enemy.FleeDuration;

        // 播放逃跑音效（可选）
        AudioPoolManager.Instance?.PlaySFX("Damage", 0.5f, 1.5f);
    }

    public override void OnUpdate()
    {
        if (enemy.Player == null)
        {
            enemy.StateMachine.ChangeState<PatrolState>();
            return;
        }

        _fleeTimer -= Time.deltaTime;

        // 远离玩家
        Vector3 fleeDirection = (transform.position - enemy.Player.position).normalized;
        fleeDirection.y = 0;

        transform.position += fleeDirection * enemy.FleeSpeed * Time.deltaTime;
        LookAtSmooth(transform.position + fleeDirection);

        // 状态转换
        bool timeUp = _fleeTimer <= 0f;
        bool safeEnough = enemy.DistanceToPlayer > enemy.SafeDistance;

        if (timeUp || safeEnough)
        {
            float healthPercent = enemy.CurrentHealth / enemy.MaxHealth;

            if (healthPercent > enemy.FleeHealthThreshold)
                enemy.StateMachine.ChangeState<ChaseState>();
            else
                enemy.StateMachine.ChangeState<PatrolState>();
        }
    }

    public override void OnExit()
    {
        _fleeTimer = 0f;
    }
}