using UnityEngine;
using System;

public partial class Unit
{
    public ActionChain<DoDamageArgs> takeDamageChain;
    public Action<DoDamageArgs> onTakeDamage;

    public void TakeDamage(DoDamageArgs damageArgs)
    {
        takeDamageChain.Invoke(damageArgs);

        
        onTakeDamage?.Invoke(damageArgs);


        if (damageArgs.attacker.vampirism != null)
        {
            float vampHeal = damageArgs.attacker.vampirism.Result * damageArgs.damage._Val;

            if (vampHeal < 1) return;

            DoHealArgs vampArgs = new DoHealArgs(damageArgs.attacker, vampHeal);

            damageArgs.attacker.TakeHeal(vampArgs);
        }
    }
    




}
