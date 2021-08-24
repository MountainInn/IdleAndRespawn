using Newtonsoft.Json;
using System;
using UnityEngine;


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
    public ArithmeticNode limit;

    float limitValue;

    public bool isLimitReached => Result == limitValue;

    public bool isPercentage;

    [JsonPropertyAttribute]
    public float cost;
    public float
        valGrowth,
        costGrowth;

    
    public Action onLevelUp, onRecalculate;

    public StatMultChain(float initialValue, float valGrowth, float costGrowth)
    {
        chain = new ArithmeticChain(4);

        basevalue = new ArithmeticNode(initialValue);
        growth = new ArithmeticNode(new ArithmAdd(), 0);

        chain.Add(basevalue);
        chain.Add(growth);


        this.valGrowth = valGrowth;
        this.costGrowth = costGrowth;

        level = 1;
        RecalculateStats();
    }

    public void SetLimit(float limitValue)
    {
        if (limit == null)
        {
            limit = ArithmeticNode.CreateLimit(limitValue);

            chain.Add(int.MaxValue, limit);
        }
        else
        {
            limit.Mutation = limitValue;
        }

        this.limitValue = limitValue;
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


    public float GetValForNextLevel(int levelsForward = 1) => basevalue.Val + CalcGrowthValue(level + levelsForward);

    public float GetCostForNextLevel(int levelsForward = 1) => CalcCost(level + levelsForward);

    public float CalcCost(int level) => level * level * 0.1f * costGrowth;

    float CalcGrowthValue(int level) => level * valGrowth;

    public int maxAffordableLevel;
    public float maxAffordableCost;
    public int CalculateMaxAffordableLevel(int targetLevel, out bool canAfford)
    {
        int level = 1;
        maxAffordableCost = 0;

        do
        {
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
