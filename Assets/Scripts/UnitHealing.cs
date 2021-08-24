using System;

public partial class Unit
{
    public ActionChain<DoHealArgs> healingChain;
    public ActionChain<DoHealArgs> takeHealChain;
    public Action<DoHealArgs> onTakeHeal;

    public bool canRessurect;


    public void TakeHeal(DoHealArgs healArgs)
    {
        takeHealChain.Invoke(healArgs);

        onTakeHeal?.Invoke(healArgs);
    }
}


