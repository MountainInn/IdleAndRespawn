using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.Ecs;
using Newtonsoft.Json;


#region Progress Bar

public struct Progress { public float current, max; }

public struct ProgBarRef
{
    public ProgBarRef(Image progImage)
    {
        this.progImage = progImage;
        this.progImage.fillAmount = 1f;
    }
    public Image progImage;
}

public struct EvFinished : IEcsIgnoreInFilter {}

public struct EvTookDamage : IEcsIgnoreInFilter {}

public struct EvUpgVamp : IEcsIgnoreInFilter {}


#endregion

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


public struct Health
{
    public float current, max;

    public Health(float maxHealth)
    {
        current = max = maxHealth;
    }

    public void Add(float val)
    {
        current = Mathf.Clamp(current + val, 0, max);
    }
}


public struct Body
{
    public Health health;
}

public struct Arms
{
    public float
        damage,
        vampirism,

        critChance,
        critMult;
}



#region Flags

public struct FlagHero : IEcsIgnoreInFilter {}
public struct FlagBoss : IEcsIgnoreInFilter {}
public struct FlagFollower : IEcsIgnoreInFilter {}

#endregion

public struct DoubleEndedProgressRef { public DoubleEndedProgress val; }

public struct Cost
{
    public int valCost;
}

public struct Level
{
    public int valLevel;
}

public struct Talents
{
    public int valTalents;
}

public struct ButtonRef { public Button val; }
