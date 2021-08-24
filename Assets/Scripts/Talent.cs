using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;


[JsonObjectAttribute(MemberSerialization.OptIn)]
abstract public class Talent : DamageProcessing
{
    TalentView view;

    public string
        name = "*PLACEHOLDER*",
        description = "*PLACEHOLDER*";

    abstract public string updatedDescription {get;}

    public float cost;

    [JsonPropertyAttribute] public bool isDiscovered, isBought;

    [System.Runtime.Serialization.OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext sc)
    {
        if (isBought)
        {
            new TalentView.BoughtState().Setup(view);

            Activate();
        }
    }

    protected Talent(Unit unit) :base(unit)
    {
        FindView();
    }

    protected void InitializeViewValues(string name, string description, float cost)
    {
        this.name = name;
        this.description = description;
        this.cost = cost;
    }


    public void Discover()
    {
        if (!isDiscovered)
        {
            isDiscovered = true;

            new TalentView.DiscoveredState().Setup(view);
        }
    }

    public void Buy(Currency currency)
    {
        if (!isBought && currency.Buy(cost))
        {
            isBought = true;

            Activate();

            new TalentView.BoughtState().Setup(view);

            GameLogger.Logg("talent", $"{name} Activated");
        }
    }

    public bool CanAfford()
    {
        return Vault.talentPoints.CanAfford(cost);
    }

    override public bool CanActivate()
    {
        return isBought;
    }


    void FindView()
    {
        string name = this.GetType().Name + "_Talview";

        var view = TalentView.instances.FirstOrDefault(tv => tv.gameObject.name == name);

        if (view == null || view == default)
        {
            Debug.LogWarning($"Can't find talview named {name}");
            return;
        }

        ConnectToView(view);
    }

    void ConnectToView(TalentView view)
    {
        this.view = view;

        view.ConnectToTalent(this);
    }


}

public class FullBlood : Talent
{
    Range hp;
    float fullBonus = .6f;
    float currentBonus;

    public FullBlood(Unit unit) : base(unit)
    {
        this.hp = unit.healthRange;
        
        base.InitializeViewValues("Full Blood",
                                  $"Increases damage based on remaining health.\nFull bonus at full health.",
                                  50);
    }

    public override string updatedDescription { get => $"\n({currentBonus:P0}%)"; }

    [AttackOrder(15)] void Attack_FullBlood(DoDamageArgs damageArgs)
    {
        damageArgs.damage._Val *= 1f + currentBonus;
    }


    protected override void SubclassActivation()
    {
        UpdateBonus(unit.healthRange.GetRatio());
        unit.healthRange.onRatioChanged += UpdateBonus;
    }

    void UpdateBonus(float healthRatio)
    {
        currentBonus = fullBonus * healthRatio;
    }
}

public class BloodMadness : Talent
{
    [JsonProperty]
    float bonusDamage;
    [JsonProperty]
    float damageToFollowers;

    public BloodMadness(Unit unit) :base(unit)
    {
        base.InitializeViewValues("Blood Madness",
                                  $"When you leech health you also inflict as much damage to your followers.\n"+
                                $"You inflict bonus damage with each attack base on damage done to followers this way.\n"+
                                $"This damage cannot be critical.",
                                  150);
    }

    public override string updatedDescription { get => $"\n({bonusDamage})"; }

    protected override void SubclassActivation()
    {
        Hero._Inst.followers.takeDamageChain.Add(0, HarvestBonus);
    }

    [AttackOrder(100)] void BloodMadness_AddDamage(DoDamageArgs damageArgs)
    {
        damageArgs.damage._Val += bonusDamage;
    }

    [VampOrder(20)] void BloodMad_DamageFollowers(DoHealArgs healArgs)
    {
        if (Hero._Inst.followers.Alive)
            Hero._Inst.followers.TakeDamage(

                new DoDamageArgs(healArgs.healer, healArgs.heal){isBloodMadness = true});
    }

