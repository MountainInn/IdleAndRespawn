using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Followers : Unit
{
    static Followers inst;
    static public Followers _Inst => inst??=GameObject.FindObjectOfType<Followers>();


    new public static  List<Followers> _Instances = new List<Followers>();

    protected override void OnAwake()
    {
        _Instances.Add(this);


        target = GameObject.FindObjectOfType<Boss>();

        new CriticalHit(this);
        new TakeDamageArmor(this);
        new TakeDamageHealth(this);
        new TakeHeal(this);

    }

    void Start()
    {
    }


    protected override void FirstInitStats()
    {
        damage = new StatMultChain(100, 7, 400);

        attackSpeed = new StatMultChain(2, 0, 0){ isPercentage = true };

        InitHealth(100, 100, 1000);

        armor = new StatMultChain(0, 1, 50);

        reflect = new StatMultChain(0, 0, 0);

        critChance = new StatMultChain(0f, 0f, 500) { isPercentage = true };

        critMult = new StatMultChain(1.6f, .0f, 500){ isPercentage = true };

    }

}
