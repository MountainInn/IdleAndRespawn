using System.Collections.Generic;
using UnityEngine;
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
    }

    void Start()
    {
    }


    protected override void FirstInitStats()
    {
        damage = new StatMultChain(10, 10, 500);

        attackSpeed = new StatMultChain(3.5f, 0, 0){ isPercentage = true };

        InitHealth(200, 100, 600);

        armor = new StatMultChain(0, 1, 50);

        critChance = new StatMultChain(.01f, 0.01f, 500){ isPercentage = true };

        critMult = new StatMultChain(1.2f, .01f, 500){ isPercentage = true };

    }

}
