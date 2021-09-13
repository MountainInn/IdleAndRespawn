using Newtonsoft.Json;
using System;
using UnityEngine;
using System.Runtime.Serialization;


public class Order : Attribute
{
    public int order;
    public Order(int order){ this.order = order; }
}

public sealed class AttackOrder : Order
{
    public AttackOrder(int order) : base(order){}
}
public sealed class TakeDamageOrder : Order
{
    public TakeDamageOrder(int order) : base(order){}
}
public sealed class VampOrder : Order
{
    public VampOrder(int order) : base(order){}
}
public sealed class HealOrder : Order
{
    public HealOrder(int order) : base(order){}
}
public sealed class TakeHealOrder : Order
{
    public TakeHealOrder(int order) : base(order){}
}
public sealed class StatInitOrder : Order 
{
    public StatInitOrder(int order):base(order){}
}
public sealed class TimeredActionsUNOrdered : Order 
{
    public TimeredActionsUNOrdered():base(0){}
}
public sealed class OnDeathOrder : Order 
{
    public OnDeathOrder(int order):base(order){}
}

[SaveClass]
public class Stat
{
    public int level;

    [SaveField]
    public float
        val,
        cost,
        baseVal,
        valGrowth,
        costGrowth;

    public Action onLevelUp, onRecalculate;

    public Stat(float initialValue, float valGrowth, float costGrowth)
    {
        this.baseVal = this.val = initialValue;
        this.valGrowth = valGrowth;
        this.costGrowth = costGrowth;

        level = 1;
        CalculateStats();
    }


    public void LevelUp(Currency currency, int levelIncrease = 1)
    {
        if ( currency.Buy(GetCostForNextLevel(levelIncrease)) )
        {
            level += levelIncrease;
            CalculateStats();
            onLevelUp?.Invoke();
        }
    }

    public void CalculateStats()
    {
        val = CalcValue(level);
        cost = CalcCost(level);

        onRecalculate?.Invoke();
    }


    public float GetValForNextLevel(int levelsForward = 1) => CalcValue(level + levelsForward);

    float CalcValue(int level) => baseVal + level * valGrowth;


    public float GetCostForNextLevel(int levelsForward = 1) => CalcCost(level + levelsForward);

    float CalcCost(int level) => level * costGrowth;

    public void Multiply(float mult)
    {
        val *= mult;
        valGrowth *= mult;
    }
    public void Divide(float mult)
    {
        val /= mult;
        valGrowth /= mult;
    }


    static public implicit operator float(Stat upg) => upg.val;
}



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
    public ArithmeticNode limitGrowth;

    public bool isLimitReached => limitGrowth.Result >= limitGrowth.Mutation;

    public bool
        isPercentage,
        isLimited;

    [JsonPropertyAttribute]
    public float cost;
    public float
        valGrowth,
        costGrowth;


    public Action onLevelUp, onRecalculate;

    public StatMultChain(float initialValue, float valGrowth, float costGrowth, float limitVal)
        :this(initialValue, valGrowth, costGrowth)
    {
        limitGrowth.Mutation = limitVal;
        isLimited = true;
    }

    public StatMultChain(float initialValue, float valGrowth, float costGrowth)
    {
        chain = new ArithmeticChain(6);

        chain.Add(basevalue = ArithmeticNode.CreateRoot(initialValue));
        chain.Add(growth = ArithmeticNode.CreateAdd());
        chain.Add(limitGrowth = ArithmeticNode.CreateLimit(float.MaxValue));


        this.valGrowth = valGrowth;
        this.costGrowth = costGrowth;

        level = 0;
        RecalculateStats();
    }



    public void LevelUp(Currency currency, int levelIncrease = 1)
    {
        if ( currency.Buy(maxAffordableCost) )
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

    public float CalcCost(int level) => costGrowth + level * Mathf.Log(level, 10) * 0.1f * costGrowth;

    float CalcGrowthValue(int level) => level * valGrowth;

    public int maxAffordableLevel;
    public float maxAffordableCost;
    public float CalculateMaxAffordableLevel_2(int targetLevel, out bool canAfford)
    {
        Tuple<int, float>
            prev = Tuple.Create(1, GetCostForNextLevel(1)),
            cur = Tuple.Create(1, GetCostForNextLevel(1));

        do
        {
            canAfford = Vault.expirience.CanAfford(cur.Item2);

            if (!canAfford) break;
            else
            {
                prev = cur;

                int nextLevel = cur.Item1 + 1;
                cur = Tuple.Create(nextLevel, cur.Item2 + GetCostForNextLevel(nextLevel));
            }
        }
        while (cur.Item1 < targetLevel);

        maxAffordableCost = prev.Item2;

        canAfford = Vault.expirience.CanAfford(maxAffordableCost);

        return maxAffordableLevel = Mathf.Clamp(prev.Item1, 0, targetLevel);
    }
    public int CalculateMaxAffordableLevel(int targetLevel, out bool canAfford)
    {
        int level = 0;
        maxAffordableCost = 0;

        do
        {
            level++;

            float newCost = GetCostForNextLevel(level);

            maxAffordableCost += newCost;

            canAfford = level > 0 && maxAffordableCost > 0 && Vault.expirience.CanAfford(maxAffordableCost);

            if (!canAfford) 
            {
                maxAffordableCost -= newCost;
                level--;

                canAfford = level > 0 && maxAffordableCost > 0 && Vault.expirience.CanAfford(maxAffordableCost);
                return maxAffordableLevel = Mathf.Clamp(level, 0, targetLevel);
            }
            else
            {
                level++;
            }
        }
        while (level < targetLevel);

        return maxAffordableLevel = Mathf.Clamp(level, 0, targetLevel);
    }

    static public implicit operator float(StatMultChain stat) => stat.Result;
}
