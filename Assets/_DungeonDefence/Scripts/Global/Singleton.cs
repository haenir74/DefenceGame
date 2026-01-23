using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return null; 
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
    
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _applicationIsQuitting = true;
        }
    }
}