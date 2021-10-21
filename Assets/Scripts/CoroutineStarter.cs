using System;
using UnityEngine;
using System.Collections;

public class CoroutineStarter
{
    MonoBehaviour gameObject;
    IEnumerator enumerator;
    Coroutine coroutine;

    public CoroutineStarter(MonoBehaviour gameObject, IEnumerator enumerator)
    {
        this.gameObject = gameObject;
        this.enumerator = enumerator;
    }

    public void StartCoroutine()
    {
        if (coroutine == null)
            coroutine = gameObject.StartCoroutine(enumerator);
    }

    public void StopCoroutine()
    {
        if (coroutine != null)
        {
            gameObject.StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
