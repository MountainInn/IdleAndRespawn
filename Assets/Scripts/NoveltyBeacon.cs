using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

abstract public class NoveltyBeacon
{
    MonoBehaviour monoBehaviour;
    protected Image beacon;
    protected object targetObject;
    protected GoTweenConfig tweenConfig;
    public GoTween goTween {get; protected set;}
    Coroutine coroutine;
    public float checkInterval = 4f;

    Func<bool> dontStopWhile;
    public bool isPlaying;

    public NoveltyBeacon(MonoBehaviour monoBehaviour, Image beacon, object targetObject, Func<bool> dontStopWhile)
    {
        this.monoBehaviour = monoBehaviour;
        this.dontStopWhile = dontStopWhile;
        this.beacon = beacon;
        this.targetObject = targetObject;        
    }

    public void StartSignal()
    {
        if (!isPlaying)
        {
            beacon.gameObject.SetActive(true);
            goTween.play();

            coroutine = monoBehaviour.StartCoroutine(CheckWhenToStopSignal());

            isPlaying = true;
        }
    }

    public void StopSignal()
    {
        if (isPlaying)
        {
            goTween.pause();
            ResetState();
            beacon.gameObject.SetActive(false);

            if (coroutine != null)
            {
                monoBehaviour.StopCoroutine(coroutine);
                coroutine = null;
            }

            isPlaying = false;
        }
    }

    IEnumerator CheckWhenToStopSignal()
    {
        do
        {
            yield return new WaitForSecondsRealtime(checkInterval);
        }
        while (dontStopWhile.Invoke());

        yield return goTween.WaitForBackIterationCompletion();

        StopSignal();
    }

    abstract protected void ResetState();
}

public class NoveltyBeaconScale : NoveltyBeacon
{
    float
        originalScale,
        amplitude = 0.2f;

    public NoveltyBeaconScale(MonoBehaviour monoBehaviour, Image beacon, float amplitude, Func<bool> dontStopWhile)
        : base(monoBehaviour, beacon, beacon.transform, dontStopWhile)
    {
        tweenConfig =
            new GoTweenConfig()
            .vector3Prop("localScale", Vector3.one * amplitude, true)
            .setIterations(int.MaxValue, GoLoopType.PingPong)
            .setEaseType(GoEaseType.Linear)
            ;

        goTween = new GoTween(targetObject, .75f, tweenConfig, null);
        Go.addTween(goTween);
        goTween.pause();

        originalScale = beacon.transform.localScale.x;

        this.amplitude = amplitude;
    }

    protected override void ResetState()
    {
        beacon.transform.localScale = Vector3.one * originalScale;
    }
}

public class NoveltyBeaconColor : NoveltyBeacon
{
    Color
        begColor,
        endColor;


    public NoveltyBeaconColor(MonoBehaviour monoBehaviour, Image beacon, Color begColor, Color endColor, Func<bool> dontStopWhile)
        : base(monoBehaviour, beacon, beacon, dontStopWhile)
    {
        tweenConfig =
            new GoTweenConfig()
            .colorProp("color", endColor)
            .setIterations(int.MaxValue, GoLoopType.PingPong)
            .setEaseType(GoEaseType.Linear);

        goTween = new GoTween(targetObject, 1f, tweenConfig, null);
        Go.addTween(goTween);
        goTween.pause();

        this.begColor = begColor;
        this.endColor = endColor;

        beacon.color = begColor;
    }

    protected override void ResetState()
    {
        beacon.color = begColor;
    }
}
