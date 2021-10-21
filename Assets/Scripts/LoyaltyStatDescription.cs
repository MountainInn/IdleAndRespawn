
public class LoyaltyStatDescription : UpdatableStatDescription
{
    new LoyaltyStat stat;
    void Start()
    {
        stat = LoyaltyStat._Inst;
    }
    override protected string updatableDescription =>
        $"\nHealth +{stat.healthAddition.Mutation.ToStringFormatted()}" +
        $"\nArmor +{stat.armorAddition.Mutation.ToStringFormatted()}";
}
