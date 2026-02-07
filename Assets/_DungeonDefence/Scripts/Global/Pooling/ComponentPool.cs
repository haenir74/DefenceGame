using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPool<T> where T : Component
{
    private T prefab;
    private Transform parent;
    private Queue<T> poolQueue = new Queue<T>();

    public ComponentPool(T prefab, int initialSize, Transform root)
    {
        this.prefab = prefab;
        
        GameObject poolObj = new GameObject($"{prefab.name}_Pool");
        poolObj.transform.SetParent(root);
        parent = poolObj.transform;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNew();
        }
    }

    private T CreateNew()
    {
        T instance = Object.Instantiate(prefab, parent);
        instance.gameObject.SetActive(false);
        
        var tracker = instance.gameObject.AddComponent<PoolObject>();
        tracker.OriginalPrefab = prefab.gameObject;
        
        poolQueue.Enqueue(instance);
        return instance;
    }

    public T Get()
    {
        if (poolQueue.Count == 0)
        {
            CreateNew();
        }

        T instance = poolQueue.Dequeue();
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Release(T instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(parent);
        poolQueue.Enqueue(instance);
    }
}

public class PoolObject : MonoBehaviour
{
    public GameObject OriginalPrefab;
}