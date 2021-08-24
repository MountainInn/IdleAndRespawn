using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;


[JsonObjectAttribute(MemberSerialization.OptIn)]
abstract public class DamageProcessing
{
    protected Unit unit;

    
    protected DamageProcessing(Unit unit)
    {
        this.unit = unit;
    }

    public Unit GetUnit() => unit;
    

    virtual public bool CanActivate()
    {
        return true;
    }

    

    public void Activate()
    {
        if (!CanActivate()) return;
        
        Type thisType = this.GetType();

        SubclassActivation();

        IncludeMethodsInChains(true, thisType);

        foreach (var button in thisType.GetFields(BindingFlags.Instance).OfType<UpgradeButton>() )
        {
            if ( button != null ) button.gameObject.SetActive(true);
        }

    }

    protected virtual void SubclassActivation(){}

    public void Deactivate()
    {
        Type thisType = this.GetType();

        SubclassDeactivation();
        
        IncludeMethodsInChains(true, thisType);

        foreach (var field in thisType.GetFields(BindingFlags.Instance))
        {
            if (field.GetType() == typeof(Unit)) continue;

            if (field.GetType() == typeof(UpgradeButton)) 
            {
                ( (UpgradeButton)( field.GetValue(this) ) ).gameObject.SetActive(false);
            }

            field.SetValue(this, default);
        }
    }

    protected virtual void SubclassDeactivation() {}

    void IncludeMethodsInChains(bool toggle, Type thisType)
    {

        foreach (var method in thisType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance) )
        {
            foreach (Order attr in method.GetCustomAttributes().Where(a => a.GetType().BaseType == typeof( Order )))
            {
                if (attr is AttackOrder ato)
                {
                    AddOrRemoveMethod(unit.attackChain, ato, method, typeof(Action<DoDamageArgs>), toggle);
                }
                else if (attr is TakeDamageOrder tdo)
                {
                    AddOrRemoveMethod(unit.takeDamageChain, tdo, method, typeof(Action<DoDamageArgs>), toggle);
                }
                else if (attr is VampOrder vo)
                {
                    AddOrRemoveMethod(unit.vampChain, vo, method, typeof(Action<DoHealArgs>), toggle);
                }
                else if (attr is TakeHealOrder)
                {
                    AddOrRemoveMethod(unit.vampChain, attr, method, typeof(Action<DoHealArgs>), toggle);
                }
                else if (attr is HealOrder ho)
                {
                    AddOrRemoveMethod(unit.healingChain, ho, method, typeof(Action<DoHealArgs>), toggle);
                }
                else if (attr is StatInitOrder sio)
                {
                    AddOrRemoveMethod(unit.statInitChain, sio, method, typeof(Action<Unit>), toggle);
                }
                else if (attr is TimeredActionsUNOrdered uNOrdered)
                {
                    AddOrRemoveMethod(unit.timeredActionsList, uNOrdered, method, typeof(Action), toggle);
                }
                else if (attr is OnDeathOrder)
                {
                    AddOrRemoveMethod(unit.onDeathChain, attr, method, typeof(Action<Unit>), toggle);
                }
            }
        }

    }

    void AddOrRemoveMethod<T>(ActionChain<T> chain, Order attribOrder, MethodInfo method, Type delegateType, bool toggle)
            where T : class
        {
            chain
                .GetType()
                .GetMethod(toggle ? "Add" : "Remove", new Type[]{ typeof(int), delegateType })
                .Invoke(
                    chain,
                    new object[]
                    {
                        attribOrder.order,
                        method.CreateDelegate(delegateType, this)
                    });

            }
    
    void AddOrRemoveMethod<T>(List<T> list, Order attribOrder, MethodInfo method, Type delegateType, bool toggle)
            where T : class
        {
            list
                .GetType()
                .GetMethod(toggle ? "Add" : "Remove")
                .Invoke(
                    list,
                    new object[]
                    {
                        method.CreateDelegate(delegateType, this)
                    });
            }
}

