using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
abstract public partial class Unit : MonoBehaviour
{
    public Unit
        target,
        followers;


    [JsonPropertyAttribute]
    public Range
        healthRange,
        barrierRange;

    [JsonPropertyAttribute]
    public StatMultChain
        damage,

        critChance,

        critMult,

        reflect,

        armor,

        health,

        healing,

        healSpeed,

        vampirism,

        attackSpeed

        ;

    public bool hasBlock;
    public bool ableToFight = true;
    public bool Alive => healthRange._Val > 0;

    public void InitStats()
    {
        statInitChain.Invoke(this);
    }
    abstract protected void FirstInitStats();


    protected void InitHealth(float initialVal, float growthVal, float growthCost)
    {
        health = new StatMultChain(initialVal, growthVal, growthCost);

        health.chain.onRecalculateChain += () =>{ healthRange.UpgradeMax(health.Result); };

        healthRange = new Range(health.Result);

        healthRange.onLessThanZero += ()=>{ onDeathChain.Invoke(this); };
    }


    public ActionChain<Unit> statInitChain, onDeathChain;

    public List<Action> timeredActionsList;

    public Action makeAttack;
    public TakeDamageReflect takeDamageReflect;

    public static List<Unit> _Instances = new List<Unit>();

    protected void Awake()
    {
        _Instances.Add(this);

        takeDamageChain = new ActionChain<DoDamageArgs>();
        
        attackChain = new ActionChain<DoDamageArgs>();

        healingChain = new ActionChain<DoHealArgs>();

        takeHealChain = new ActionChain<DoHealArgs>();

        vampChain = new ActionChain<DoHealArgs>();

        timeredActionsList = new List<Action>();


        statInitChain = new ActionChain<Unit>();

        statInitChain.Add(0, (unit)=>{ FirstInitStats(); });

        onDeathChain = new ActionChain<Unit>();

        InitStats();

        new Attack(this);

        OnAwake();
    }

    abstract protected void OnAwake();


    public void AffectHP(float change)
    {
        healthRange._Val += change;
    }


    public StatBasedTimer attackTimer;

    public StatBasedTimer InitAttackTimer()
    {
        return attackTimer = new StatBasedTimer(attackSpeed);
    }

    public Timer healingTimer;

    public Timer InitHealingTimer()
    {
        return healingTimer = new StatBasedTimer(healSpeed);
    }

    protected void FixedUpdate()
    {
        if (ableToFight &&
            Alive &&
            target.ableToFight &&
            target.Alive
            )
            timeredActionsList.ForEach(action => action.Invoke());
    }



    protected DoDamageArgs MakeDamageArgs(float damage, bool isReflected = false)
    {
        return new DoDamageArgs(this, damage);
    }

    
    public IEnumerator OnDeath()
    {
        ableToFight = false;

        yield return RecoverOnRespawn();

        ableToFight = true;
    }

    public void CutoffAttackTimer()
    {
        attackTimer.Reset();
    }

    protected IEnumerator RecoverOnRespawn()
    {
        float
            softResetDelta = Time.deltaTime / SoftReset.respawnDuration,

            missingHP = healthRange._Max - healthRange._Val,
            healthDelta = softResetDelta * missingHP,

            attackTimerDelta = softResetDelta * attackTimer.endTime,
            healingTimerDelta = softResetDelta * healingTimer.endTime;
        bool
            isHealthMissing,
            isHealTimerFull,
            isAttackTimerFull;

        do
        {
            if (isHealthMissing = ( healthRange._Val < healthRange._Max ))
                AffectHP(healthDelta);

            if (isAttackTimerFull = (attackTimer.T > 0))
                attackTimer.T -= attackTimerDelta;

            if (healingTimer != null && (isHealTimerFull = (healingTimer.T > 0)))
                healingTimer.T -= healingTimerDelta;
                
            yield return new WaitForSeconds(Time.deltaTime);
        }
        while (isHealthMissing || isAttackTimerFull);
    }

    public IEnumerable<StatMultChain> AllStats()
    {
        var stats = this.GetType()
            .GetFields(BindingFlags.Instance)
            .Where(f=>f.FieldType == typeof(StatMultChain))
            .Select(f=>(StatMultChain)f.GetValue(this));

        foreach (var item in stats)
        {
            yield return item;
        }
    }
    
}

