/// <summary>
/// 波次状态
/// </summary>
public enum WaveState
{
    Idle,          // 空闲（未开始）
    Preparing,     // 准备中（玩家准备）
    Spawning,      // 生成中（敌人持续生成）
    Fighting,      // 战斗中（等待玩家清完）
    Completed,     // 波次完成（进入休息）
    AllCompleted,  // 所有波次完成
}