    void HarvestBonus(DoDamageArgs damageArgs)
    {
        if (damageArgs.isBloodMadness) 
        {
            damageToFollowers += damageArgs.damage._Val;
            bonusDamage = DamageBonus();
        }
    }

    float DamageBonus()
    {
        float log = Mathf.Log(damageToFollowers + 1, 2);
        return (log*log)/10;
    }
}
    

public class HotHand : Talent
{
    float
        critDamageLimitVal = 2.5f,
        selfDamageMult = .15f;

    ArithmeticNode critDamageLimit ;


    public HotHand(Unit unit) : base(unit)
    {
        critDamageLimit = unit.critMult.limit;

        base.InitializeViewValues("Hot Hand",
                                  $"Critical damage increased from 150% to {critDamageLimitVal:P0}, but you take 15% of it.",
                                  200);

    }
    public override string updatedDescription { get => ""; }

    [AttackOrder(30)] void Hothanded_DamageSelf(DoDamageArgs damageArgs)
    {
        if (damageArgs.isCritical)
        {
            var dargsSelfDamage = new DoDamageArgs(unit, damageArgs.damage._Val * selfDamageMult)
            {
                isHotHanded = true
            };

            unit.TakeDamage(dargsSelfDamage);
        }
    }

    protected override void SubclassActivation()
    {
        critDamageLimit.Mutation = critDamageLimitVal;
    }

    protected override void SubclassDeactivation()
    {
        critDamageLimit.Mutation = 2.00f;
    }

}

public class LastStand : Talent
{
    bool isActive;
    float
        mult = 1.5f,
        healthThreshold = .5f;

    ArithmeticNode armorBonus;

    public LastStand(Unit unit) : base(unit)
    {
        armorBonus = new ArithmeticNode(new ArithmMult(), 1);

        unit.armor.chain.Add(armorBonus);

        base.InitializeViewValues("Last Stand",
                                $"You have {mult - 1f:P0} more armor while below {healthThreshold:P0} health.",
        100);

    }

    public override string updatedDescription { get => ""; }
    
    [TakeDamageOrder(45)] void Laststand_ActivateBonus(DoDamageArgs damageArgs)
    {
        if (!isActive && unit.healthRange.GetRatio() < healthThreshold)
        {
            armorBonus.Mutation = mult;
            isActive = true;
        }
        else if(isActive && unit.healthRange.GetRatio() >= healthThreshold)
        {
            armorBonus.Mutation = 1;
            isActive = false;
        }
    }

}


public class FindWeakness : Talent
{
    private float critChanceStack = .01f;
    private float totalCritChanceBonus;

    ArithmeticNode critBonus;


    public FindWeakness(Unit unit) : base(unit)
    {
        critBonus = new ArithmeticNode(new ArithmAdd(), 0);

        unit.critChance.chain.Add(critBonus);

        base.InitializeViewValues("Find Weakness",
                                  $"Critical chance growth by 1% for each non-critical attack.\nResets on crit.'",
                                  100);

    }

    public override string updatedDescription { get => $"({totalCritChanceBonus}%)"; }

    [AttackOrder(22)] void Stack_FindWeakness(DoDamageArgs dargs)
    {
        if (!dargs.isCritical)
        {
            totalCritChanceBonus += critChanceStack;
        }
        else
        {
            totalCritChanceBonus = 0f;
        }

        critBonus.Mutation = totalCritChanceBonus;
    }
}

public class DoubleReflect : Talent
{
    float mult = 2f;

    ArithmeticNode reflectMult;
    
    public DoubleReflect(Unit unit) : base(unit)
    {
        reflectMult = ArithmeticNode.CreateMult();

        unit.reflect.chain.Add(reflectMult);

        base.InitializeViewValues("Double Reflect",
                                  $"You deal more reflected damage.",
                                  200);

    }

    public override string updatedDescription { get => $"({mult:P0})"; }

    protected override void SubclassActivation()
    {
        reflectMult.Mutation = mult;
    }

    protected override void SubclassDeactivation()
    {
        reflectMult.Mutation = 1;
    }
}

public class DoubleJudgement : Talent
{
    public Timer timer;

