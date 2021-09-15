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
    static public Action<Talent> onActivation;
    TalentView view;

    public string
        name = "*PLACEHOLDER*",
        description = "*PLACEHOLDER*";

    abstract public string updatedDescription {get;}

    public float cost;

    [JsonPropertyAttribute] public bool isDiscovered, isBought;

    [System.Runtime.Serialization.OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext sc)
    {
       onDeserializedConcrete?.Invoke();
    }
    protected Action onDeserializedConcrete;

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
        isDiscovered = true;

        view.SwitchState(view.discoveredState);
    }

    public void Buy(Currency currency)
    {
        if (!isBought && currency.Buy(cost))
        {
            isBought = true;

            Activate();
        }
    }

    new public void Activate()
    {
        base.Activate();
        view.SwitchState(view.boughtState);
        onActivation?.Invoke(this);
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


    public Talent SetPhase(int phase)
    {
        cost = phase * 20;
        return this;
    }
}

public class FullBlood : LiftedTalent // Debuged
{
    float fullBonus = 1f;
    float currentBonus;



    public FullBlood(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Full Blood",
                                  $"Increases damage based on remaining health.\nFull bonus at full health",
                                  50);

    }

    public override string updatedDescription { get => $"\n{currentBonus:P0}"; }

    [AttackOrder(15)] void Attack_FullBlood(DoDamageArgs damageArgs)
    {
        damageArgs.damage._Val *= 1f + currentBonus;
    }


    protected override void Connect()
    {
        UpdateBonus(unit.healthRange.GetRatio());
        unit.healthRange.onRatioChanged += UpdateBonus;
    }

    void UpdateBonus(float healthRatio)
    {
        currentBonus = fullBonus * healthRatio;
    }

    protected override void Disconnect()
    {
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class BloodMadness : LiftedTalent // Debuged
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
                                $"This damage cannot be critical",
                                  150);
    }

    public override string updatedDescription { get => $"\n({bonusDamage})"; }

    protected override void Connect()
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
        return (log*log)/5;
    }

    protected override void Disconnect()
    {
    }
}

public class Transfusion : LiftedTalent
{
    float fraction = .5f;

    public Transfusion() :base(Hero._Inst)
    {
        base.InitializeViewValues("Transfusion",
                                  $"Hero's vampirism now heals followers instead of Hero if Hero's health is full",
                                  150);
    }

    public override string updatedDescription { get => ""; }

    protected override void Connect()
    {
        unit.takeHealChain.Add(120, VampOverhealToFollowers);
    }

    void VampOverhealToFollowers(DoHealArgs hargs)
    {
        if (hargs.isVamp && hargs.heal > 0)
        {
            hargs.heal *= fraction;
            unit.followers.TakeHeal(hargs);
        }
    }

    protected override void Disconnect()
    {
    }
}

public class HotHand : LiftedTalent   // Debuged
{
    float
        critDamageLimitVal = 2.5f,
        selfDamageMult = .15f;

    public HotHand(Unit unit) : base(unit)
    {

        base.InitializeViewValues("Hot Hand",
                                  $"Hero's critical damage limit \n"
                                  +$"increased from 150% to {critDamageLimitVal:P0}, but Hero takes 15% of it.\n"
                                  +$"This damage ignores armor and reflect",
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

    protected override void Connect()
    {
        unit.critMult.limitGrowth.Mutation = critDamageLimitVal;
    }

    protected override void Disconnect()
    {
        unit.critMult.limitGrowth.Mutation = 1.5f;
    }

}
public class Block : LiftedTalent // Debuged
{
    float
        chance = .25f,
        mult = 3f;

    public Block(Unit unit) : base(unit)
    {

        base.InitializeViewValues("Block",
                                  $"Hero has {chance:P0} on being hit to defend with double armor",
                                  1000);
    }

    public override string updatedDescription { get => ""; }


    override protected void Connect()
    {
        unit.hasBlock = true;
        unit.blockChance = chance;
        unit.blockMult = mult;
    }

    protected override void Disconnect()
    {
    }
}

public class LastStand : LiftedTalent // Debuged
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
                                  $"You have {mult - 1f:P0} more armor while below {healthThreshold:P0} health",
                                  100);

    }

    public override string updatedDescription { get => ""; }


    protected override void Connect()
    {
        unit.onDeathChain.Add((unit)=>{ armorBonus.Mutation = 1; });
    }

    protected override void Disconnect()
    {
    }

    [TakeDamageOrder(45)] void Laststand_ActivateBonus(DoDamageArgs damageArgs)
    {
        bool underThreshold = unit.healthRange.GetRatio() < healthThreshold;

        if (isActive != underThreshold)
            armorBonus.Mutation = (underThreshold) ? mult : 1;

        isActive = underThreshold;
    }

}


