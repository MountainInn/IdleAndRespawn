using Newtonsoft.Json;
using System;
using UnityEngine;
using System.Runtime.Serialization;


[JsonObjectAttribute(MemberSerialization.OptIn)]
public class StatMultChain
{
    [JsonPropertyAttribute]
    public int level;

    public ArithmeticChain chain;

    public float Result => chain.Result;

    [JsonPropertyAttribute]
    public ArithmeticNode basevalue, growth;

    [JsonPropertyAttribute]
    public ArithmeticNode growthLimit;

    public bool isLimitReached => growthLimit.Result >= growthLimit.Mutation;

    public bool
        isPercentage,
        isLimited;

    public float cost;
    public float
        valGrowth,
        costGrowth;

    public StatView view;

    public Action onLevelUp, onRecalculate, onLoaded;

    [OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext sc)
    {
        if (isLimitReached)
            view.SwitchState(view.limitReachedState);

        onLoaded?.Invoke();;
    }


    public StatMultChain(float initialValue, float valGrowth, float costGrowth, float limitVal)
        : this(initialValue, valGrowth, costGrowth)
    {
        growthLimit.Mutation = limitVal;
        isLimited = true;
    }

    public StatMultChain(float initialValue, float valGrowth, float costGrowth)
    {
        chain = new ArithmeticChain(6);

        chain.Add(basevalue = ArithmeticNode.CreateRoot(initialValue));
        chain.Add(growth = ArithmeticNode.CreateAdd());
        chain.Add(growthLimit = ArithmeticNode.CreateLimit(float.MaxValue));


        this.valGrowth = valGrowth;
        this.costGrowth = costGrowth;

        level = 0;
        RecalculateStats();
    }



    public void LevelUp(Currency currency, int levelIncrease = 1)
    {
        if (currency.Buy(maxAffordableCost))
        {
            level += levelIncrease;
            RecalculateStats();
            onLevelUp?.Invoke();
        }
    }

    public void RecalculateStats()
    {
        growth.Mutation = CalcGrowthValue(level);

        cost = CalcCost(level);

        onRecalculate?.Invoke();
    }


    public float GetValForNextLevel(int levelsForward = 1) => basevalue.Result + CalcGrowthValue(level + levelsForward);

    public float GetCostForNextLevel(int levelsForward = 1) => CalcCost(level + levelsForward);

    public float CalcCost(int level) => costGrowth + level * Mathf.Log(level + 2, 10) * 0.125f * costGrowth;

    float CalcGrowthValue(int level) => level * valGrowth;

    public int maxAffordableLevel;
    public float maxAffordableCost;

    public int CalculateMaxAffordableLevel2(int targetLevel, out bool canAfford)
    {
        int untilLimit = GetLevelLimitFromValueLimit();

        int level = 1;
        float acum = GetCostForNextLevel(level);
        canAfford = Vault.Expirience.CanAfford(acum);

        int nextLevel;
        float nextAcum = acum;
        bool nextCanAfford;

        do
        {
            nextLevel = level + 1;

            if (nextLevel > targetLevel ||
                nextLevel > untilLimit
            )
                break;

            nextAcum += GetCostForNextLevel(nextLevel);
            nextCanAfford = Vault.Expirience.CanAfford(nextAcum);

            if (nextCanAfford)
            {
                level = nextLevel;
                acum = nextAcum;
                canAfford = nextCanAfford;
            }
            else
                break;
        }
        while (level <= targetLevel);

        maxAffordableCost = acum;
        return maxAffordableLevel = level;
    }


    int GetLevelLimitFromValueLimit()
    {
        if (growthLimit.Mutation == float.MaxValue) return int.MaxValue;
        else
            return (int)( ( growthLimit.Mutation - basevalue.Result) / valGrowth ) - level +1;
    }

    static public implicit operator float(StatMultChain stat) => stat.Result;
}
