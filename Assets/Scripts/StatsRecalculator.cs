using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

public class StatsRecalculator : MonoBehaviour
{
    Unit unit;

    void Start()
    {
        unit = GetComponent<Unit>();

        if (unit is Hero hero) RecalculateAllStats(hero.GetType(), hero);

        else if (unit is Followers followers) RecalculateAllStats(followers.GetType(), followers);

        else if (unit is Boss boss) RecalculateAllStats(boss.GetType(), boss);
    }


    void RecalculateAllStats(Type unitSubtype, Unit unitObj)
    {
        foreach (var stat in
                 from s in unitSubtype.GetFields(BindingFlags.Public | BindingFlags.Instance)
                 where s.FieldType == typeof(StatMultChain)
                 select (StatMultChain) s.GetValue(unitObj))
        {
            if (stat != null)
                stat.chain.RecalculateChain();
        }

    }
}
