using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField] Unit unit;
    [SerializeField] string upgradeName;

    [SerializeField]
    Text
        levelText,
        nextLevelText,
        valText,
        costText;

    Button self;
    StatMultChain stat;

    int targetLevelIncrease => ShoppingCart._BuyLevelQuantity;
    StatMultChain FindUpgradeField<T>(T unit)
        where T : Unit
    {
        return (StatMultChain) unit.GetType()
            .GetField(upgradeName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(unit);
    }

    void Start()
    {
        self = GetComponent<Button>();


        stat = FindUpgradeField(unit);
        
        if (stat == null) Debug.LogWarning($"From [{gameObject.name}] Upgrade {upgradeName} is null!");



        self.onClick.AddListener(() =>
        {
            stat.LevelUp(Vault.expirience, stat.maxAffordableLevel);

            UpdateInteractable();
        });

        
        stat.chain.onRecalculateChain += ()=>
        {
            CacheStrings();
            CacheNextStrings();
            DisplayVal();


            CheckLimits();
        };

        if (stat.limit != null)
            stat.limit.onMutationUpdated += () =>
            {
                CheckLimits();
            };

        CacheStrings();
        CacheNextStrings();
        DisplayVal();
        UpdateInteractable();

        Vault.expirience.onChanged += (change) => {
            CacheNextStrings();
            DisplayVal();
            UpdateInteractable();
        };

        ShoppingCart.onChangedBuyLevelQuantity += ()=>
        {
            CacheNextStrings();
            DisplayVal();
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

    void DisplayNextLevel()
    {
        strNextLevel = stat.maxAffordableLevel.ToString();
        nextLevelText.text = "+"+strNextLevel;
        nextLevelText.gameObject.SetActive(true);
    }

    void DisplayVal()
    {
        valText.text = strVal;

        costText.text = strNextCost;

        levelText.text = strLevel;
    }


    void CacheStrings()
    {
        if (stat.isPercentage) strVal = stat.Result.ToString("P0");
        else if (stat.Result < 1000) strVal = stat.Result.ToString("N0");
        else strVal = stat.Result.ToString("e2");

        strLevel = stat.level.ToString();
    }

    void CacheNextStrings()
    {
        strNextVal = stat.GetValForNextLevel(targetLevelIncrease).ToString();

        strNextCost = FloatExt.BeautifulFormat((stat.maxAffordableLevel > 0) ? stat.maxAffordableCost : stat.GetCostForNextLevel(1) );
    }


    Color
        green = Color.green,
        red = Color.red;
    
    void UpdateInteractable()
    {
        stat.CalculateMaxAffordableLevel(targetLevelIncrease, out bool canAfford);
        
        self.interactable = canAfford;

        costText.color = (canAfford) ? green : red;

        CacheNextStrings();
        DisplayNextLevel();
        DisplayVal();
    }




}
