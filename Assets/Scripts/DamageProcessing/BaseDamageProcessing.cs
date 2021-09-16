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

    protected void Activate()
    {
        if (!CanActivate()) return;
        
        Type thisType = this.GetType();

        Connect();

        foreach (var button in thisType.GetFields(BindingFlags.Instance).OfType<UpgradeButton>() )
        {
            if ( button != null ) button.gameObject.SetActive(true);
        }

    }

    protected virtual void Connect(){}
    protected virtual void Disconnect() {}
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
        SoftReset.onReincarnation += RecalculateMutation;

        RecalculateMutation();
    }

    private void RecalculateMutation()
    {
        float log = Mathf.Log(Result+1, 10)/5;
        mutation = 1 + log;

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
        return
            unit.followers.Alive &&
            unit.followers.healthRange._Val < unit.followers.healthRange._Max
            || unit.canRessurect;
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
        var healArgs = new DoHealArgs(unit, unit.healing.Result){ isHeal = true };

		unit.healingChain.Invoke(healArgs);

		target.TakeHeal(healArgs);
    }
}


public class TakeHeal : DamageProcessing
{
    public TakeHeal(Unit unit) : base( unit)
    {
        unit.takeHealChain.Add(100, TakeHeal_);
    }

    void TakeHeal_(DoHealArgs hargs)
    {
        float missingHP = unit.healthRange._Max - unit.healthRange._Val;

        float nonOverheal = Mathf.Min(missingHP, hargs.heal);

        unit.AffectHP(nonOverheal);

        hargs.heal -= nonOverheal;
    }
}
