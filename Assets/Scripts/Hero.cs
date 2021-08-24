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
    }

    [JsonPropertyAttribute]
    public StatMultChain
        perseverance,
        loyalty ;

    override protected void FirstInitStats()
    {
        damage = new StatMultChain(5, 5, 100);


        attackSpeed = new StatMultChain(2, 0, 0){isPercentage = true};

        critChance = new StatMultChain(.05f, 0.01f, 500){ isPercentage = true };
        critChance.SetLimit(1f);

        critMult = new StatMultChain(1.2f, .01f, 500){ isPercentage = true };
        critMult.SetLimit(1.5f);

        reflect = new StatMultChain(.05f, .01f, 500){ isPercentage = true };

        armor = new StatMultChain(5, 5, 500);

        InitHealth(500, 100, 600);

        healing = new StatMultChain(5, 3, 50);

        healSpeed = new StatMultChain(4, 0, 0);

        vampirism = new StatMultChain(.1f, .035f, 400){ isPercentage = true };

        perseverance = new PerseveranceStat(0, 1, 1e6f);

        loyalty = new LoyaltyStat(0, 1, 20000);
    }

    public string StatsString() =>
        "Damage " + damage +
        "CritChance " + critChance +
        "CritMult " + critMult +
        "Armor " + armor +
        "Health " + health;


}

