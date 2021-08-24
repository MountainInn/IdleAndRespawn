using UnityEngine;

public class EatTheWeak : Talent
{
    float mult = .1f;

    public EatTheWeak() : base(Boss._Inst)
    {
        unit.attackChain.Add(0, Eat);

        Followers._Inst.takeDamageChain.Add(0, ConsumeEat);
    }
    public override string updatedDescription => throw new System.NotImplementedException();

    
    void Eat(DoDamageArgs dargs)
    {
        dargs.eatTheWeak = true;
    }

    void ConsumeEat(DoDamageArgs dargs)
    {
        if (dargs.eatTheWeak) dargs.damage._Val *= mult;
    }
}

public class Hatred : Talent
{
    ArithmeticNode critChanceBonus;
    float multPerLevel = .05f;

    public Hatred() : base(Boss._Inst)
    {
        critChanceBonus = new ArithmeticNode(new ArithmAdd(), multPerLevel);

        unit.critChance.chain.Add(critChanceBonus);
    }

    public override string updatedDescription => throw new System.NotImplementedException();
}

public class CurseOfDoom:Talent
{
    Timer timer = new Timer(20);

    ProgressImage doomProgress;

    new Boss unit;

    public CurseOfDoom(ProgressImage progressImage) : base(Boss._Inst)
    {
        this.unit = Boss._Inst;
        unit.timeredActionsList.Add(CurseTick);

        doomProgress=progressImage;
        doomProgress.SetValue( 0);

        SoftReset.onReset += ()=> { timer.Reset(); };
    }

    public override string updatedDescription => throw new System.NotImplementedException();

    void CurseTick()
    {
        if (unit.shieldPutUp)
        {
            if (timer.Tick()) ApplyCurse();
        }
        else
            timer.Reset();

        doomProgress.SetValue(timer.GetRatio());
    }

    void ApplyCurse()
    {
        var heroHealth = Hero._Inst.healthRange._Max;
        var dargs = new DoDamageArgs(unit, heroHealth){ isDoom = true };

        Hero._Inst.TakeDamage(dargs);
    }
}

public class SharpSpikes : Talent
{
    float maxHealthFraction = .05f;

    public SharpSpikes() : base(Boss._Inst)
    {
        unit.target.takeDamageChain.Add(0, TakeSpike);
    }

    public override string updatedDescription => throw new System.NotImplementedException();

    void TakeSpike(DoDamageArgs dargs)
    {
        if (dargs.isReflected)
            dargs.damage._Val += dargs.target.healthRange._Max * maxHealthFraction; 
    }
}

public class CurseOfWeakness : Talent
{
    public CurseOfWeakness() : base(Boss._Inst)
    {
        throw new System.NotImplementedException();
    }

    public override string updatedDescription => throw new System.NotImplementedException();
}
