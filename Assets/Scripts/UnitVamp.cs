
using System;

public partial class Unit
{
    public ActionChain<DoHealArgs> vampChain;

    public void Vamp(DoDamageArgs damageArgs)
    {
        vampChain.Invoke(new DoHealArgs(this, damageArgs.damage._Val * vampirism));

        
    }

}