    public int judgementsCount;


    public DoubleJudgement(Unit unit) : base( unit)
    {
        timer = new Timer(2);

        base.InitializeViewValues("Double Judgement",
                                  $"Your reflect strikes second time after short delay.",
                                  100);

    }

    public override string updatedDescription { get => ""; }

    protected override void SubclassActivation()
    {
        unit.target.takeDamageChain.Add(41, OnEnemyTookReflectedDamage);
    }

    void OnEnemyTookReflectedDamage(DoDamageArgs dargs)
    {
        if (dargs.isReflected) judgementsCount++;
    }

    [TimeredActionsUNOrdered] void Judgement()
    {
        if (judgementsCount > 0 && timer.Tick())
        {
            judgementsCount--;
            
            DoDamageArgs judgement = DoDamageArgs.CreateReflected(unit, unit.takeDamageReflect.damageReflected);

            unit.target.TakeDamage(judgement);
        }
    }
}

public class MassReflect : Talent
{
    public MassReflect(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Mass Reflect", $"Hero's reflect now works with Followers", 1000);
    }

    public override string updatedDescription => "";


    protected override void SubclassActivation()
    {
        Followers._Inst.reflect = Hero._Inst.reflect;

        Phases.allActiveDamageProcessings.Add(new TakeDamageReflect(Followers._Inst));
    }
}

public class Interruption : Talent
{
    float interrupted;
    
    public Interruption(Hero hero) : base(hero) 
    {
        base.InitializeViewValues("Interruption",
                                  $"You take half of the damage that your followers would take.",
                                  100);

    }
    public override string updatedDescription { get => ""; }


    [TakeDamageOrder(9)]
    public void Interrupt(DoDamageArgs damageArgs)
    {
        if (damageArgs.isReflected) return;

        interrupted = damageArgs.damage._Val * .5f;

        damageArgs.damage._Val -= interrupted;
    }

    [TakeDamageOrder(19)]
    void ConsumeInterruptedDamage(DoDamageArgs dargs)
    {
        dargs.damage._Val += interrupted;
    }
}


public class CoordinatedActions : Talent
{
    float mult = 3f;

    ArithmeticNode attackspeedMult;

    public CoordinatedActions(Unit unit) : base(unit)
    {
        attackspeedMult = ArithmeticNode.CreateMult();

        unit.attackSpeed.chain.Add(attackspeedMult);

        base.InitializeViewValues("Coordinated Actions",
                                  $"Followers' have more attack speed.",
                                  100);

    }

    public override string updatedDescription { get => "\n({mult:P0})"; }

    protected override void SubclassActivation()
    {
        attackspeedMult.Mutation = mult;
    }

    protected override void SubclassDeactivation()
    {
        attackspeedMult.Mutation = 1;
    }

}

public class Diversion : Talent
{
    float fraction = 0.05f;
    float diversionDamage;

    public Diversion(Unit unit ) : base(unit )
    {
        base.InitializeViewValues("Diversion",
                                  $"Tiny fraction of the damage ignores boss's defences.",
                                  200);

    }
    public override string updatedDescription { get => "\n({fraction}%)"; }

    [AttackOrder(-100)] void SaveDiversionDamage(DoDamageArgs dargs)
    {
        diversionDamage = dargs.damage._Val * fraction;
    }


    [AttackOrder(41)] void DoDiversionDamage(DoDamageArgs dargs)
    {
        diversionDamage = Mathf.Min(diversionDamage, dargs.target.healthRange._Val);

        dargs.target.AffectHP(-diversionDamage);
    }
}


[JsonObjectAttribute]
public class StaminaTraining : Talent
{
    Timer recalculationTimer;

    ArithmeticNode staminaTrainingMult;

    [System.Runtime.Serialization.OnDeserializedAttribute]
    public void OnDeserialize(StreamingContext sc)
    {
        RecalculateHealthBonus();
        Debug.Log("Loaded time since last reset: " + SoftReset.TimeSinceLastReset);
    }

