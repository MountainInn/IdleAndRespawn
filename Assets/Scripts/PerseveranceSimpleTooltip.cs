using System;
using UnityEngine;

public class PerseveranceSimpleTooltip : SimpleTooltipTarget
{
    PerseveranceStat perseverance;

    new protected void Awake()
    {
        base.Awake();

        perseverance = Hero._Inst.perseverance as PerseveranceStat;

        tooltip = GameObject.FindObjectOfType<SimpleTooltip>();

        var simpleTooltip = tooltip as SimpleTooltip;

        
        onPointerEnter = () => simpleTooltip.SetContent(content + $"\nBonus: ({perseverance.mutation-1f:P2})");
    }
}