public class FindWeakness : LiftedTalent // Debuged
{
    private float critChanceStack = .01f;
    private float totalCritChanceBonus;

    ArithmeticNode critBonus;


    public FindWeakness(Unit unit) : base(unit)
    {
        critBonus = new ArithmeticNode(new ArithmAdd(), 0);

        unit.critChance.chain.Add(critBonus);

        base.InitializeViewValues("Find Weakness",
                                  $"Critical chance growth by 1% for each non-critical attack.\nResets on crit.\n",
                                  100);

    }

    public override string updatedDescription { get => $"{totalCritChanceBonus:P0}"; }

    protected override void Connect()
    {
        unit.attackChain.Add(22, Stack_FindWeakness);
    }

    protected override void Disconnect()
    {
    }

    void Stack_FindWeakness(DoDamageArgs dargs)
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


public class DoubleJudgement : LiftedTalent // Debuged
{
    static public Timer timer;

    public int judgementsCount;
    float judgementDamage;

    float mult = 2f;
    ArithmeticNode reflectMult;

    public DoubleJudgement(Unit unit) : base( unit)
    {
        timer = new Timer(1);

        reflectMult = ArithmeticNode.CreateMult();

        unit.reflect.chain.Add(reflectMult);

        base.InitializeViewValues("Double Judgement",
                                  $"Hero's reflect is multiplied by {mult:P0} and \n"
                                  +"strikes second time after short delay",
                                  100);

    }

    public override string updatedDescription { get => $"({mult:P0})"; }

    protected override void Connect()
    {
        unit.timeredActionsList.Add(Judgement);
        unit.target.takeDamageChain.Add(41, OnEnemyTookReflectedDamage);
        reflectMult.Mutation = mult;
    }

    void OnEnemyTookReflectedDamage(DoDamageArgs dargs)
    {
        if (!dargs.isJudgement && dargs.isReflected)
        {
            judgementsCount++;
            judgementDamage = dargs.damage._Val;
        }
    }

    void Judgement()
    {
        if (judgementsCount > 0 && timer.Tick())
        {
            judgementsCount--;

            DoDamageArgs judgement =
                new DoDamageArgs(unit, judgementDamage)
                { isReflected = true, isJudgement = true };

            unit.target.TakeDamage(judgement);
        }
    }

    protected override void Disconnect()
    {
    }
}

public class MassReflect : LiftedTalent // Debuged
{
    public MassReflect(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Mass Reflect", $"Hero's reflect now defends Followers", 1000);
    }

    public override string updatedDescription => "";


    protected override void Connect()
    {
        Followers._Inst.reflect = Hero._Inst.reflect;

        var takeDamageReflect = new TakeDamageReflect(Followers._Inst);

        takeDamageReflect.Activate();
    }

    protected override void Disconnect()
    {
    }
}

public class Interruption : LiftedTalent // Debuged
{
    float interrupted;

    public Interruption(Hero hero) : base(hero)
    {
        base.InitializeViewValues("Interruption",
                                  $"Hero takes half of the damage that Followers would take, if Hero has more health than Followers",
                                  100);


    }
    public override string updatedDescription { get => ""; }


    override protected void Connect()
    {
        unit.followers.takeDamageChain.Add(35, Interrupt);
        unit.takeDamageChain.Add(15, ConsumeInterruptedDamage);
    }

    void Interrupt(DoDamageArgs damageArgs)
    {
        if (!IsHeroHelthierThanFollowers() || damageArgs.isReflected) return;

        interrupted = damageArgs.damage._Val * .5f;

        damageArgs.damage._Val -= interrupted;
    }
    bool IsHeroHelthierThanFollowers()
    {
        return Followers._Inst.Alive && Hero._Inst.healthRange._Val > Followers._Inst.healthRange._Val;
    }

