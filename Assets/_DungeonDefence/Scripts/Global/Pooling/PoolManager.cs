using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    protected override bool DontDestroy => true;

    private Dictionary<GameObject, ComponentPool<Component>> _pools = new Dictionary<GameObject, ComponentPool<Component>>();

    private Transform _rootContainer;

    protected override void Awake()
    {
        base.Awake();
        _rootContainer = new GameObject("Pool_Container").transform;
        if (DontDestroy) DontDestroyOnLoad(_rootContainer.gameObject);
    }

    public void CreatePool<T>(T prefab, int initialSize) where T : Component
    {
        if (_pools.ContainsKey(prefab.gameObject)) return;

        var pool = new ComponentPool<Component>(prefab, initialSize, _rootContainer);
        _pools.Add(prefab.gameObject, pool);
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (!_pools.ContainsKey(prefab.gameObject))
        {
            CreatePool(prefab, 5);
        }

        var component = _pools[prefab.gameObject].Get() as T;
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
        if (poolKey != null && _pools.ContainsKey(poolKey.OriginalPrefab))
        {
                if (obj is IPoolable poolable) poolable.OnDespawn();
            _pools[poolKey.OriginalPrefab].Release(obj);
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }
}