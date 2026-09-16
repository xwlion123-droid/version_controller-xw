using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用Event Channel(SO)替换GameManager的Action事件
/// 创建UIManager 订阅事件更新界面
/// 实现游戏结束面板
/// 在Inspector中可视化事件订阅关系
/// </summary>
[CreateAssetMenu(fileName = "VoidEventChannel_", menuName = "Events/Void Event Channel")]
public class VoidEventChannel : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
        Debug.Log($"<color=cyan>📢 事件触发：{name}</color>");
    }

    public void Subscribe(UnityAction listener)
    {
        OnEventRaised += listener;
        Debug.Log($"<color=blue>🔗 {listener.Method.Name} 订阅了 {name}</color>");
    }

    public void Unsubscribe(UnityAction listener)
    {
        OnEventRaised -= listener;
        Debug.Log($"<color=orange>🔓 {listener.Method.Name} 取消订阅 {name}</color>");
    }

    public void ClearAll()
    {
        OnEventRaised = null;
    }
}