    void ConsumeInterruptedDamage(DoDamageArgs dargs)
    {
        if (interrupted > 0)
        {
            dargs.damage._Val += interrupted;

            interrupted = 0f;
        }
    }

    protected override void Disconnect()
    {
    }
}


public class CoordinatedActions : LiftedTalent // Debuged
{
    float mult = .7f;

    ArithmeticNode attackspeedMult;

    public CoordinatedActions(Unit unit) : base(unit)
    {
        attackspeedMult = ArithmeticNode.CreateMult();

        unit.attackSpeed.chain.Add(attackspeedMult);

        base.InitializeViewValues("Coordinated Actions",
                                  $"Followers attack {1f-mult:P0} faster",
                                  100);

    }

    public override string updatedDescription { get => ""; }

    protected override void Connect()
    {
        attackspeedMult.Mutation = mult;
    }

    protected override void Disconnect()
    {
        attackspeedMult.Mutation = 1;
    }

}

public class Diversion : LiftedTalent // Debuged
{
    float fraction = 0.1f;
    float diversionDamage;

    public Diversion(Unit unit ) : base(unit )
    {
        base.InitializeViewValues("Diversion",
                                  $"Followers deal extra {fraction:P0} damage which ignores the Boss's defences",
                                  200);

    }
    public override string updatedDescription { get => $"\n{fraction:P0}"; }

    protected override void Connect()
    {
        unit.attackChain.Add(200, SaveDiversionDamage);
    }

    protected override void Disconnect()
    {
    }

    void SaveDiversionDamage(DoDamageArgs dargs)
    {
        diversionDamage = dargs.damage._Val * fraction;

        DoDamageArgs diversionDargs = new DoDamageArgs(dargs.attacker, diversionDamage){ isDiversion = true };

        dargs.target.TakeDamage(diversionDargs);
    }
}


public class Regeneration : LiftedTalent // Debuged
{
    Timer timer;

    float
        regenFraction = 0.02f,
        regenFractionValue;

    public Regeneration(Unit unit) : base( unit)
    {
        timer = new Timer(1);

        base.InitializeViewValues("Regeneration",
                                  $"Every {timer.endTime:F0} seconds Hero regenerates some health",
                                  200);

    }
    public override string updatedDescription { get => $"\n({regenFractionValue:F0})"; }

    protected override void Connect()
    {
        unit.timeredActionsList.Add(Regeneration_Regenerate);
    }

    protected override void Disconnect()
    {
        unit.timeredActionsList.Remove(Regeneration_Regenerate);
    }

    void Regeneration_Regenerate()
    {
        if ( unit.Alive && timer.Tick() )
        {
            regenFractionValue = unit.healthRange._Max * regenFraction;

            var hargs = new DoHealArgs(unit, regenFractionValue){ isRegen = true };

            unit.TakeHeal(hargs);
        }
    }
}

public class Infirmary : LiftedTalent // Debuged
{
    Timer timer;

    float
        regenFraction = 0.02f,
        regenFractionValue;

    public Infirmary(Unit unit) : base( unit)
    {
        timer = new Timer(1);

        base.InitializeViewValues("Infirmary",
                                  $"Every {timer.endTime:F0} seconds followers regenerate some health if they are alive",
                                  200);

    }
    public override string updatedDescription { get => $"\n({regenFractionValue:F0})"; }

    protected override void Connect()
    {
        unit.timeredActionsList.Add(Infirmary_Regenerate);
    }

    protected override void Disconnect()
    {
        unit.timeredActionsList.Remove(Infirmary_Regenerate);
    }

    [TimeredActionsUNOrdered] void Infirmary_Regenerate()
    {
        if ( unit.Alive && timer.Tick() )
        {
            regenFractionValue = unit.healthRange._Max * regenFraction;

            var hargs = new DoHealArgs(unit, regenFractionValue){ isRegen = true };

            unit.TakeHeal(hargs);
        }
    }
}


public class BlindingLight : LiftedTalent // Debuged
{
    float damageFraction = .25f;

    public BlindingLight(Unit unit) :base(unit)
    {
        base.InitializeViewValues("Blinding Light",
                                  $"Hero's healing deals damage equal to {damageFraction:P0} of healpower",
                                  300);

    }
    public override string updatedDescription { get => ""; }


