using UnityEngine;
using System;

public partial class Unit
{
    public ActionChain<DoDamageArgs> takeDamageChain;
    public Action<DoDamageArgs> onTakeDamage;

    public void TakeDamage(DoDamageArgs damageArgs)
    {
        takeDamageChain.Invoke(damageArgs);
    }
    




}
