/// <summary>
/// 接口状态：所有状态类实现此接口 
/// </summary>
public interface IState
{
    /// <summary>
    /// 进入状态时调用（初始化）
    /// </summary>
    void OnEnter();

    /// <summary>
    /// 每帧调用 （状态逻辑）
    /// </summary>
    void OnUpdate();

    /// <summary>
    /// 退出状态时调用（清理）
    /// </summary>
    void OnExit();
}