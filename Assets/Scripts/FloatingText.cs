using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField]
    float
        lifetime,
        alphaLifetime;
    public Action onLifetimeEnd;

    [SpaceAttribute]
    [SerializeField] public Vector3 velocity;

    public Text text;

    GoTweenConfig goConfig;
    GoTween goTween;

    void Awake()
    {
        goConfig = new GoTweenConfig()
            .setIterations(1)
            .setDelay(lifetime/2)
            .setEaseType(GoEaseType.Linear);

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

    public void OnEnable()
    {
        Go.to(text,
              lifetime/2,
              goConfig.colorProp("color", text.color.SetA(0)));
    }

    void OnDisable()
    {
        t = 0;
        text.color = text.color.SetA(1);
        goConfig.clearProperties();
    }
}
