using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

public class ArithmeticChain
{
    SortedList<int, ArithmeticNode> chain;

    public Action onRecalculateChain;


    public ArithmeticChain(int startCapacity)
    {
        chain = new SortedList<int, ArithmeticNode>(startCapacity);
    }

    public ArithmeticChain(int startCapacity, float rootValue) : this(startCapacity)
    {
        chain.Add(chain.Count, new ArithmeticNode(rootValue));
    }

    public void Add(ArithmeticNode node) 
    {
        chain.Add(chain.Count, node);

        node.chain = this;

        if (chain.Count > 1)
            RecalculateChain(node);
    }

    public void Add(int index, ArithmeticNode node)
    {
        chain.Add(index, node);

        node.chain = this;

        if (chain.Count > 1)
            RecalculateChain(node);
    }

    public void Remove(ArithmeticNode multiplier)
    {
        int removedIndex = chain.IndexOfValue(multiplier);

        chain.RemoveAt(removedIndex);

        multiplier.chain = null;

        if (chain.Count > 1)
            RecalculateChain(chain[removedIndex]);
    }

    public void RecalculateChain()
    {
        RecalculateChain(0);
    }

    public void RecalculateChain(ArithmeticNode startingFrom)
    {
        var changedIndex = chain.IndexOfValue(startingFrom);

        RecalculateChain(changedIndex);
    }

    public void RecalculateChain(int changedIndex)
    {
        if (changedIndex < 1) changedIndex = 1;


        ArithmeticNode prev, next;

        
        for (int i = changedIndex; i < chain.Count; i++)
        {
            prev = chain.Values[i-1];
            next = chain.Values[i];

            next.Mutate(prev.Result);
        }

        onRecalculateChain?.Invoke();
    }


    public float Result => Last.Result;

    ArithmeticNode Last => chain.Values[chain.Count - 1];

    new public string ToString()
    {
        string output = string.Empty;

        foreach (var item in chain)
        {
            output += $"{item.Key:0000} }} {item.Value.ToString()}\n";  
        }

        return output;
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class ArithmeticNode
{
    public ArithmeticChain chain;

    Arithm arithm;

    [JsonPropertyAttribute]
    float
        mutation,
        result;

    public Action onMutationUpdated;
        
    [OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext context)
    {
        onMutationUpdated?.Invoke();
        chain.RecalculateChain(this);
    }

    public float Mutation
    {
        get => mutation;

        set {
            mutation = value;
                
            onMutationUpdated?.Invoke();;
            
            chain.RecalculateChain(this);
        }
    }

    public float Result
    {
        get => result;
    }


    public void Mutate(float previousVal)
    {
        result = arithm.Mutate(previousVal, mutation);
    }


    public ArithmeticNode(Arithm arithm, float mutation)
    {
        this.mutation = mutation;

        this.arithm = arithm;
    }

    public ArithmeticNode(float rootVal)
    {
        this.result = rootVal;
    }

    new public string ToString()
    {
        return $"{( arithm == null ? "root" : arithm.ToString() )} {mutation} = {result}";
    }

    static public ArithmeticNode CreateRoot(float initialValue = 1) => new ArithmeticNode(initialValue);

    static public ArithmeticNode CreateMult(float mutation = 1) => new ArithmeticNode(new ArithmMult(), mutation);

    static public ArithmeticNode CreateAdd() => new ArithmeticNode(new ArithmAdd(), 0);

    static public ArithmeticNode CreateLimit(float limit) => new ArithmeticNode(new ArithmLimit(), limit);
}

abstract public class Arithm
{
    abstract public float Mutate(float previousVal, float mutation);

    new abstract public string ToString();
}


public class ArithmMult : Arithm
{
    public override float Mutate(float previousVal, float mutation)
    {
        return previousVal * mutation;
    }

    public override string ToString() => "*";
}

public class ArithmAdd : Arithm
{
    public override float Mutate(float previousVal, float mutation)
    {
        return previousVal + mutation;
    }

    public override string ToString() => "+";
}


public class ArithmLimit : Arithm
{
    public override float Mutate(float previousVal, float mutation)
    {
        if (mutation <= 0)
            return Mathf.Max(previousVal, mutation);

        else
            return Mathf.Min(previousVal, mutation);
    }
    
    public override string ToString() => "Limit";

}
