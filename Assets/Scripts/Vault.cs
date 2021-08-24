using System;
using UnityEngine;
using UnityEngine.UI;

public class Vault : MonoBehaviour
{
    static public Currency
        expirience,
        talentPoints,
        bossSouls;

    [SerializeField] Text expirienceView, talentPointsView, bossSoulsView;
    [SerializeField] FloatingTextMaker expEarn, talEarn, bossSoulsEarn;

    void Awake()
    {
        expirience = new Currency(0);
        talentPoints = new Currency(200000);
        bossSouls = new Currency(0);

        InitView(expirienceView, expEarn, expirience);
        InitView(talentPointsView, talEarn, talentPoints);
        InitView(bossSoulsView, bossSoulsEarn, bossSouls);
    }

    void InitView(Text view, FloatingTextMaker earnText, Currency currency)
    {
        view.text = currency._Val.ToString();

        currency.onChanged += (change) =>
        {
            view.text = FloatExt.BeautifulFormatSigned(currency._Val);

            earnText.SpawnText(FloatExt.BeautifulFormatSigned(change), Color.white);
        };
    }

}
