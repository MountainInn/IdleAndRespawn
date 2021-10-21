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


    void AwakeReincarnations()
    {
        InitReincarnationMult();
        AddReincarnationMult();

        UpdateReincarnationMult(Hero._Inst.frags);
        Hero.onFragsUpdated += UpdateReincarnationMult;
    }

    void InitReincarnationMult()
    {
        reincarnationDamageMult = ArithmeticNode.CreateMult();
        reincarnationHealthMult = ArithmeticNode.CreateMult();
        reincarnationAttackSpeedMult = ArithmeticNode.CreateMult();
    }


    void AddReincarnationMult()
    {
        damage.chain.Add(reincarnationDamageMult);
        health.chain.Add(reincarnationHealthMult);
        attackSpeed.chain.Add(reincarnationAttackSpeedMult);
    }


    void UpdateReincarnationMult(int frags)
    {
        multOne = 1 + (frags * Mathf.Pow(Mathf.Log10(1 +frags), 2))/2;
        reincarnationDamageMult.Mutation = multOne;
        reincarnationHealthMult.Mutation = multOne;

        multTwo = Mathf.Max(0.6f, (1f - frags * 0.05f));
        reincarnationAttackSpeedMult.Mutation = multTwo;
    }

}