    public StaminaTraining(Unit unit) : base( unit)
    {
        recalculationTimer = new Timer(10);

        staminaTrainingMult = ArithmeticNode.CreateMult();
        unit.health.chain.Add(1000, staminaTrainingMult );

        base.InitializeViewValues("Stamina Training",
                                  $"Followers' health increases with time from last respawn.",
                                  300);

    }
    public override string updatedDescription { get => "\n({healthBonus})"; }


    [TimeredActionsUNOrdered] void StaminaTraining_Recalculation()
    {
        if (recalculationTimer.Tick()) RecalculateHealthBonus();
    }

    void RecalculateHealthBonus()
    {
        float minutes = SoftReset.TimeSinceLastReset / 60;
        float log = Mathf.Log(minutes  + 2, 1.8f);
        staminaTrainingMult.Mutation = 1 + log*log;
    }

}

public class Regeneration : Talent
{
    Timer timer;

    float
        regenFraction = 0.005f,
        regenFractionValue;

    public Regeneration(Unit unit) : base( unit)
    {
        timer = new Timer(3);

        base.InitializeViewValues("Regeneration",
                                  $"Every second Hero regenerates some health.",
                                  200);

    }
    public override string updatedDescription { get => $"\n({regenFractionValue})"; }


    [TimeredActionsUNOrdered] void Regeneration_Regenerate()
    {
        if ( unit.Alive && timer.Tick() )
        {
            regenFractionValue = unit.healthRange._Max * regenFraction;

            var hargs = new DoHealArgs(unit, regenFractionValue);

            unit.TakeHeal(hargs);
        }
    }
}

public class Infirmary : Talent
{
    Timer timer;

    float
        regenFraction = 0.005f,
        regenFractionValue;
    
    public Infirmary(Unit unit) : base( unit)
    {
        timer = new Timer(3);

        base.InitializeViewValues("Infirmary",
                                  $"Every 3 seconds followers regenerate some health if they are alive.",
                                  200);

    }
    public override string updatedDescription { get => "\n({regenFractionValue})"; }


    [TimeredActionsUNOrdered] void Infirmary_Regenerate()
    {
        if ( unit.Alive && timer.Tick() )
        {
            regenFractionValue = unit.healthRange._Max * regenFraction;

            var hargs = new DoHealArgs(unit, regenFractionValue);

            unit.TakeHeal(hargs);
        }
    }
}


public class BlindingLight : Talent
{
    float damageFraction = .15f,
    damage;

    public BlindingLight(Unit unit) :base(unit)
    {
        base.InitializeViewValues("Blinding Light",
                                  $"When you heal you also deal damage to the Boss.",
                                  300);

    }
    public override string updatedDescription { get => "\n({damage})"; }


    [HealOrder(100)]
    void BlindingLight_Damage(DoHealArgs healArgs)
    {
        damage = healArgs.heal * damageFraction;

        healArgs.healer.target.TakeDamage(new DoDamageArgs(unit, damage));
    }
}


public class Ressurection : Talent
{
    public Ressurection(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Ressurection",
                                  $"Your healing can ressurect followers.",
                                  300);

    }
    public override string updatedDescription { get => ""; }

    [StatInitOrder(0)] public void BecomeRessurector(Unit unit)
    {
        unit.canRessurect = true;
    }
}


public class BattleExpirience : Talent
{
    LoyaltyStat loyalty;

    float postRespawnBonus;

    ArithmeticNode damageAddition;


    public BattleExpirience(Hero hero) : base(hero )
    {
        loyalty = hero.loyalty as LoyaltyStat;

        damageAddition = ArithmeticNode.CreateAdd();
        (unit as Hero).followers.damage.chain.Add(900, damageAddition);


        base.InitializeViewValues("Battle Expirience",
                                  $"Followers do more damage with each respawn based on loyalty attribute.",
                                  300);
    }

    public override string updatedDescription { get => $"\n({damageAddition.Mutation})"; }

    protected override void SubclassActivation()
    {

        loyalty.onRecalculate += CalcPostRespawnBonus;
        SoftReset.onReset += AddBonus;
    }

