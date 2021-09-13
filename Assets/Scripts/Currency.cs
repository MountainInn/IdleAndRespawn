using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Currency
{
    [JsonPropertyAttribute]
    float val;

    [OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext context)
    {
        onChanged?.Invoke();;
    }


    public event Action<float> onChanged_Amount;
    public event Action onChanged;

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

        onChanged_Amount?.Invoke(earnings);
        onChanged?.Invoke();
    }

    public void Spend(float spend)
    {
        _Val -= spend;

        onChanged_Amount?.Invoke(-spend);
        onChanged?.Invoke();
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
