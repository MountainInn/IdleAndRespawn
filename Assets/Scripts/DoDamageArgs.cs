
public class DoDamageArgs
{
    public Unit attacker {get; private set;}
    public Unit target => attacker.target;
    public Range damage;
    public bool
        isReflected,
        isCritical,
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

        isReflected = false;
        isCritical = false;
        isBloodMadness = false;
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
