using UnityEngine;
using Newtonsoft.Json;


public partial class Boss
{

    float multOne, multTwo;

    [JsonPropertyAttribute]
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
        reincarnationDamageMult.Mutation = multOne;
        reincarnationHealthMult.Mutation = multOne;

        multTwo = Mathf.Max(0.6f, (1f - PlayerStats._Inst.bossKilled * 0.05f));
        reincarnationAttackSpeedMult.Mutation = multTwo;
    }

}