    protected override void Connect()
    {
        unit.healingChain.Add(0, BlindingLight_Damage);
    }

    protected override void Disconnect()
    {
    }

    void BlindingLight_Damage(DoHealArgs healArgs)
    {
        if (healArgs.isHeal)
        {
            var dargs = new DoDamageArgs(unit, healArgs.heal * damageFraction){ isBlindingLight = true };

            unit.target.TakeDamage(dargs);
        }
    }
}


public class Ressurection : LiftedTalent // Debuged
{
    float mult = 2f;

    ArithmeticNode healingBonus;

    public Ressurection(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Ressurection",
                                  $"Hero can ressurect Followers and healing is {mult-1f:P0} more powerful",
                                  300);

        unit.healing.chain.Add( healingBonus = ArithmeticNode.CreateMult() );
    }

    public override string updatedDescription { get => ""; }

    protected override void Connect()
    {
        unit.canRessurect = true;
        healingBonus.Mutation = mult;
    }

    protected override void Disconnect()
    {
        unit.canRessurect = false;
        healingBonus.Mutation = 1f;
    }
}


[JsonObjectAttribute(MemberSerialization.OptIn)]
public class BattleExpirience : LiftedTalent // Debuged
{
    LoyaltyStat loyalty;

    [JsonPropertyAttribute]
    float damageMutation;

    ArithmeticNode damageAddition;


    public BattleExpirience(Hero hero) : base(hero )
    {
        loyalty = hero.loyalty as LoyaltyStat;

        damageAddition = ArithmeticNode.CreateAdd();
        (unit as Hero).followers.damage.chain.Add(900, damageAddition);


        base.InitializeViewValues("Battle Expirience",
                                  $"Followers do more damage based on respawn count",
                                  300);

        onDeserializedConcrete = ()=>
        {
            damageAddition.Mutation = damageMutation;
        };
    }

    public override string updatedDescription { get => $"\n({damageMutation})"; }

    protected override void Connect()
    {
        ApplyBonus();
        SoftReset.onReset += ApplyBonus;
    }

    protected override void Disconnect()
    {
        SoftReset.onReset -= ApplyBonus;
    }

    void ApplyBonus()
    {
        damageMutation = SoftReset.respawnCount;
        damageAddition.Mutation = damageMutation;
    }

}

public class VeteransOfThirdWar : LiftedTalent // Debuged
{
    LoyaltyStat loyalty;

    Material hpBarMaterial;

    float armorMutation = 1;
    ArithmeticNode armorBonus;

    bool isVeteransEngaged;

    static public float veteransRatio = 0f;
    int progressProteryID;

    public VeteransOfThirdWar() : base(Followers._Inst)
    {
        loyalty = Hero._Inst.loyalty as LoyaltyStat;

        armorBonus = ArithmeticNode.CreateMult();

        unit.armor.chain.Add(1000, armorBonus);


        base.InitializeViewValues("Veterans Of Third War",
                                  "",
                                  500);


        progressProteryID = Shader.PropertyToID("_Progress");

        hpBarMaterial = FollowersView._Inst.healthBar.image.material;
        hpBarMaterial.SetFloat(progressProteryID, veteransRatio);
    }

    public override string updatedDescription { get =>
            $"When Followers' health drops below {veteransRatio:P0}, they recieve armor bonus.\nThreshold and bonus are based on Loyalty stat.\n\n(Armor bonus {armorMutation-1f:P0})";
            }


    [TakeDamageOrder(45)]
    void EngageVeterans(DoDamageArgs dargs)
    {
        bool underThreshold = unit.healthRange.GetRatio() < veteransRatio;

        if (isVeteransEngaged != underThreshold)
        {
            armorBonus.Mutation = (underThreshold) ? armorMutation : 1;
        }

        isVeteransEngaged = underThreshold;

    }

    override protected void Connect()
    {
        RecalculateVeterans();
        loyalty.onRecalculate += RecalculateVeterans;
        unit.onDeathChain.Add((unit) =>{ armorBonus.Mutation = 1; });
    }

    override protected void Disconnect()
    {
        loyalty.onRecalculate -= RecalculateVeterans;
    }

