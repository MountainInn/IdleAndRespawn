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
    static public Action<Talent>
        onDiscoveredFirstTime;
        static public Action<TalentStripView>
        onStripViewInitialized;
    public Action
        onDeserializedConcrete,
        onDiscovered,
        onActivated,
        onRecalculated;
    public string
        name = "*PLACEHOLDER*",
        description = "*PLACEHOLDER*";

    abstract public string updatedDescription {get;}
    public TalentStripView view;

    [JsonPropertyAttribute] public OneTimeVendible vendible;
    [JsonPropertyAttribute] public Lifted lifted;
    [JsonPropertyAttribute]
    public bool
        isDiscovered,
        isFirstTimeDiscovered = true;

    [OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext sc)
    {
        onDeserializedConcrete?.Invoke();

        Debug.Log("Loaded");

        if (vendible.isOwned)
        {
            vendible.onBought.Invoke();
            isFirstTimeDiscovered = false;
        }
        else if (lifted.isLifted)
        {
            lifted.onLifted -= Discover;
            isFirstTimeDiscovered = false;
            Discover();
        }
    }

    protected Talent(Unit unit) :base(unit)
    {
        lifted = new Lifted();
        lifted.onLifted += Discover;

        onDiscovered = () =>{ view.SwitchState(view.discoveredState); };

        vendible = new OneTimeVendible(Vault.TalentPoints);
        vendible.onBought = Activate;

        onActivated = () =>{ view.SwitchState(view.boughtState); };
    }

    protected void InitializeViewValues(string name, string description)
    {
        this.name = name;
        this.description = description;
    }


    public void Discover()
    {
        EnsureViewExists();
        isDiscovered = true;
        onDiscovered.Invoke();

        if (isFirstTimeDiscovered)
        {
            Debug.Log("First time");
            onDiscoveredFirstTime?.Invoke(this);
            isFirstTimeDiscovered = false;
        }
    }

    override public bool CanActivate()
        => vendible.isOwned;

    new protected void Activate()
    {
        EnsureViewExists();
        base.Activate();
        onActivated.Invoke();;
    }

    protected void EnsureViewExists()
    {
        if (view == null)
        {
            view = ReferenceHeap.InstTalentStripView();
            view.ConnectToTalent(this);

            onStripViewInitialized?.Invoke(view);
        }
    }

    public Talent SetPhase(int phase)
    {
        vendible.price = phase * 20;
        return this;
    }
}

public class BloodHunger : Talent // Debuged
{
    float fullBonus = .7f;
    float currentBonus;



    public BloodHunger(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Blood Hunger",
                                  $"Increases damage based on remaining health.\nFull bonus at zero health");

        unit.attackChain.Add(15, Attack_BloodHunger);
    }

    public override string updatedDescription { get => $"\n{currentBonus:P0}"; }

    void Attack_BloodHunger(DoDamageArgs damageArgs)
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
        currentBonus = fullBonus * (1.0f - healthRatio);
        onRecalculated?.Invoke();
    }

    protected override void Disconnect()
    {
    }
}

public class Transfusion : Talent
{
    float followersHealthFraction = .1f;

    public Transfusion() :base(Hero._Inst)
    {
        base.InitializeViewValues("Transfusion",
                                  $"While Hero's health is full their vampirism heals Followers for up to {followersHealthFraction:P0} of max health");
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
            hargs.heal = Mathf.Min(hargs.heal, unit.followers.healthRange._Max * .05f);
            unit.followers.TakeHeal(hargs);
        }
    }

    protected override void Disconnect()
    {
    }
}

public class HotHand : Talent   // Debuged
{
    float
        critDamageLimitVal = 2.5f,
        selfDamageMult = .25f;

    public HotHand(Unit unit) : base(unit)
    {

        base.InitializeViewValues("Hot Hand",
                                  $"Hero's critical damage limit = {critDamageLimitVal:P0}, but Hero takes 15% of critical damage.\n"
                                  +$"This damage ignores armor");
    }
    public override string updatedDescription { get => ""; }

    void Hothanded_DamageSelf(DoDamageArgs damageArgs)
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
        unit.critMult.growthLimit.Mutation = critDamageLimitVal;
        unit.attackChain.Add(30, Hothanded_DamageSelf);
    }

    protected override void Disconnect()
    {
        unit.critMult.growthLimit.Mutation = 1.5f;
    }

}
public class Block : Talent // Debuged
{
    float
        chance = .5f,
        mult = 2f;