abstract public class FollowersHealthTo : DamageProcessing
{
    protected Range fHealth;
    protected float maxHealthMult = 1.50f;

    public FollowersHealthTo(Followers followers, float maxHealthMult) : base(followers)
    {
        fHealth = followers.healthRange;
    }

    protected float GetHealthMult()
    {
        return fHealth.GetRatio() * maxHealthMult;
    }
}



public class FollowersHealthToDamage : FollowersHealthTo
{
    public FollowersHealthToDamage(Followers followers, float maxHealthMult)
        : base(followers, maxHealthMult)
    {
    }

    [AttackOrder(-20)] void FHealthToDamage(DoDamageArgs dargs)
    {
        dargs.damage._Val *= GetHealthMult();
    }
}



public class LoyaltyStat : StatMultChain
{
    static public ArithmeticNode healthAddition, armorAddition;

    public LoyaltyStat(float initialValue, float valGrowth, float costGrowth) : base(initialValue, valGrowth, costGrowth)
    {
        healthAddition = new ArithmeticNode(new ArithmAdd(), HealthToAdd());
        armorAddition = new ArithmeticNode(new ArithmAdd(), ArmorToAdd());

        Followers._Inst.health.chain.Add(healthAddition);
        Followers._Inst.armor.chain.Add(armorAddition);

        onRecalculate += () =>{ healthAddition.Mutation = HealthToAdd(); };
        onRecalculate += () =>{ armorAddition.Mutation = ArmorToAdd(); };
    }

    float HealthToAdd() => Result * 300;
    float ArmorToAdd() => Result * 100;
}

public class PerseveranceStat : StatMultChain
{
    static public ArithmeticNode armorBonus, damageBonus;

    public float mutation {protected set; get;}

    public PerseveranceStat(float initialValue, float valGrowth, float costGrowth) : base(initialValue, valGrowth, costGrowth)
    {
        mutation = 1;

        armorBonus = new ArithmeticNode(new ArithmMult(), mutation);
        Hero._Inst.armor.chain.Add(armorBonus);

        damageBonus = new ArithmeticNode(new ArithmMult(), mutation);
        Hero._Inst.damage.chain.Add(damageBonus);

        onRecalculate += RecalculateMutation;
        SoftReset.onReset += RecalculateMutation;
        SoftReset.onBossSpawn += RecalculateMutation;

        RecalculateMutation();
    }

    private void RecalculateMutation()
    {
        float log = Mathf.Log(Result + SoftReset.maxStage, 10);
        mutation = 1 + log * log * .05f;

        armorBonus.Mutation = mutation;
        damageBonus.Mutation = mutation;
    }
}






public class Healing : DamageProcessing
{
    Timer timer;
    Unit target;

    public Healing(Unit unit) : base( unit)
    {
        timer = unit.InitHealingTimer();

        unit.timeredActionsList.Add(MainHealingFunction);
    }


    void MainHealingFunction()
    {
        if (timer.Tick())
        {
            if (CanHealFollowers())
            {
                HealFollowers();

                if (AdProgression.SharingLight.isSharing)
                    HealUnit();
            }
            else
                HealUnit();
        }
    }

    bool CanHealFollowers()
    {
        return (unit.followers.Alive || unit.canRessurect) &&
            unit.followers.healthRange._Val < unit.followers.healthRange._Max;
    }

    void HealFollowers()
    {
        target = unit.followers;
        MakeHealing();
    }

    void HealUnit()
    {
        target = unit;
        MakeHealing();
    }
    
    public void MakeHealing()
    {
        var healArgs = new DoHealArgs(unit, unit.healing.Result);
        
        target.TakeHeal(healArgs);
    }
}


public class TakeHeal : DamageProcessing
{
    public TakeHeal(Unit unit) : base( unit)
    {
        unit.takeHealChain.Add(TakeHeal_);    
    }

    void TakeHeal_(DoHealArgs hargs)
    {
        unit.AffectHP(+hargs.heal);
    }
}
