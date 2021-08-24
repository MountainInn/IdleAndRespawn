using System;
using UnityEngine;

public class Currency
{
    float val;

    public event Action<float> onChanged;

    public float _Val
    {
        get => val;

        private set 
        {
            val = Mathf.Clamp( value, 0, float.MaxValue );
        }
    }
    
    public Currency(float initialValue)
    {
        val = initialValue;
    }


    public void Earn(float earnings)
    {
        _Val += earnings;

        onChanged?.Invoke(earnings);
    }

    public void Spend(float spend)
    {
        _Val -= spend;

        onChanged?.Invoke(-spend);
    }

    public bool Buy(float cost)
    {
        if (CanAfford(cost))
        {
            Spend(cost);

            return true;
        }
        return false;
    }

    public bool CanAfford(float cost)
    {
        return (_Val >= cost);
    }


    static public implicit operator float(Currency currency) => currency._Val;
}
