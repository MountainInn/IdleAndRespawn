using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour, IPoolable
{
    [SerializeField]
    float
        lifetime,
        alphaLifetime;
    public Action onLifetimeEnd;

    [SpaceAttribute]
    [SerializeField] public Vector3 velocity;

    public Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    float t = 0f;

    void Update()
    {
        t += Time.deltaTime;

        if (t > lifetime )
        {
            onLifetimeEnd?.Invoke();;
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    public void AfterAcquired()
    {
        text.CrossFadeColor(text.color.SetA(0), alphaLifetime, true, true);
    }

    void OnDisable()
    {
        t = 0;
        text.color = text.color.SetA(1);
    }
}
