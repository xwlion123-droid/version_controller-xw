using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用状态机：管理状态切换和更新
/// </summary>
public class StateMachine
{
    public IState CurrentState { get; private set; }

    // 存储所有状态（类型 → 实例）
    private Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

    /// <summary>
    /// 注册状态
    /// </summary>
    public void AddState<T>(T state) where T : IState
    {
        _states[typeof(T)] = state;
    }

    /// <summary>
    /// 切换到指定状态
    /// </summary>
    public void ChangeState<T>() where T : IState
    {
        Type type = typeof(T);

        if (!_states.ContainsKey(type))
        {
            Debug.LogError($"⚠️ 状态 {type.Name} 未注册！");
            return;
        }

        IState newState = _states[type];

        if (newState == CurrentState) return;

        // 退出旧状态
        CurrentState?.OnExit();

        // 切换
        CurrentState = newState;

        // 进入新状态
        CurrentState?.OnEnter();
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    public void Update()
    {
        CurrentState?.OnUpdate();
    }

    /// <summary>
    /// 获取指定类型的状态实例
    /// </summary>
    public T GetState<T>() where T : IState
    {
        if (_states.TryGetValue(typeof(T), out IState state))
            return (T)state;
        return default;
    }
}