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
    static public Currency
        expirience,
        talentPoints,
        soulshard,
        soulEnergy;
    [SerializeField]
    Text
        expirienceView,
        talentPointsView,
        soulshardView,
        soulenergyView;
    [SerializeField]
    FloatingTextMaker
        expEarn,
        talEarn,
        soulshardEarn,
        soulEnergyEarn;
    [SerializeField, SpaceAttribute]
    GameObject
        soulshardIncome,
        bossSoulsViewParent;

    void Awake()
    {
        expirience = new Currency(0);
        talentPoints = new Currency(1000);
        soulshard = new Currency(0);

        InitView(expirienceView, expEarn, expirience);
        InitView(talentPointsView, talEarn, talentPoints);

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
        InitView(soulshardView, soulshardEarn, soulshard);
        InitView(soulenergyView, soulEnergyEarn, soulEnergy);

        soulshardIncome.SetActive(true);
    }

}
