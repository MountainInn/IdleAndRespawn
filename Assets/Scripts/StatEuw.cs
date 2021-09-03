using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class StatEuw : Euw<StatEuw>
{
    static public List<StatEuw> instances = new List<StatEuw>();

    [SerializeField] Unit unit;
    [SerializeField] string upgradeName;

    [SerializeField]
    Text
        level,
        levelAddition,
        value,
        cost;

    public Button button;
    public StatMultChain stat;

    int targetLevelIncrease => ShoppingCart._BuyLevelQuantity;

    public EuwState<StatEuw>
        checkingVaultState = new StateCheckingVault(),
        limitReachedState = new StateLimitReached();

    void Awake()
    {
        instances.Add(this);
    }

    void Start()
    {
        stat = FindUpgradeField(unit);

        checkingVaultState.Setup(this);
    }

    StatMultChain FindUpgradeField<T>(T unit) where T : Unit
    {
        return (StatMultChain) unit.GetType()
            .GetField(upgradeName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(unit);
    }


    public class StateCheckingVault : EuwState<StatEuw>
    {
        protected override void ConcreteSetup()
        {
            UpdateInteractable();

            Vault.expirience.onChanged += UpdateInteractable;
            
            ShoppingCart.onChangedBuyLevelQuantity += UpdateInteractable;

            view.stat.chain.onRecalculateChain += UpdateAfterBuy;

            view.button.onClick.AddListener(LevelupStat);

            if (view.stat.limit != null)
                view.stat.limit.onMutationUpdated += CheckLimit; 
        }

        protected override void Uninstall()
        {
            Vault.expirience.onChanged -= UpdateInteractable;

            ShoppingCart.onChangedBuyLevelQuantity -= UpdateInteractable;

            view.stat.chain.onRecalculateChain -= UpdateAfterBuy;

            view.button.onClick.RemoveListener(LevelupStat);

            if (view.stat.limit != null)
                view.stat.limit.onMutationUpdated -= CheckLimit; 
        }

        public override void Update() {}

        void LevelupStat()
        {
            view.stat.LevelUp(Vault.expirience, view.stat.maxAffordableLevel);

            // Возможно колбэк на expirience.OnChanged здесь справится
            // UpdateInteractable();
        }

        public void UpdateInteractable()
        {
            view.stat.CalculateMaxAffordableLevel(view.targetLevelIncrease, out bool canAfford);

            view.levelAddition.text = "+" + view.stat.maxAffordableLevel.ToString();

            view.button.interactable = canAfford;

            view.cost.color = (canAfford) ? Color.green : Color.red;
        }

        void UpdateAfterBuy()
        {
            view.value.text =
                (view.stat.isPercentage)
                ? view.stat.Result.ToString("P0")
                : view.stat.Result.ToStringFormatted();

            view.level.text = view.stat.level.ToString();
        }

        void CheckLimit()
        {
            if (view.stat.isLimitReached)
            {
                view.limitReachedState.Setup(view);
            }
        }
    }
    public class StateLimitReached : EuwState<StatEuw>
    {
        public override void Update()
        {}

        protected override void ConcreteSetup()
        {
            view.button.interactable = false;
            
            view.stat.limit.onMutationUpdated += CheckLimit;
        }
        protected override void Uninstall()
        {
            view.stat.limit.onMutationUpdated -= CheckLimit;
        }

        void CheckLimit()
        {
            if (!view.stat.isLimitReached)
            {
                view.checkingVaultState.Setup(view);
            }
        }
    }
}


public abstract class EuwState<T> where T:Euw<T>
{
    public T view;

    public void Setup(T view)
    {
        this.view = view;

        view.currentState?.Uninstall();
        view.currentState = this;

        ConcreteSetup();
    }

    abstract public void Update();

    abstract protected void ConcreteSetup();

    abstract protected void Uninstall();
}

abstract public class Euw<T> : MonoBehaviour where T:Euw<T>
{
    public EuwState<T> currentState;
}
