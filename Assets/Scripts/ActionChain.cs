using System;
using UnityEngine;
using System.Collections.Generic;

public class ActionChain<T> where T : class
{
    SortedDictionary<int, Action<T>> callbacks = new SortedDictionary<int, Action<T>>();

    public ActionChain() {}

    public int Count => callbacks.Count;
    
    public void Add(Action<T> callback)
    {
        if( !callbacks.ContainsKey(0) )
        {
            callbacks.Add(0, default(Action<T>));
        }

        callbacks[0] += callback;
    }

    public void Add(int order, Action<T> callback)
    {
        if( !callbacks.ContainsKey(order) )
        {
            callbacks.Add(order, default(Action<T>));
        }

        callbacks[order] += callback;
    }

    public void Remove(int order, Action<T> callback)
    {
        callbacks[order] -= callback;
    }

    public T Invoke(T input)
    {
        foreach (var item in new List<Action<T>>(callbacks.Values))
        {
            item?.Invoke(input);
        }

        return input;
    }


    public void DebugOutput()
    {
        foreach (var key in callbacks.Keys)
        {
            foreach (var val in callbacks[key].GetInvocationList())
            {
                Debug.Log(key +" "+ val.ToString());
            }
        }
    }

}
