using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPool<T> where T : Component
{
    private T _prefab;
    private Transform _parent;
    private Queue<T> _poolQueue = new Queue<T>();

    public ComponentPool(T prefab, int initialSize, Transform root)
    {
        _prefab = prefab;
        
        GameObject poolObj = new GameObject($"{prefab.name}_Pool");
        poolObj.transform.SetParent(root);
        _parent = poolObj.transform;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNew();
        }
    }

    private T CreateNew()
    {
        T instance = Object.Instantiate(_prefab, _parent);
        instance.gameObject.SetActive(false);
        
        var tracker = instance.gameObject.AddComponent<PoolObject>();
        tracker.OriginalPrefab = _prefab.gameObject;
        
        _poolQueue.Enqueue(instance);
        return instance;
    }

    public T Get()
    {
        if (_poolQueue.Count == 0)
        {
            CreateNew();
        }

        T instance = _poolQueue.Dequeue();
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Release(T instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(_parent);
        _poolQueue.Enqueue(instance);
    }
}

public class PoolObject : MonoBehaviour
{
    public GameObject OriginalPrefab;
}