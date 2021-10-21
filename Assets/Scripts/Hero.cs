using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public partial class Hero : Unit
{
    static Hero inst;
    static public Hero _Inst => inst??=GameObject.FindObjectOfType<Hero>();

    static public Action<int> onFragsUpdated;
    int _frags;
    [JsonPropertyAttribute]
    public int frags {
        get => _frags;
        set
        {
            _frags = value;

            onFragsUpdated.Invoke(_frags);
        }
    }

    new public static List<Hero> _Instances = new List<Hero>();

    [OnDeserializedAttribute]
    new public void OnDeserialized(StreamingContext context)
    {
        base.OnDeserialized(context);
        // if (frags > 0) Vault.ActivateBossSoulsView();
    }

    protected override void OnAwake()
    {
        _Instances.Add(this);

        target = Boss._Inst;
        followers = Followers._Inst;

        new TakeDamageBarrier();
        new TakeDamageFollowers(this);
        new TakeDamageArmor(this);
        new TakeDamageHealth(this);

        new Healing(this);
        new CriticalHit(this);
        new TakeHeal(this);

        barrierRange = new Range(0);
    }

    [JsonPropertyAttribute]
    public StatMultChain
        perseverance,
        loyalty ;

    override protected void FirstInitStats()
    {
        damage = new StatMultChain(36, 6, 200);

        attackSpeed = new StatMultChain(1.5f, 0, 0){isPercentage = true};

        critChance = new StatMultChain(.05f, 0.01f, 400, limitVal:1f){ isPercentage = true };

        critMult = new StatMultChain(1.2f, .01f, 400, limitVal:1.5f){ isPercentage = true };

        reflect = new StatMultChain(3, 3, 200);

        armor = new StatMultChain(10, 8, 320);

        InitHealth(100, 200, 1000);

        healing = new StatMultChain(20, 7, 340);

        healSpeed = new StatMultChain(2.1f, 0, 0);

        vampirism = new StatMultChain(.1f, .035f, 800, limitVal:1.5f){ isPercentage = true };

        perseverance = new PerseveranceStat(0, 1, 15_000);

        loyalty = new LoyaltyStat(0, 1, 15_000);
    }

    public string StatsString() =>
        "Damage " + damage +
        "CritChance " + critChance +
        "CritMult " + critMult +
        "Armor " + armor +
        "Health " + health;


}
