
using UnityEngine;

/// <summary>
/// 敌人状态基类：提供通用工具方法
/// 所有敌人状态类继承此类
/// </summary>
public abstract class EnemyStateBase : IState
{
    protected EnemyAI enemy;
    protected Transform transform;

    public EnemyStateBase(EnemyAI enemy)
    {
        this.enemy = enemy;
        this.transform = enemy.transform;
    }

    //子类可选择实现
    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }

    // ==== 通用工具方法 ====
    /// <summary>
    /// 移动到目标位置
    /// </summary>
    /// <param name="target"></param>
    /// <param name="speed"></param>
    protected void MoveTowards(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }

    /// <summary>
    /// 平滑朝向目标
    /// </summary>
    /// <param name="target"></param>
    /// <param name="speed"></param>
    protected void LookAtSmooth(Vector3 target, float speed = 5f)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * speed
            );
        }
    }

    /// <summary>
    /// 朝向目标（立即）
    /// </summary>
    /// <param name="target"></param>
    protected void LookAtImmediate(Vector3 target)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

}