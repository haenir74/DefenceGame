using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    protected override bool DontDestroy => true;

    private Dictionary<GameObject, ComponentPool<Component>> pools = new Dictionary<GameObject, ComponentPool<Component>>();

    private Transform rootContainer;

    protected override void Awake()
    {
        base.Awake();
        rootContainer = new GameObject("Pool_Container").transform;
        if (DontDestroy) DontDestroyOnLoad(rootContainer.gameObject);
    }

    public void CreatePool<T>(T prefab, int initialSize) where T : Component
    {
        if (pools.ContainsKey(prefab.gameObject)) return;

        var pool = new ComponentPool<Component>(prefab, initialSize, rootContainer);
        pools.Add(prefab.gameObject, pool);
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (!pools.ContainsKey(prefab.gameObject))
        {
            CreatePool(prefab, 5);
        }

        var component = pools[prefab.gameObject].Get() as T;
        if (component != null)
        {
            component.transform.SetPositionAndRotation(position, rotation);
            
            if (component is IPoolable poolable)
            {
                poolable.OnSpawn();
            }
        }
        return component;
    }

    public void Despawn<T>(T obj) where T : Component
    {        
        var poolKey = obj.GetComponent<PoolObject>();
        if (poolKey != null && pools.ContainsKey(poolKey.OriginalPrefab))
        {
                if (obj is IPoolable poolable) poolable.OnDespawn();
            pools[poolKey.OriginalPrefab].Release(obj);
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }

    public T Pop<T>(T prefab) where T : Component
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity);
    }

    public void Push<T>(T obj) where T : Component
    {
        Despawn(obj);
    }
}