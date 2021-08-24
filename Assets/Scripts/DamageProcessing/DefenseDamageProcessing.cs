using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamageArmor : DamageProcessing
{
    public TakeDamageArmor(Unit unit) : base(unit)
    {
    }

    [TakeDamageOrder(30)] void TakeDamage_Armor(DoDamageArgs dargs)
    {
        if (dargs.isReflected || dargs.isHotHanded || dargs.isDoom || dargs.damage._Val < 1)
        {
            return;
        }

        DecreaseDamage(dargs, unit);
    }

    static public void DecreaseDamage(DoDamageArgs dargs, Unit unit)
    {
        dargs.damage._Val = Mathf.Max(0, dargs.damage._Val - unit.armor.Result );
    }
}

public class TakeDamageHealth : DamageProcessing
{
    public TakeDamageHealth(Unit unit) : base(unit)
    {
    }
    

    [TakeDamageOrder(40)] void TakeDamage_Health(DoDamageArgs dargs)
    {
        if (dargs.damage._Val < 1) return;


        float nonOverkill = Mathf.Min(dargs.damage._Val, unit.healthRange._Val );

        unit.AffectHP(-nonOverkill);

        dargs.damage._Val -= nonOverkill;

        if (dargs.isBloodMadness || dargs.attacker.vampChain.Count == 0) return;
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
        if (dargs.isReflected || dargs.isDoom || dargs.isHotHanded || dargs.damage._Val < 1) return;

        damageReflected = DecreaseDamage(dargs, unit);

        ReflectDamage(dargs, unit, damageReflected);
    }


    public static float DecreaseDamage(DoDamageArgs dargs, Unit unit)
    {
        float
            decreased = dargs.damage._Val * Mathf.Min(unit.reflect.Result, .5f),

            reflected = dargs.damage._Val * unit.reflect.Result;

        dargs.damage._Val -= decreased;

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
    public TakeDamageFollowers(Unit unit) : base(unit)
    {
    }
    
    [TakeDamageOrder(10)] void TakeDamage_Followers(DoDamageArgs dargs)
    {
        if (unit.followers.Alive)
			unit.followers.TakeDamage(dargs);
    }
}
