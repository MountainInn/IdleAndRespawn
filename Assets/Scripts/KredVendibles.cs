using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class KredVendibles : MonoBehaviour
{
    static KredVendibles inst;
    static public KredVendibles _Inst => inst??=GameObject.FindObjectOfType<KredVendibles>();

    static public Currency Kredits;

    [SerializeField] KredVendibleView prefKredVendibleView;
    [SerializeField] Sprite hereditaryEquipmentIcon, combatTrainingIcon, wardAgainstEvilIcon;

    Pool<KredVendibleView> kredVendiblePool;

    KredVendible
        hereditaryEquipment,
        combatTraining ,
        wardAgainstEvil ;

    void Awake()
    {
        Kredits = new Currency(1000);

        kredVendiblePool = new Pool<KredVendibleView>(transform, prefKredVendibleView, 3);

        hereditaryEquipment = new HereditaryEquipment().SetIcon(hereditaryEquipmentIcon);
        combatTraining = new CombatTraining().SetIcon(combatTrainingIcon);
        wardAgainstEvil = new WardAgainstEvil().SetIcon(wardAgainstEvilIcon);


        kredVendiblePool.Acquire().ConnectVendible(hereditaryEquipment);
        kredVendiblePool.Acquire().ConnectVendible(combatTraining);
        kredVendiblePool.Acquire().ConnectVendible(wardAgainstEvil);
    }


    public void OnInventoryChecked(object itemsList)
    {

    }

    public void OnVendiblesReady()
    {
        foreach (var item in kredVendiblePool.EachActiveObject())
        {
            item.UpdateView();
        }
    }

<<<<<<< HEAD
=======


>>>>>>> c870829 (Переписал Talent чтобы он включал в себя Vendible)
    abstract public class KredVendible : OneTimeVendible
    {
        public string name, description;
        public Sprite icon;

        public KredVendible SetIcon(Sprite icon)
        {
            this.icon = icon;
            return this;
        }

        public KredVendible(string name, string description, int price)
        {
            this.name = name; this.description = description; this.price = price;
        }
    }


    abstract public class ArmorDamageMultiplierKredVendible : KredVendible
    {
        protected const float mult = 1;

        ArithmeticNode armorMult, damageMult;
        Unit unit ;

        protected ArmorDamageMultiplierKredVendible(Unit unit, string unitName, string goodName, int price)
            : base($"{goodName}",
                   $"{unitName}'s armour x{mult:F2}\n"+
                   $"{unitName}'s damage x{mult:F2}",
                   price)
        {
            armorMult = ArithmeticNode.CreateMult(mult);
            damageMult = ArithmeticNode.CreateMult(mult);


        }

        protected override void OnBought()
        {
            unit.armor.chain.Add(2000, armorMult);
            unit.damage.chain.Add(2000, damageMult);
        }
    }


    public class HereditaryEquipment : ArmorDamageMultiplierKredVendible
    {
        new const float mult = 1.25f;

        public HereditaryEquipment() : base(Hero._Inst, "Hero", "Hereditary Equipment", 100) {}
    }

    public class CombatTraining : ArmorDamageMultiplierKredVendible
    {
        new const float mult = 1.25f;

        public CombatTraining() : base(Followers._Inst, "Followers", "Combat Training", 100) {}
    }

    public class WardAgainstEvil : ArmorDamageMultiplierKredVendible
    {
        new const float mult = .75f;

        public WardAgainstEvil() : base(Boss._Inst, "Boss", "Ward Against Evil", 100) {}
    }



}

public class Vendible
{
    public float price;

    public Action onBought;


    virtual public void Buy(Currency currency)
    {
        if (CanBuy(currency))
        {
            OnBought();

            onBought?.Invoke();
        }
    }

    virtual public bool CanBuy(Currency currency)
    {
        return (currency >= price);
    }

    virtual protected void OnBought(){}
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class OneTimeVendible : Vendible
{
    [JsonPropertyAttribute]
    public bool isOwned;

    public OneTimeVendible()
    {
    }

    [OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext streamingContext)
    {
        if (isOwned)
        {
            OnBought();
            onBought?.Invoke();
        }
    }

    override public void Buy(Currency currency)
    {
        if (CanBuy(currency))
        {
            isOwned = true;

            OnBought();

            onBought?.Invoke();
        }
    }

    override public bool CanBuy(Currency currency)
    {
        return !isOwned && (currency >= price);
    }
}
