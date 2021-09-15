using UnityEngine;
using Newtonsoft.Json;



[JsonObjectAttribute(MemberSerialization.OptIn)]
abstract public class SoulGrade : Vendible, ILifted
{
    public string name, description;
    protected Unit unit;
    protected uint _level = 0;


#region Vendible

    override protected void OnBought()
    {
        Level++;
    }

    [JsonPropertyAttribute]
    protected uint Level
    {
        get => _level;
        set
        {
            _level = value;
            OnLevelup();
        }
    }
    abstract protected void OnLevelup();
#endregion

#region ILifted

    bool ILifted.isLifted { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    int ILifted.floor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    void ILifted.OnDropped() {}

    public void OnLifted()
    {

    }
#endregion
}


/// Когда босс кастует - каждая атака героя дает стак MageBane
/// и наносит доп урон, основанный на этих стаках
[JsonObjectAttribute(MemberSerialization.OptIn)]
public class MageBane : SoulGrade
{
    float bonusDamage = 0;
    uint _stacks;
    [JsonPropertyAttribute]
    uint Stacks
    {
        get => _stacks;
        set {
            _stacks = value;
            RecalcBonusDamage();
        }
    }
    public MageBane()
    {
        unit = Hero._Inst;

        name = "MageBane";
        description = "While the Boss casts spell each Hero's attack gives stack of Mage Bane\nand deals extra damage based on these stacks";
    }
    void UseMageBaneStacks(DoDamageArgs dargs)
    {
        if (dargs.target.isCasting)
        {
            dargs.damage._Val += bonusDamage;
            Stacks++;
        }
    }
    override protected void OnLevelup()
    {
        RecalcBonusDamage();
    }

    void RecalcBonusDamage()
    {
        bonusDamage = Mathf.Log(1 + Stacks, 3) * Level/20;
    }

    new public void OnLifted()
    {
        base.OnLifted();
        unit.attackChain.Add(300, UseMageBaneStacks);
    }
}
