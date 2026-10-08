using UnityEngine;

public class ChaseState : EnemyStateBase
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

    public override void OnEnter()
    {
        Debug.Log($"<color=yellow>🏃 {enemy.name} 进入追击状态</color>");
    }

    public override void OnUpdate()
    {
        if (enemy.Player == null)
        {
            enemy.StateMachine.ChangeState<PatrolState>();
            return;
        }

        // 追向玩家
        MoveTowards(enemy.Player.position, enemy.ChaseSpeed);
        LookAtImmediate(enemy.Player.position);

        float distance = enemy.DistanceToPlayer;

        // 进入攻击范围 → 攻击
        if (distance < enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState<AttackState>();
        }
        // 玩家跑远 → 巡逻
        else if (distance > enemy.LoseTargetRange)
        {
            enemy.StateMachine.ChangeState<PatrolState>();
        }
    }
}