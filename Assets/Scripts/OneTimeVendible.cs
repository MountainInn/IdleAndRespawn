using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class OneTimeVendible : Vendible
{
    [JsonPropertyAttribute]
    public bool isOwned;

    public OneTimeVendible(Currency currency) : base(currency)
    {
    }

    override public void Buy()
    {
        if (CanBuy())
        {
            isOwned = true;

            OnBought();

            onBought?.Invoke();
        }
    }

    override public bool CanBuy()
    {
        return !isOwned && (currency >= price);
    }
}
