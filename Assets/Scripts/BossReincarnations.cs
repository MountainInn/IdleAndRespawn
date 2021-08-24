using UnityEngine;
using Newtonsoft.Json;


public partial class Boss
{

    [JsonPropertyAttribute]
    float multOne, multTwo;

    ArithmeticNode
        reincarnationDamageMult,
        reincarnationHealthMult,
        reincarnationAttackSpeedMult;


    void InitReincarnationMult()
    {
        reincarnationDamageMult = new ArithmeticNode(new ArithmMult(), multOne);
        reincarnationHealthMult = new ArithmeticNode(new ArithmMult(), multOne);
        reincarnationAttackSpeedMult = new ArithmeticNode(new ArithmMult(), multTwo);
    }


    void AddReincarnationMult()
    {
        damage.chain.Add(reincarnationDamageMult);
        health.chain.Add(reincarnationHealthMult);
        attackSpeed.chain.Add(reincarnationAttackSpeedMult);
    }


    void UpdateReincarnationMult()
    {
        multOne = Mathf.Max(1, PlayerStats._Inst.bossKilled * 10);
        multTwo = Mathf.Max(0.6f, (1f - PlayerStats._Inst.bossKilled * 0.05f));

        reincarnationDamageMult.Mutation = multOne;
        reincarnationHealthMult.Mutation = multOne;
        reincarnationAttackSpeedMult.Mutation = multTwo;
    }


    void Sub_UpdateReincarnationMult_OnDeath()
    {
        onDeathChain.Add((unit)=>UpdateReincarnationMult());
    }
}
