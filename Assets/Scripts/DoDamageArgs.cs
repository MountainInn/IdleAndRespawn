
public class DoDamageArgs
{
    public Unit attacker {get; private set;}
    public Unit target => attacker.target;
    public Range damage;

    public bool IsSimpleAttack => !(isJudgement || isDiversion || isBlindingLight || isHotHanded || isReflected || isDoom || isBloodMadness);

    public bool
        isCritical,

        isDiversion,
        isBlindingLight,

        isReflected,
        isJudgement,

        isBloodMadness,
        isHotHanded,
        isDoom;

    public bool
        eatTheWeak,
        sharpSpikes
        ;

    public DoDamageArgs(Unit attacker, float damage)
    {
        this.attacker = attacker;
        this.damage = new Range(damage, float.MaxValue);
    }

    public DoDamageArgs CopyShallow()
    {
        return (DoDamageArgs)this.MemberwiseClone();
    }

    static public DoDamageArgs CreateReflected(Unit attacker, float damage)
    {
        return new DoDamageArgs(attacker, damage)
        {
            isReflected = true
        };
    }
}
