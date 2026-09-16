using UnityEngine;

public abstract class SingletonBase<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                Debug.LogWarning($"[{typeof(T).Name}]正在退出，返回null");
                return null;
            }
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                    // 如果是持久单例，标记为不销毁
                    if (typeof(T).GetInterface(nameof(IPersistent)) != null)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        if (this is IPersistent)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}

//标记接口 持久单例
public interface IPersistent { }