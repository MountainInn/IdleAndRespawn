using System;

public class Vendible
{
    public float price;

    public Action onBought;

    protected Currency currency;

    public Vendible(Currency currency)
    {
        this.currency = currency;
    }

    virtual public void Buy()
    {
        if (CanBuy())
        {
            OnBought();

            onBought?.Invoke();
        }
    }

    virtual public bool CanBuy()
    {
        return (currency >= price);
    }

    virtual protected void OnBought(){}
}
