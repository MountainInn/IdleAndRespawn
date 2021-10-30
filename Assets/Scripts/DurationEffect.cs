using UnityEngine;

abstract public class DurationEffect
{
    public float duration;

    public Unit unit;

    public DurationEffect(float duration)
    {
        this.duration = duration;
    }


    public void Start(Unit unit)
    {
        this.unit = unit;

        TakeEffect();

        unit.StartCoroutine(CoroutineExtension.InvokeAfter(Dispell, duration));
    }

    abstract protected void TakeEffect();

    abstract protected void Dispell();
}
