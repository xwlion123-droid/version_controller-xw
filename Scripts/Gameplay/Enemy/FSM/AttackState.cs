using UnityEngine;

public class AttackState : EnemyStateBase
{
    private float _attackCooldown;

    public AttackState(EnemyAI enemy) : base(enemy) { }

    public override void OnEnter()
    {
        Debug.Log($"<color=red>⚔️ {enemy.name} 进入攻击状态</color>");
        _attackCooldown = 0f;
    }

    public override void OnUpdate()
    {
        if (enemy.Player == null)
        {
            enemy.StateMachine.ChangeState<PatrolState>();
            return;
        }

        // 面向玩家
        LookAtImmediate(enemy.Player.position);

        // 冷却
        _attackCooldown -= Time.deltaTime;

        // 攻击
        if (_attackCooldown <= 0f)
        {
            enemy.Attack();
            _attackCooldown = enemy.AttackInterval;
        }

        // 玩家跑远 → 追击
        if (enemy.DistanceToPlayer > enemy.AttackRange * 1.3f)
        {
            enemy.StateMachine.ChangeState<ChaseState>();
        }
    }

    public override void OnExit()
    {
        _attackCooldown = 0f;
    }
}