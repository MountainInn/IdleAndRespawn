using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalHit : DamageProcessing
{
    public CriticalHit(Unit unit) : base(unit)
    {
        unit.attackChain.Add(20, CriticalRoll);
    }

    void CriticalRoll(DoDamageArgs dargs)
    {
        float critChance = unit.critChance.Result;

        float
            critMult = unit.critMult.Result,
            totalCritMult = 1;

        do
        {
            float roll = UnityEngine.Random.value;

            if (roll < critChance)
            {
                totalCritMult += critMult - 1f;
            }

            critChance -= 1f;
        }
        while (critChance > 0f);


        if (totalCritMult > 1f)
        {
            dargs.isCritical = true;

            dargs.damage._Val *= totalCritMult;
        }
    }
}

public class Attack : DamageProcessing
{
    public StatBasedTimer timer;
    

    public Attack(Unit unit) : base( unit)
    {
        timer = unit.InitAttackTimer();

        unit.makeAttack = MakeAttack;

        unit.timeredActionsList.Add(MainAttackFunction);
    }


    void MainAttackFunction()
    {
        if (unit.target != null &&
            timer.Tick()
        )
            MakeAttack();
    }

    
    public void MakeAttack()
    {
        if (unit.attackChain == null) return;

        var attackArgs = unit.attackChain.Invoke(new DoDamageArgs(unit, unit.damage.Result));

        unit.target.TakeDamage(attackArgs);

        unit.onAttack?.Invoke();
    }
}
