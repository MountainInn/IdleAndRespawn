using UnityEngine;
using System;

public partial class Unit
{
    public ActionChain<DoHealArgs> vampChain;
    internal float blockChance = 0f;
    internal float blockMult = 1f;

    public void Vamp(DoDamageArgs damageArgs)
    {
        float vampHeal = damageArgs.attacker.vampirism.Result * damageArgs.damage._Val;

        vampHeal = Mathf.Clamp(vampHeal, 1f, healthRange._Max * .1f);

        var vampArgs = new DoHealArgs(this, vampHeal){ isVamp = true };

        vampChain.Invoke(vampArgs);

        TakeHeal(vampArgs);
    }

}
