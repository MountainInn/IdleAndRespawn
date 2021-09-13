using UnityEngine;

public class LoyaltySimpleTooltip : SimpleTooltipTarget
{
    LoyaltyStat loyalty;

    new protected void Awake()
    {
        base.Awake();

        loyalty = Hero._Inst.loyalty as LoyaltyStat;

        tooltip = GameObject.FindObjectOfType<SimpleTooltip>();

        var simpleTooltip = tooltip as SimpleTooltip;



        onPointerEnter = () => simpleTooltip.SetContent(content +
                                                        $"\nBonus Health: ({LoyaltyStat.healthAddition.Mutation.ToStringFormatted()})" +
                                                        $"\nBonus Armor: ({LoyaltyStat.armorAddition.Mutation.ToStringFormatted()})");
    }

}
