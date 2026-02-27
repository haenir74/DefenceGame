using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();
    protected static bool applicationIsQuitting = false;
    protected bool isShuttingDown = false;
    
    public static bool IsQuitting => applicationIsQuitting;
    public static bool InstanceExists => instance != null;

    protected virtual bool DontDestroy => false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    
                    if (instance != null && (instance as Singleton<T>)?.isShuttingDown == true)
                    {
                        return null;
                    }

                    if (instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
    }


    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (DontDestroy) DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
        isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        isShuttingDown = true;
        if (instance == this)
        {
            instance = null;
        }
    }
}