    protected override void SubclassDeactivation()
    {
        loyalty.onRecalculate -= CalcPostRespawnBonus;
        SoftReset.onReset -= AddBonus;
    }

    void CalcPostRespawnBonus()
    {
        float log = Mathf.Log(loyalty.Result + 2, 10);

        postRespawnBonus = 1 + log * log;
    }

    void AddBonus()
    {
        damageAddition.Mutation += postRespawnBonus;
    }

}

public class VeteransOfThirdWar : Talent
{
    Range follHealth;
    LoyaltyStat loyalty;
    Unit followers;

    Material hpBarMaterial;

    float armorMutation, reflectMutation;
    ArithmeticNode armorMult, reflectAdd;

    bool isVeteransEngaged;
    DoDamageArgs tempDargs;

    float veteransThreshold;
    static public float veteransRatio = 0f;
    int progressProteryID;

    public VeteransOfThirdWar(Hero hero) : base( hero)
    {
        progressProteryID = Shader.PropertyToID("_Progress");

        follHealth = hero.followers.healthRange;
        hpBarMaterial = FollowersView._Inst.healthBar.image.material;

        loyalty = hero.loyalty as LoyaltyStat ;

        hpBarMaterial.SetFloat(progressProteryID, veteransRatio);

        followers = hero.followers;

        armorMult = ArithmeticNode.CreateMult();

        reflectAdd = ArithmeticNode.CreateAdd();


        base.InitializeViewValues("Veterans Of Third War",
                                  $"When followers' health drops bellow certain \n"+
                                  $"threshold, they gain bonus to armor and reflect.\n Threshold is based on Loyalty stat.",
                                  500);


        
    }

    public override string updatedDescription { get => $"\n(Threshold {veteransThreshold})"+
            "\n(Reflect bonus {CalculateReflectBonus})"+
            "\n(Armor bonus {CalculateArmorBonus})"; }


    [TakeDamageOrder(15)]
    void EngageVeterans(DoDamageArgs dargs)
    {
        float diff = follHealth._Val - veteransThreshold;


        if (diff > 0 &&
            !isVeteransEngaged &&
            VeteransGotDamaged(dargs)
        )
        {
            follHealth._Val -= diff;

            dargs.damage._Val -= diff;


            isVeteransEngaged = true;

            Debug.Log("Veterans go!");
        
            armorMult.Mutation = armorMutation;
            reflectAdd.Mutation = reflectMutation;
        }
        else
        {
            isVeteransEngaged = false;

            Debug.Log("Veterans retreat!");

            armorMult.Mutation = 1;
            reflectAdd.Mutation = 0;
        }

        bool VeteransGotDamaged(DoDamageArgs dargs)
        {
            tempDargs = dargs.CopyShallow();

            TakeDamageReflect.DecreaseDamage(tempDargs, followers);

            TakeDamageArmor.DecreaseDamage(tempDargs, followers);


            return tempDargs.damage._Val > diff;
        }
    }

    override protected void SubclassActivation()
    {
        loyalty.onRecalculate += RecalculateVeterans;
    }

    override protected void SubclassDeactivation()
    {
        loyalty.onRecalculate -= RecalculateVeterans;
    }

    private void RecalculateVeterans()
    {
        CalculateThreshold();
        veteransRatio = veteransThreshold / follHealth._Val;

        CalculateArmorBonus();
        if (isVeteransEngaged) armorMult.Mutation = armorMutation;

        CalculateReflectBonus();
        if (isVeteransEngaged) reflectAdd.Mutation = reflectMutation;

        hpBarMaterial.SetFloat(progressProteryID, veteransRatio);
    }

    private void CalculateThreshold()
    {
        veteransThreshold = LoyaltyStat.healthAddition.Mutation;
    }
    private void CalculateReflectBonus()
    {
        float log = Mathf.Log(loyalty.Result + 2, 2);
        reflectMutation = ( 1 + log * log )/5;
    }
    private void CalculateArmorBonus()
    {
        float log = Mathf.Log(loyalty.Result + 2, 2);
        armorMutation = 1 + log * log;
    }
}

