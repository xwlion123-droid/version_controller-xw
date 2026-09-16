using UnityEngine;
using UnityEngine.Events;

public abstract class EventChannel<T> : ScriptableObject
{
    public UnityAction<T> OnEventRaised;

    public void RaiseEvent(T data)
    {
        OnEventRaised?.Invoke(data);
        Debug.Log($"<color=cyan>📢 事件触发：{name} (数据：{data})</color>");
    }

    public void Subscribe(UnityAction<T> listener)
    {
        OnEventRaised += listener;
        Debug.Log($"<color=blue>🔗 {listener.Method.Name} 订阅了 {name}</color>");
    }

    public void Unsubscribe(UnityAction<T> listener)
    {
        OnEventRaised -= listener;
        Debug.Log($"<color=orange>🔓 {listener.Method.Name} 取消订阅 {name}</color>");
    }

    public void ClearAll()
    {
        OnEventRaised = null;
    }

}

//具体事件类型
[CreateAssetMenu(fileName = "IntEventChannel_", menuName = "Events/Int Event Channel")]
public class IntEventChannel : EventChannel<int> { }

[CreateAssetMenu(fileName = "StringEventChannel_", menuName = "Events/String Event Channel")]
public class StringEventChannel : EventChannel<string> { }

[CreateAssetMenu(fileName = "FloatEventChannel_", menuName = "Events/Float Event Channel")]
public class FloatEventChannel : EventChannel<float> { }

[CreateAssetMenu(fileName = "BoolEventChannel_", menuName = "Events/Bool Event Channel")]
public class BoolEventChannel : EventChannel<bool> { }
