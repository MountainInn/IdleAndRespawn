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


    new public static List<Hero> _Instances = new List<Hero>();


    protected override void OnAwake()
    {
        _Instances.Add(this);

        target = Boss._Inst;
        followers = Followers._Inst;

        new Healing(this);

        barrierRange = new Range(0);
    }

    [JsonPropertyAttribute]
    public StatMultChain
        perseverance,
        loyalty ;

    override protected void FirstInitStats()
    {
        damage = new StatMultChain(25, 5, 100);

        attackSpeed = new StatMultChain(1, 0, 0){isPercentage = true};

        critChance = new StatMultChain(.05f, 0.01f, 400, limitVal:1f){ isPercentage = true };

        critMult = new StatMultChain(1.2f, .01f, 400, limitVal:1.5f){ isPercentage = true };

        reflect = new StatMultChain(3, 3, 200);

        armor = new StatMultChain(5, 5, 200);

        InitHealth(500, 200, 550);

        healing = new StatMultChain(20, 4, 100);

        healSpeed = new StatMultChain(2, 0, 0);

        vampirism = new StatMultChain(.1f, .035f, 800, limitVal:6f){ isPercentage = true };

        perseverance = new PerseveranceStat(0, 1, 10_000);

        loyalty = new LoyaltyStat(0, 1, 10_000);
    }

    public string StatsString() =>
        "Damage " + damage +
        "CritChance " + critChance +
        "CritMult " + critMult +
        "Armor " + armor +
        "Health " + health;


}

