using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

[SerializableAttribute, JsonObjectAttribute(MemberSerialization.OptIn)]
public class CallbackTimer
{
    [SerializeField] public float endTime;

    float _t;
    [HideInInspector, JsonPropertyAttribute]
    public float T
    {
        get => _t ;
        protected set
        {
            _t = value ;
            onRatioChanged?.Invoke(GetRatio());
        }
    }

    public Action<float> onRatioChanged;
    public Action onFinished;

    public CallbackTimer(float max)
    {
        this.endTime = max;
        T = 0f;
    }

    public void Tick(float seconds)
    {
        T += seconds;

        while (T >= endTime )
        {
            T -= endTime;

            onFinished?.Invoke();
        }
    }

    public float GetRatio() => T / endTime;
}

[SerializableAttribute, JsonObjectAttribute(MemberSerialization.OptIn)]
public class Timer
{
    [SerializeField] public float endTime;

    [HideInInspector, JsonPropertyAttribute]
    float _t;

    public float T
    {
        get => _t;
        set
        {
            _t = value;

            onRatioChanged?.Invoke(GetRatio());
        }
    }

    [HideInInspector] public Action<float> onRatioChanged;
    [HideInInspector] public bool isLooping = true, isFinished;

    public Timer(float max)
    {
        this.endTime = max;
        _t = 0f;
    }

    public void AddSeconds(float diff)
    {
        _t += diff;

        T = Mathf.Clamp(_t, 0.0f, endTime);
    }

    public void Reset()
    {
        T = 0;
        isFinished = false;
    }

    public bool Countup()
    {
        if (_t < endTime)
        {
            AddSeconds(Time.deltaTime);

            return false;
        }
        else
        {
            isFinished = true;

            onRatioChanged?.Invoke(GetRatio());

            return isFinished;
        }
    }

    public bool Tick()
    {
        T += Time.deltaTime;

        if (_t >= endTime )
        {
            T = 0;

            return true;
        }
        return false;
    }

    public void SetEndTime(float endTime)
    {
        this.endTime = endTime;
    }

    public float GetRatio() => _t / endTime;
}

public class StatBasedTimer : Timer
{
    public StatMultChain endtimeStat;

    public StatBasedTimer(StatMultChain stat) : base(stat.Result)
    {
        endtimeStat = stat;

        SetStat(stat);
    }

    public void SetStat(StatMultChain stat)
    {
        endtimeStat.chain.onRecalculateChain += UpdateEndtime;
    }

    public void UpdateEndtime() => SetEndTime(endtimeStat.Result);
}
