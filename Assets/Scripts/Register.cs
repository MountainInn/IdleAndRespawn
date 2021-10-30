using UnityEngine;
using System.Collections.Generic;

abstract public class Register<T> : MonoBehaviour
    where T : class
{
    static public List<T> instances = new List<T>();

    protected void RegisterSelf()
    {
        instances.Add(this as T);
    }
}

