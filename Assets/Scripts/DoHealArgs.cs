
public class DoHealArgs
{
    public Unit healer {get; private set;}
    public Unit followers => healer.followers;

    public float heal;
    

    public DoHealArgs(Unit healer, float heal)
    {
        this.healer = healer;
        this.heal = heal;
    }

}