public class Rebirth : Talent
{
    Range health => unit.healthRange;

    StatMultChain perseverance;
    
    [JsonProperty]
    float rebirths;

    public Rebirth(Hero hero) : base( hero)
    {
        perseverance = hero.perseverance;

        base.InitializeViewValues("Rebirth",
                                  $"When your health would drop to zero, you rebirth instead of dying."+
                                  $"\nAmount of extra lives is based on perseverance attribute.",
                                  1000);

    }
    public override string updatedDescription { get => $"(Extra lives {rebirths})"; }

    protected override void SubclassActivation()
    {
        CalculateRebirths();

        perseverance.onRecalculate += CalculateRebirths;
        SoftReset.onReset += CalculateRebirths;
        SoftReset.onBossSpawn += CalculateRebirths;
    }

    [TakeDamageOrder(39)]
    void ActivateRebirth(DoDamageArgs dargs)
    {
        bool isDeadlyDamage = dargs.damage._Val >= health._Val;

        if (isDeadlyDamage &&
            rebirths > 0
        )
        {
            dargs.damage._Val = 0;


            float newLife = Mathf.Min(rebirths, 1);

            rebirths -= newLife;

            health._Val = health._Max * newLife;
        }
    }

    void CalculateRebirths()
    {
        float log = Mathf.Log(perseverance.Result + 2, 10);
        rebirths = 1 + log * log*1.5f;
    }
}

public class Dejavu : Talent
{
    StatMultChain perseverance;

    int maxBonusStage = 100;

    float statsFraction;

    List<StatMultChain> stats;
    List<ArithmeticNode> additions;

    float bonus ;
    float percentBonus ;

    public Dejavu(Hero hero) : base( hero)
    {
        perseverance = hero.perseverance;

        base.InitializeViewValues("Dejavu",
                                  $"You get bonus to all stats based on highest stage and last stage.\n" ,
                                  1000);

        InitFields();
    }

    public override string updatedDescription { get => $"(Flat attributes bonus: {bonus})\n"+
            $"(Percent attributes bonus: {percentBonus})"; }


    void InitFields()
    {
        stats =
            typeof(Hero)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(StatMultChain))
            .Select(f => (StatMultChain)f.GetValue(Hero._Inst))
            .ToList();


        additions = new List<ArithmeticNode>(stats.Count);

        for (int i = 0; i < stats.Count; i++)
        {
            additions.Add(new ArithmeticNode(new ArithmAdd(), 0));

            stats[i].chain.Add(900, additions[i]);
        }

    }

    void ApplyDejavu()
    {
        RecalculateStatBonus();

        for (int i = 0; i < stats.Count; i++)
        {
            additions[i].Mutation += (stats[i].isPercentage) ? percentBonus : bonus;
        }
    }

    protected override void SubclassActivation()
    {
        RecalculateStatBonus();
        SoftReset.onReset += ApplyDejavu;
        SoftReset.onBossSpawn += ApplyDejavu;
    }

    protected override void SubclassDeactivation()
    {
        SoftReset.onReset -= ApplyDejavu;
        SoftReset.onBossSpawn -= ApplyDejavu;
    }
    
    void RecalculateStatBonus()
    {
        var halfMaxStage = maxBonusStage / 2;

        var statBonus =
            Mathf.Pow(SoftReset.lastStage - halfMaxStage, 3)
            / Mathf.Pow(maxBonusStage, 2);


        bonus = Mathf.Ceil(Mathf.Clamp(statBonus, 0.1f, 2));
        percentBonus = bonus / 1000000;
    }
}

public class TitansGrowth : Talent
{
    ArithmeticNode titansGrowthMult;

    Timer recalculationTimer;

    [JsonProperty]
    float mult;

    [System.Runtime.Serialization.OnDeserializedAttribute]
    public void OnDeserialize(StreamingContext sc)
    {
        RecalculateHealthMult();
        Debug.Log("Loaded time since last reset: " + SoftReset.TimeSinceLastReset);
    }

