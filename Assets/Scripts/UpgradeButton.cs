using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Button))]
public class UpgradeButton : Register<UpgradeButton>
{
    [SerializeField] Unit unit;
    [SerializeField] string upgradeName;

    [SerializeField]
    Text
        levelText,
        nextLevelText,
        valText,
        costText;

    public Button self;
    public StatMultChain stat;

    int targetLevelIncrease => ShoppingCart._BuyLevelQuantity;
    StatMultChain FindUpgradeField<T>(T unit)
        where T : Unit
    {
        return (StatMultChain) unit.GetType()
            .GetField(upgradeName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(unit);
    }

    void Awake()
    {
        RegisterSelf();
    }

    void Start()
    {
        self = GetComponent<Button>();


        stat = FindUpgradeField(unit);
        
        if (stat == null) Debug.LogWarning($"From [{gameObject.name}] Upgrade {upgradeName} is null!");



        self.onClick.AddListener(() =>
        {
            stat.LevelUp(Vault.Expirience, stat.maxAffordableLevel);

            UpdateInteractable();
        });

        
        stat.chain.onRecalculateChain += ()=>
        {


            DisplayVal();


            CheckLimits();
        };

        if (stat.growthLimit != null)
            stat.growthLimit.onMutationUpdated += () =>
            {
                CheckLimits();
            };



        DisplayVal();
        UpdateInteractable();

        Vault.Expirience.onChanged_Amount += (change) => {

            UpdateInteractable();
        };

        ShoppingCart.onChangedBuyLevelQuantity += ()=>
        {
            UpdateInteractable();
        };

        nextLevelText.gameObject.SetActive(false);
    }

    void CheckLimits()
    {
        if (stat.isLimitReached)
        {
            strNextCost = "Max";
        }
    }


    string
        strVal,
        strNextVal,
        strNextCost,
        strLevel, strNextLevel;

    void DisplayNextVal()
    {
        valText.text = strNextVal;
    }

    public void UpdateInteractable()
    {
        stat.CalculateMaxAffordableLevel2(targetLevelIncrease, out bool canAfford);

        self.interactable = canAfford;

        costText.color = (canAfford) ? green : red;

        DisplayNextLevel();
        DisplayVal();
    }

    void DisplayNextLevel()
    {
        strNextLevel = stat.maxAffordableLevel.ToString();
        nextLevelText.text = "+"+strNextLevel;
        nextLevelText.gameObject.SetActive(true);
    }

    void DisplayVal()
    {
        valText.text = strVal;

        levelText.text = strLevel;
    }


    void MakeStrings()
    {
        strVal =
            (stat.isPercentage)
            ? stat.Result.ToString("P0")
            : stat.Result.ToStringFormatted();

        strLevel = stat.level.ToString();
    }


    Color
        green = Color.green,
        red = Color.red;

}
