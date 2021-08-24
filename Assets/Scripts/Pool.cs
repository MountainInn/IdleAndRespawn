using System;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : Component
{
    public Transform poolObject;

    protected List<T> prefObjects;

    public List<T> pool { protected set; get; }
    public List<T> actives { protected set; get; }
    public event Action<T> onRelease, onAcquire, onDequed, onInstantiated;
    public event Action onAllReleased;

    public int totalNumber => pool.Count + actives.Count;
    public int activeNumber => actives.Count;


    public Pool(Transform poolObject, T prefObject, int preinstCount = 0) : this(poolObject, new List<T>(){prefObject}, preinstCount) {}

    public Pool(Transform poolObject, T[] prefObjects, int preinstCount = 0) : this(poolObject, new List<T>(prefObjects), preinstCount) {}

    public Pool(Transform poolObject, List<T> prefObjects, int preinstCount = 0)
    {
        this.prefObjects = prefObjects;

        pool = new List<T>();
        actives = new List<T>();

        this.poolObject = poolObject;

        for (int i = 0; i < preinstCount; i++)
        {
            T b = InstantiateObject();

            Release(b);
        }
    }

    public void FitActiveToNumber(int count)
    {
        int diff = activeNumber - count;

        if (diff > 0)           // Too much actives
        {
            for(int i = activeNumber-1; i > count; i--)
            {
                Release(actives[i]);
            }
        }
        else if (diff < 0)
        {
            for(int i = 0; i < Mathf.Abs(diff); i++)
            {
                Acquire();
            }
        }
    }

    protected T InstantiateObject()
    {
        T n = MonoBehaviour.Instantiate((prefObjects.Count == 1 ? prefObjects[0] : prefObjects.GetRandom()));

        n.gameObject.name = n.gameObject.name + totalNumber;

        onInstantiated?.Invoke(n.GetComponent<T>());

        return n;
    }


    public T AcquireAndPlace(Transform parent, Vector3 position, Quaternion rotation)
    {
        T newObject = Acquire();

        newObject.transform.SetParent(parent, false);
        newObject.transform.position = position;
        newObject.transform.rotation = rotation;
        newObject.transform.localScale = Vector3.one;

        return newObject;
    }

    public T Acquire()
    {
        T n = default;

        if(pool.Count > 0)
        {
            n = pool.ExtractFirst();

            onDequed?.Invoke(n);
        }
        else
        {
            n = InstantiateObject();
        }

        onAcquire?.Invoke(n);

        n.gameObject.SetActive(true);


        actives.Add(n);

        return n;
    }


    public void Release(T ob)
    {
        if(ob.gameObject != null && !ob.gameObject.activeInHierarchy) return;

        pool.Add(ob);

        ob.transform.SetParent(poolObject);

        ob.gameObject.SetActive(false);

        actives.Remove(ob);

        if (activeNumber == 0) onAllReleased?.Invoke();

        onRelease?.Invoke(ob.GetComponent<T>());
    }


    public T GetPrefab(int i = 0)
    {
        return prefObjects[i];
    }


    public IEnumerable<T> EachObject()
    {
        Debug.LogWarning("Пул содержит только неактивные объекты, остальные находятся в actives");

        foreach (var item in pool)
        {
            yield return item;
        }
    }

    public IEnumerable<T> EachActiveObject()
    {
        foreach (var item in actives)
        {
            if (item.gameObject.activeInHierarchy)
                yield return item;
        }
    }
}

public interface IPoolable
{
    public void AfterAcquired();
}