    public TitansGrowth(Unit unit) : base( unit)
    {
        recalculationTimer = new Timer(60);

        titansGrowthMult = ArithmeticNode.CreateMult();

        unit.health.chain.Add(1232, titansGrowthMult);

        base.InitializeViewValues("Titan's Growth",
                                  $"Your health increases with time since last respawn ({mult}X)",
                                  1000);
    }
    public override string updatedDescription { get => $"({mult}%)"; }

    [TimeredActionsUNOrdered]
    void MainRecalculateHealthMult()
    {
        if (recalculationTimer.Tick())
        {
            RecalculateHealthMult();
        }
    }

    void RecalculateHealthMult()
    {
        float minutes = SoftReset.TimeSinceLastReset / 60;
        float log = Mathf.Log(minutes + 2, 2);
        mult = log * log;
        titansGrowthMult.Mutation = mult;
    }

    protected override void SubclassActivation()
    {
        RecalculateHealthMult();
        SoftReset.onReset += RecalculateHealthMult;
        SoftReset.onBossSpawn += RecalculateHealthMult;
    }

    protected override void SubclassDeactivation()
    {
        titansGrowthMult.Mutation = 1;
    }


}

public class CounterAttack : Talent
{
    float chance = .2f;


    public CounterAttack(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Counter Attack",
                                  $"You have {chance:P0} chance to counter attack each time you take damage.",
                                  1000);
    }
    public override string updatedDescription { get => $""; }


    [TakeDamageOrder(0)] void MakeCounterAttack(DoDamageArgs dargs)
    {
        if (UnityEngine.Random.value < chance)
            unit.makeAttack();
    }
}

public class Cyclone : Talent
{
    ArithmeticNode cycloneMult;

    float mult = .5f;
    
    public Cyclone(Unit unit) : base(unit)
    {

        cycloneMult = ArithmeticNode.CreateMult();

        unit.attackSpeed.chain.Add(cycloneMult);


        base.InitializeViewValues("Cyclone",
                                  $"You attack {mult:P0} faster.",
                                  300);

    }
    public override string updatedDescription { get => $""; }


    protected override void SubclassActivation()
    {
        cycloneMult.Mutation = mult;
    }

    protected override void SubclassDeactivation()
    {
        cycloneMult.Mutation = 1;
    }
    
}

public class EnfeeblingStrike : Talent
{
    float mult = .6f;

    bool isEnemyEnfeebled ;

    public EnfeeblingStrike(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Enfeeble",
                                  $"",
                                  500);

    }
    public override string updatedDescription { get => $"After your critical strike Boss' next attack will deal {1 - mult:P0} less damage."; }

    protected override void SubclassActivation()
    {
        unit.attackChain.Add(21, Enfeeble);

        unit.target.attackChain.Add(0, ConsumeEnfeeble);
    }

    protected override void SubclassDeactivation()
    {
        unit.attackChain.Remove(21, Enfeeble);

        unit.target.attackChain.Remove(0, ConsumeEnfeeble);
    }

    void Enfeeble(DoDamageArgs dargs)
    {
        if (dargs.isCritical)
        {
            isEnemyEnfeebled = true;
        }
    }

    void ConsumeEnfeeble(DoDamageArgs dargs)
    {
        dargs.damage._Val *= mult;

        isEnemyEnfeebled = false;
    }
}


public class Multicrit : Talent
{
    float critChanceLimitVal = 10f;

    public Multicrit(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Multicrit",
                                  $"Your crit chance now can rise up to {critChanceLimitVal}*hundred %.\n"
                                  +$"It means you can crit multiple times in one attack.\n"
                                  +$"These crits stack additively.",
                                  3000);
    }
    public override string updatedDescription { get => $""; }


    protected override void SubclassActivation()
    {
        unit.critChance.limit.Mutation = critChanceLimitVal;
    }

    protected override void SubclassDeactivation()
    {
        unit.critChance.limit.Mutation = 1f;
    }

}
