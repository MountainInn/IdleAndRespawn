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
    public Range healthRange;

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

        OnAwake();
    }

    abstract protected void OnAwake();


    public void AffectHP(float change)
    {
        healthRange._Val += change;

        GameLogger.Logg("health", $"{gameObject.name} {change:+#;-#}");
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
        if (ableToFight && Alive)
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
        attackTimer.t = 0f;
    }

    protected IEnumerator RecoverOnRespawn()
    {
        float missingHP = healthRange._Max - healthRange._Val;
        bool
            isHealthMissing,
            isAttackTimerFull;

        do
        {
            if (isHealthMissing = ( healthRange._Val < healthRange._Max ))
                AffectHP( Time.deltaTime / SoftReset.respawnDuration * missingHP );

            if (isAttackTimerFull = (attackTimer.t > 0))
                attackTimer.t -= Time.deltaTime / SoftReset.respawnDuration * attackTimer.endTime;
                
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