    public Block(Unit unit) : base(unit)
    {

        base.InitializeViewValues("Block",
                                  $"Hero has {chance:P0} chance to defend with {mult:P0} armor");
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

public class LastStand : Talent // Debuged
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
                                  $"You have {mult - 1f:P0} more armor while below {healthThreshold:P0} health");

    }

    public override string updatedDescription { get => ""; }


    protected override void Connect()
    {
        unit.onDeathChain.Add((unit)=>{ armorBonus.Mutation = 1; });
        unit.takeDamageChain.Add(45, Laststand_ActivateBonus);
    }

    protected override void Disconnect()
    {
    }

    void Laststand_ActivateBonus(DoDamageArgs damageArgs)
    {
        bool underThreshold = unit.healthRange.GetRatio() < healthThreshold;

        if (isActive != underThreshold)
            armorBonus.Mutation = (underThreshold) ? mult : 1;

        isActive = underThreshold;
    }

}


public class FindWeakness : Talent // Debuged
{
    private float critChanceStack = .01f;
    private float totalCritChanceBonus;

    ArithmeticNode critBonus;


    public FindWeakness(Unit unit) : base(unit)
    {
        critBonus = new ArithmeticNode(new ArithmAdd(), 0);

        unit.critChance.chain.Add(critBonus);

        base.InitializeViewValues("Find Weakness",
                                  $"Critical chance growth by 1% for each non-critical attack.\nResets on crit.\n");

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


public class Interruption : Talent // Debuged
{
    float fraction = .6f;
    float interrupted;

    public Interruption(Hero hero) : base(hero)
    {
        base.InitializeViewValues("Interruption",
                                  $"Hero takes {fraction:P0} of the damage that Followers would take, if Hero has more health than Followers");


    }
    public override string updatedDescription { get => ""; }


    public void PublicConnect()
    {
        Connect();
    }

    override protected void Connect()
    {
        unit.followers.takeDamageChain.Add(35, Interrupt);
        unit.takeDamageChain.Add(15, ConsumeInterruptedDamage);
    }

    void Interrupt(DoDamageArgs damageArgs)
    {
        if (!IsHeroHelthierThanFollowers() || damageArgs.isReflected || damageArgs.isInterrupted) return;

        interrupted = damageArgs.damage._Val * fraction;
        damageArgs.damage._Val -= interrupted;
    }
    bool IsHeroHelthierThanFollowers()
    {
        return Followers._Inst.Alive && Hero._Inst.healthRange._Val > Followers._Inst.healthRange._Val;
    }

    void ConsumeInterruptedDamage(DoDamageArgs dargs)
    {
        if (dargs.isInterrupted) return;

        if (interrupted > 0)
        {
            var interruptedDargs = new DoDamageArgs(dargs.attacker, interrupted){isInterrupted = true};

            interrupted = 0f;

            unit.TakeDamage(interruptedDargs);
        }
    }

    protected override void Disconnect()
    {
    }
}


public class CoordinatedActions : Talent // Debuged
{
    float mult = .7f;

    ArithmeticNode attackspeedMult;

    public CoordinatedActions(Unit unit) : base(unit)
    {
        attackspeedMult = ArithmeticNode.CreateMult();

        unit.attackSpeed.chain.Add(attackspeedMult);

        base.InitializeViewValues("Coordinated Actions",
                                  $"Followers attack {1f-mult:P0} faster");

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

public class Diversion : Talent // Debuged
{
    float fraction = 0.2f;
    float diversionDamage;

    public Diversion(Unit unit ) : base(unit )
    {
        base.InitializeViewValues("Diversion",
                                  $"Followers deal extra {fraction:P0} damage which ignores the Boss's defences");

    }
    public override string updatedDescription { get => ""; }

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


public class Regeneration : Talent // Debuged
{
    Timer timer;

    float
        regenFraction = 0.005f,
        regenFractionValue;

    public Regeneration(Unit unit) : base( unit)
    {
        timer = new Timer(1);

        base.InitializeViewValues("Regeneration",
                                  $"Every {timer.endTime:F0} seconds Hero regenerates some health");

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
            onRecalculated?.Invoke();
        }
    }
}

public class Infirmary : Talent // Debuged
{
    Timer timer;

    float
        regenFraction = 0.005f,
        regenFractionValue;

    public Infirmary(Unit unit) : base( unit)
    {
        timer = new Timer(1);

        base.InitializeViewValues("Infirmary",
                                  $"Every {timer.endTime:F0} seconds followers regenerate some health if they are alive");

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

    void Infirmary_Regenerate()
    {
        if ( unit.Alive && timer.Tick() )
        {
            regenFractionValue = unit.healthRange._Max * regenFraction;

            var hargs = new DoHealArgs(unit, regenFractionValue){ isRegen = true };

            unit.TakeHeal(hargs);
            onRecalculated?.Invoke();
        }
    }
}


public class BlindingLight : Talent // Debuged
{
    float damageFraction = .5f;

    public BlindingLight(Unit unit) :base(unit)
    {
        base.InitializeViewValues("Blinding Light",
                                  $"Hero's healing deals damage equal to {damageFraction:P0} of healing power");


    }
    public override string updatedDescription { get => ""; }


    protected override void Connect()
    {
        unit.healingChain.Add(0, BlindingLight_Damage);

        unit.healing.onRecalculate += ()=>{ onRecalculated?.Invoke(); };
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


public class Ressurection : Talent // Debuged
{
    float mult = 1.5f;

    ArithmeticNode healingBonus;

    public Ressurection(Unit unit) : base(unit)
    {
        base.InitializeViewValues("Ressurection",
                                  $"Hero can ressurect Followers and healing is {mult-1f:P0} more powerful");

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
public class BattleExpirience : Talent // Debuged
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
                                  $"Followers do more damage based on respawn count");

        onDeserializedConcrete = ()=>
        {
            damageAddition.Mutation = damageMutation;
        };
    }

    public override string updatedDescription { get => $"\n({damageMutation})"; }

    protected override void Connect()
    {
        ApplyBonus();
        SoftReset.onRespawnCountChanged += ApplyBonus;
    }

    protected override void Disconnect()
    {
        SoftReset.onRespawnCountChanged -= ApplyBonus;
    }

    void ApplyBonus()
    {
        damageMutation = Mathf.Log(1 + SoftReset.respawnCount * .009f, 2) * 300;
        damageAddition.Mutation = damageMutation;

        onRecalculated?.Invoke();
    }

}

public class VeteransOfThirdWar : Talent // Debuged
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
                                  "");


        // progressProteryID = Shader.PropertyToID("_Progress");
        // hpBarMaterial = FollowersView._Inst.healthBar.image.material;
        // hpBarMaterial.SetFloat(progressProteryID, veteransRatio);
    }

    public override string updatedDescription { get =>
            $"While Followers' health < {veteransRatio:P0}, they recieve {armorMutation-1f:P0} armor bonus.\nBased on loyalty";
            }


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
        unit.takeDamageChain.Add(45, EngageVeterans);
    }

    override protected void Disconnect()
    {
        loyalty.onRecalculate -= RecalculateVeterans;
    }

    private void RecalculateVeterans()
    {
        CalculateThreshold();

        if (isVeteransEngaged) armorBonus.Mutation = armorMutation;

        // hpBarMaterial.SetFloat(progressProteryID, veteransRatio);

        onRecalculated?.Invoke();
    }
    private void CalculateThreshold()
    {
        float log = Mathf.Log(loyalty.Result + 2, 10) / 4;
        armorMutation = 1.4f + log;

        veteransRatio = Mathf.Min(1, log);
        armorBonus.Mutation = armorMutation;
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Rebirth : Talent   // Debuged
{
    Range health => unit.healthRange;

    StatMultChain perseverance;

    [JsonProperty]
    float maxRebirths, rebirths;

    public Rebirth(Hero hero) : base( hero)
    {
        perseverance = hero.perseverance;

        base.InitializeViewValues("Rebirth",
                                  $"Hero rebirth instead of dying\n"+
                                  $"Extra lives are based on perseverance.\n");
    }
    public override string updatedDescription { get =>
            $"Extra lives: {maxRebirths:F2}"; }

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
            rebirths > 0)
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
        maxRebirths = 1 + log * log / 6;
        onRecalculated?.Invoke();
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
public class Dejavu : Talent    // Debuged
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
                                  $"Hero's stats grow with each respawn.\n");

        InitFields();

        onDeserializedConcrete = () =>
        {
            ApplyBonus();
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
        SoftReset.onReset += RecalculateBonus;
    }

    protected override void Disconnect()
    {
    }
    
    void RecalculateBonus()
    {
        bonus = SoftReset.respawnCount * .0001f;
        ApplyBonus();
        onRecalculated?.Invoke();
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
                                  $"Followers' health increases based on respawn count");

        logMult = .02f;
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class TitansGrowth : Talent // Debug
{
    protected ArithmeticNode titansGrowthMult;

    public Timer recalculationTimer;

    [JsonProperty]
    protected float mult = 1;
    protected float logMult = .4f;


    public TitansGrowth(Unit unit) : base( unit)
    {
        recalculationTimer = new Timer(10);

        titansGrowthMult = ArithmeticNode.CreateMult();

        unit.health.chain.Add(1232, titansGrowthMult);

        base.InitializeViewValues("Titan's Growth",
                                  $"Hero's health increases based on respawn count");

        onDeserializedConcrete = ()=>
        {
            RecalculateHealthMult();
        };

    }
    public override string updatedDescription { get => $"\n{mult:P0}"; }

    protected void MainRecalculateHealthMult()
    {
        if (recalculationTimer.Tick())
        {
            RecalculateHealthMult();
        }
    }

    protected void RecalculateHealthMult()
    {
        mult = 1 + Mathf.Log(1 + SoftReset.respawnCount * .002f, 2) * logMult;

        titansGrowthMult.Mutation = mult;
        onRecalculated?.Invoke();
    }

    protected override void Connect()
    {
        unit.timeredActionsList.Add(MainRecalculateHealthMult);
        RecalculateHealthMult();
        SoftReset.onRespawnCountChanged += RecalculateHealthMult;
        SoftReset.onRespawnCountChanged += recalculationTimer.Reset;
    }

    protected override void Disconnect()
    {
        titansGrowthMult.Mutation = 1;
    }
}

public class Blitz : Talent
{
    float critChance = .15f;

    public Blitz() : base(Followers._Inst)
    {
        base.InitializeViewValues("Blitz",
                                  $"Followers can crit with {critChance:P0} chance\ndealing {unit.critMult.Result:P0} damage");
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

public class CounterAttack : Talent // Debuged
{
    float chance = .2f;
    float damageMult = 1f;


    public CounterAttack(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Counter Attack",
                                  $"{chance:P0} chance to counterattack\n"+
                                  "each time Hero or Followers take damage");
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
            unit.makeAttack(unit.damage.Result * damageMult);
    }
}

public class Cyclone : Talent   //Debuged
{
    ArithmeticNode cycloneMult;

    float mult = .15f;
    
    public Cyclone(Unit unit) : base(unit)
    {

        cycloneMult = ArithmeticNode.CreateMult();

        unit.attackSpeed.chain.Add(cycloneMult);


        base.InitializeViewValues("Cyclone",
                                  $"Hero attacks {mult:P0} faster");

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

public class EnfeeblingStrike : Talent // Debuged
{
    float mult = .85f;

    bool isEnemyEnfeebled ;

    public EnfeeblingStrike(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Enfeeble",
                                  $"");

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


public class Multicrit : Talent // Debuged
{
    float critChanceLimitVal = 5f;

    public Multicrit(Unit unit) : base( unit)
    {
        base.InitializeViewValues("Multicrit",
                                  $"Hero's critical chance limit = {critChanceLimitVal:P0}.\n"
                                  +"Damage bonus from multiple crits stacks additively");
    }
    public override string updatedDescription { get => $""; }


    protected override void Connect()
    {
        unit.critChance.growthLimit.Mutation = critChanceLimitVal;
    }

    protected override void Disconnect()
    {
        unit.critChance.growthLimit.Mutation = 1f;
    }

}

public class Salvation : Talent
{
    float
        mult = .75f,
        heroRatio,
        followersRatio;
    ArithmeticNode speedMult;

    public Salvation() : base(Hero._Inst)
    {
        base.InitializeViewValues("Salvation",
                                  $"When Hero's or Followers' health < 50% healing speed +{1-mult:P0}");

        unit.healSpeed.chain.Add(speedMult = ArithmeticNode.CreateMult());
    }

    public override string updatedDescription => string.Empty;


    protected override void Connect()
    {
        unit.healthRange.onRatioChanged += CheckHero;
        unit.followers.healthRange.onRatioChanged += CheckFollowers;
    }

    void CheckHero(float ratio)
    {
        heroRatio = ratio;
        ApplyBonus();
    }
    void CheckFollowers(float ratio)
    {
        followersRatio = ratio;
        ApplyBonus();
    }

    void ApplyBonus()
    {
        if (heroRatio <= .5f ||
            followersRatio <= .5f)
        {
            speedMult.Mutation = mult;
        }
        else
        {
            speedMult.Mutation = 1f;
        }
    }
}