    private void RecalculateVeterans()
    {
        CalculateThreshold();

        CalculateArmorBonus();

        if (isVeteransEngaged) armorBonus.Mutation = armorMutation;

        hpBarMaterial.SetFloat(progressProteryID, veteransRatio);
    }
    private void CalculateThreshold()
    {
        float log = Mathf.Log(loyalty.Result + 2, 10) / 10;
        veteransRatio = Mathf.Min(1, log);
    }
    private void CalculateArmorBonus()
    {
        armorMutation = 1 + Mathf.Log(loyalty.Result + 2, 10) / 10;
        armorBonus.Mutation = armorMutation;
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Rebirth : LiftedTalent   // Debuged
{
    Range health => unit.healthRange;

    StatMultChain perseverance;

    [JsonProperty]
    float maxRebirths, rebirths;

    public Rebirth(Hero hero) : base( hero)
    {
        perseverance = hero.perseverance;

        base.InitializeViewValues("Rebirth",
                                  $"When Hero's health would drop to zero,\n"
                                  +"they rebirth instead of dying.\n"+
                                  $"Amount of extra lives is based\n"
                                  +"on perseverance attribute.\n",
                                  1000);
    }
    public override string updatedDescription { get =>
            $"(Max extra lives {maxRebirths:F2})\n(Current extra lives {rebirths:F2})"; }

    protected override void Connect()
    {
        CalculateRebirths();
        RestoreRebirths();

        perseverance.onRecalculate += CalculateRebirths;
        SoftReset.onReset += RestoreRebirths;

        unit.takeDamageChain.Add(39, ActivateRebirth);
    }

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
        maxRebirths = 1 + log * log*1.5f;
    }

    void RestoreRebirths()
    {
        rebirths = maxRebirths;
    }

    protected override void Disconnect()
    {
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Dejavu : LiftedTalent    // Debuged
{
    StatMultChain perseverance;

    List<StatMultChain> stats;
    List<ArithmeticNode> multipliers;

    [JsonPropertyAttribute]
    float bonus;

    public Dejavu(Hero hero) : base( hero)
    {
        perseverance = hero.perseverance;

        base.InitializeViewValues("Dejavu",
                                  $"Hero's stats grow with each respawn.\n",
                                  1000);

        InitFields();

        onDeserializedConcrete = () =>
        {
            ApplyBonus();
            Debug.Log("-- Dejavu Loaded");
        };
    }

    public override string updatedDescription { get => $"{bonus:P3}"; }


    void InitFields()
    {
        stats =
            typeof(Hero)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(StatMultChain))
            .Select(f => (StatMultChain)f.GetValue(Hero._Inst))
            .ToList();


        multipliers = new List<ArithmeticNode>(stats.Count);

        for (int i = 0; i < stats.Count; i++)
        {
            multipliers.Add(ArithmeticNode.CreateMult());

            stats[i].chain.Add(9000, multipliers[i]);
        }

    }

    protected override void Connect()
    {
        SoftReset.onReset += ()=>{ UpdateBonus(); ApplyBonus(); };
    }

    protected override void Disconnect()
    {
    }
    
    void UpdateBonus()
    {
        float bonusAddition = Mathf.Log(SoftReset.maxStage + 1, 20) / 2e4f;

        bonus += bonusAddition;
    }

    void ApplyBonus()
    {
        for (int i = 0; i < stats.Count; i++)
        {
            multipliers[i].Mutation = 1 + bonus;
        }
    }
}

public class StaminaTraining : TitansGrowth // Debuged
{
    public StaminaTraining(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Stamina Training",
                                  $"Followers' health increases based on respawn count",
                                  300);

        logMult = .02f;
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class TitansGrowth : LiftedTalent // Debug
{
    protected ArithmeticNode titansGrowthMult;

    public Timer recalculationTimer;

    [JsonProperty]
    protected float mult = 1;
    protected float logMult = .031f;


    public TitansGrowth(Unit unit) : base( unit)
    {
        recalculationTimer = new Timer(10);

        titansGrowthMult = ArithmeticNode.CreateMult();

        unit.health.chain.Add(1232, titansGrowthMult);

        base.InitializeViewValues("Titan's Growth",
                                  $"Hero's health increases based on respawn count",
                                  1000);

        onDeserializedConcrete = ()=>
        {
            RecalculateHealthMult();
            Debug.Log("Loaded time since last reset: " + SoftReset.TimeSinceLastReset);
        };

    }
    public override string updatedDescription { get => $"\n{mult:P0}"; }

    [TimeredActionsUNOrdered]
    protected void MainRecalculateHealthMult()
    {
        if (recalculationTimer.Tick())
        {
            RecalculateHealthMult();
        }
    }

    protected void RecalculateHealthMult()
    {
        mult = 1 + Mathf.Log(SoftReset.respawnCount + 1, 2) * logMult;

        titansGrowthMult.Mutation = mult;
    }

    protected override void Connect()
    {
        RecalculateHealthMult();
        SoftReset.onReset += RecalculateHealthMult;
        SoftReset.onReset += recalculationTimer.Reset;
    }

    protected override void Disconnect()
    {
        titansGrowthMult.Mutation = 1;
    }
}

public class Blitz : LiftedTalent
{
    float critChance ;

    public Blitz() : base(Followers._Inst)
    {
        base.InitializeViewValues("Blitz",
                                  $"Followers can crit with 15% chance\ndealing {unit.critMult.Result:P0} damage",
                                  20_000);
    }

    public override string updatedDescription => string.Empty;


    protected override void Connect()
    {
        unit.critChance.basevalue = new ArithmeticNode(critChance);
    }

    protected override void Disconnect()
    {
    }
}

public class CounterAttack : LiftedTalent // Debuged
{
    float chance = .2f;


    public CounterAttack(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Counter Attack",
                                  $"{chance:P0} chance to counterattack\n"+
                                  "each time Hero or Followers take damage",
                                  1000);

    }
    public override string updatedDescription { get => $""; }


    override protected void Connect()
    {
        unit.takeDamageChain.Add(0, MakeCounterAttack);
        unit.followers.takeDamageChain.Add(0, MakeCounterAttack);
    }

    protected override void Disconnect()
    {
    }

    void MakeCounterAttack(DoDamageArgs dargs)
    {
        if (dargs.IsSimpleAttack && UnityEngine.Random.value < chance)
            unit.makeAttack();
    }
}

public class Cyclone : LiftedTalent   //Debuged
{
    ArithmeticNode cycloneMult;

    float mult = .5f;
    
    public Cyclone(Unit unit) : base(unit)
    {

        cycloneMult = ArithmeticNode.CreateMult();

        unit.attackSpeed.chain.Add(cycloneMult);


        base.InitializeViewValues("Cyclone",
                                  $"Hero attacks {mult:P0} faster",
                                  300);

    }
    public override string updatedDescription { get => ""; }


    protected override void Connect()
    {
        cycloneMult.Mutation = mult;
    }

    protected override void Disconnect()
    {
        cycloneMult.Mutation = 1;
    }
    
}

public class EnfeeblingStrike : LiftedTalent // Debuged
{
    float mult = .6f;

    bool isEnemyEnfeebled ;

    public EnfeeblingStrike(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Enfeeble",
                                  $"",
                                  500);

    }
    public override string updatedDescription { get => $"After Hero's critical strike Boss' next attack will deal {1 - mult:P0} less damage"; }

    protected override void Connect()
    {
        unit.attackChain.Add(21, Enfeeble);

        unit.target.attackChain.Add(0, ConsumeEnfeeble);
    }

    protected override void Disconnect()
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
        if (isEnemyEnfeebled)
        {
            dargs.damage._Val *= mult;

            isEnemyEnfeebled = false;
        }
    }
}


public class Multicrit : LiftedTalent // Debuged
{
    float critChanceLimitVal = 10f;

    public Multicrit(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Multicrit",
                                  $"Hero's critical chance limit equals {critChanceLimitVal:P0}.\n"
                                  +"This enables Hero to crit multiple times per attack\n"
                                  +"Damage bonus from multiple crits stacks additively",
                                  3000);
    }
    public override string updatedDescription { get => $""; }


    protected override void Connect()
    {
        unit.critChance.limitGrowth.Mutation = critChanceLimitVal;
    }

    protected override void Disconnect()
    {
        unit.critChance.limitGrowth.Mutation = 1f;
    }

}

