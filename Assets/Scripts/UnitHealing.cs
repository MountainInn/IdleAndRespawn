using System;
using Newtonsoft.Json;

public partial class Unit
{
    public ActionChain<DoHealArgs> healingChain;
    public ActionChain<DoHealArgs> takeHealChain;
    public Action<DoHealArgs> onTakeHeal;

    [JsonPropertyAttribute]
    public bool canRessurect;


    public void TakeHeal(DoHealArgs healArgs)
    {
        takeHealChain.Invoke(healArgs);
    }
}


