using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TakeDamageBarrier : DamageProcessing
{
    public TakeDamageBarrier() : base(Hero._Inst)
    {
        unit.takeDamageChain.Add(5, TakeDamage_Barrier);
    }

    void TakeDamage_Barrier(DoDamageArgs dargs)
    {
        if (dargs.damage._Val < 1) return;

        float nonOverkill = Mathf.Min(dargs.damage._Val, unit.barrierRange._Val );

        unit.barrierRange._Val -= nonOverkill;

        dargs.damage._Val -= nonOverkill;
    }
}

public class TakeDamageArmor : DamageProcessing
{
    public TakeDamageArmor(Unit unit) : base(unit)
    {
        unit.takeDamageChain.Add(30, TakeDamage_Armor);
    }

    void TakeDamage_Armor(DoDamageArgs dargs)
    {
        if (!dargs.IsSimpleAttack ||  dargs.damage._Val < 1) return;

        DecreaseDamage(dargs, unit);
    }

    static public void DecreaseDamage(DoDamageArgs dargs, Unit unit)
    {
        dargs.damage._Val = Mathf.Max(0, dargs.damage._Val - unit.armor.Result * BlockMult(unit));
    }

    static public float BlockMult(Unit unit)
    {
        if (unit.hasBlock && UnityEngine.Random.value < unit.blockChance)
        {
            return unit.blockMult;
        }
        else return 1f;
    }
}

public class TakeDamageHealth : DamageProcessing
{
    public TakeDamageHealth(Unit unit) : base(unit)
    {
        unit.takeDamageChain.Add(40, TakeDamage_Health);
    }
    

    void TakeDamage_Health(DoDamageArgs dargs)
    {
        if (dargs.damage._Val < 1) return;


        float
            fullDamage = dargs.damage._Val,
            nonOverkill = Mathf.Min(dargs.damage._Val, unit.healthRange._Val);

        unit.AffectHP(-nonOverkill);

        /// Заспавнить текст урона
        dargs.damage._Val = nonOverkill;
        unit.onTakeDamage.Invoke(dargs);


        /// Вампиризм
        if (dargs.IsSimpleAttack && dargs.attacker.vampirism != null)
        {
            dargs.attacker.Vamp(dargs);
        }

        /// Убрать часть урона, поглощенную здоровьем
        dargs.damage._Val = fullDamage - nonOverkill;
    }
}
public class TakeDamageReflect : DamageProcessing
{
    public float damageReflected;

    public TakeDamageReflect(Unit unit) : base(unit)
    {
        unit.takeDamageReflect = this;
    }


    [TakeDamageOrder(20)] void TakeDamage_Reflect(DoDamageArgs dargs)
    {
        if (!dargs.IsSimpleAttack || dargs.damage._Val < 1) return;

        damageReflected = DecreaseDamage(dargs, unit);

        ReflectDamage(dargs, unit, damageReflected);
    }


    public static float DecreaseDamage(DoDamageArgs dargs, Unit unit)
    {
        float reflected = unit.reflect.Result;

        dargs.damage._Val = dargs.damage._Val = Mathf.Max(0, dargs.damage._Val - reflected);

        return reflected;
    }


    public static void ReflectDamage(DoDamageArgs dargs, Unit unit, float reflected)
    {
        DoDamageArgs reflectDamageArgs = DoDamageArgs.CreateReflected(unit, reflected);
        
        dargs.attacker.TakeDamage(reflectDamageArgs);
    }
}
public class TakeDamageFollowers : DamageProcessing
{
    Followers followers;

    public TakeDamageFollowers(Unit unit) : base(unit)
    {
        followers = unit.followers as Followers;
        unit.takeDamageChain.Add(10, TakeDamage_Followers);
    }
    
    void TakeDamage_Followers(DoDamageArgs dargs)
    {
        if (followers.Alive && !dargs.isHotHanded)
        {
            followers.TakeDamage(dargs);
        }
    }
}
