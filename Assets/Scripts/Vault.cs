using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Vault : MonoBehaviour
{
    static Vault inst;
    static public Vault _Inst => inst??=GameObject.FindObjectOfType<Vault>();

    [JsonPropertyAttribute]
    public Currency
        expirience,
        talentPoints,
        soulshard,
        soulEnergy;
    static public Currency Expirience => _Inst.expirience;
    static public Currency TalentPoints => _Inst.talentPoints;
    static public Currency Soulshard => _Inst.expirience;
    static public Currency SoulEnergy => _Inst.expirience;

    [SerializeField]
    Text
        expirienceView,
        talentPointsView,
        soulshardView,
        soulenergyView;
    [SerializeField]
    FloatingTextMaker
        expEarn,
        talentEarn;
    [SerializeField, SpaceAttribute]
    GameObject
        soulshardIncome,
        bossSoulsViewParent;

    void Awake()
    {
        expirience = new Currency(0);
        talentPoints = new Currency(0);
        soulshard = new Currency(0);

        InitView(expirienceView, expEarn, Expirience);
        InitView(talentPointsView, talentEarn, TalentPoints);

        bossSoulsViewParent.SetActive(false);
    }

    void Start()
    {
    }

    void InitView(Text view, FloatingTextMaker earnText, Currency currency)
    {
        view.text = currency._Val.ToString();

        currency.onChanged += ()=>
        {
            view.text = currency._Val.ToStringFormatted();
        };

        currency.onChanged_Amount += (change) =>
        {
            earnText.SpawnText(change.ToStringFormatted(), Color.white);
        };
    }

    static public void ActivateBossSoulsView()
    {
        if (!_Inst.bossSoulsViewParent.activeSelf)
        {
            _Inst.bossSoulsViewParent.SetActive(true);

            _Inst.InitializeSouls();
        }
    }

    void InitializeSouls()
    {
        InitView(soulshardView, null, Soulshard);
        InitView(soulenergyView, null, SoulEnergy);

        soulshardIncome.SetActive(true);
    }

}
