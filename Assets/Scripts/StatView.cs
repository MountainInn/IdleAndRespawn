using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class StatView : ViewClass
{
    static public List<StatView> instances = new List<StatView>();

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

    public State
        checkingVaultState,
        limitReachedState;

    void Awake()
    {
        instances.Add(this);
    }

    void Start()
    {
        stat = FindUpgradeField(unit);

        checkingVaultState = new StateCheckingVault(this);
        limitReachedState = new StateLimitReached(this);

        SwitchState(checkingVaultState);
    }

    StatMultChain FindUpgradeField<T>(T unit) where T : Unit
    {
        return (StatMultChain) unit.GetType()
            .GetField(upgradeName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(unit);
    }


    public abstract class State : ViewState
    {
        new public StatView view;

        public State(StatView view) : base(view)
        {
            this.view = view;
        }

        protected void UpdateValue()
        {
            view.value.text =
                (view.stat.isPercentage)
                ? view.stat.Result.ToString("P0")
                : view.stat.Result.ToStringFormatted();
        }
    }

    public class StateCheckingVault : State
    {
        public StateCheckingVault(StatView view) : base(view)
        {
            this.view = view;
        }

        public override void Setup()
        {
            UpdateInteractable();

            ShoppingCart.onChangedBuyLevelQuantity += UpdateInteractable;
            Vault.expirience.onChanged += UpdateInteractable;
            view.stat.chain.onRecalculateChain += UpdateLevel;
            view.stat.chain.onRecalculateChain += UpdateValue;
            view.button.onClick.AddListener(LevelupStat);
        }

        public override void Uninstall()
        {
            ShoppingCart.onChangedBuyLevelQuantity -= UpdateInteractable;
            Vault.expirience.onChanged -= UpdateInteractable;
            view.stat.chain.onRecalculateChain -= UpdateLevel;
            view.stat.chain.onRecalculateChain -= UpdateValue;
            view.button.onClick.RemoveListener(LevelupStat);
        }

        void LevelupStat()
        {
            view.stat.LevelUp(Vault.expirience, view.stat.maxAffordableLevel);

            UpdateInteractable();

            CheckLimit();
        }

        public void UpdateInteractable()
        {
            view.stat.CalculateMaxAffordableLevel_2(view.targetLevelIncrease, out bool canAfford);

            view.levelAddition.text = "+" + view.stat.maxAffordableLevel.ToString();

            view.button.interactable = canAfford;

            view.cost.text = view.stat.maxAffordableCost.ToStringFormatted();
            view.cost.color = (canAfford) ? Color.green : Color.red;
        }

        void UpdateLevel()
        {
            view.level.text = view.stat.level.ToString();
        }

        void CheckLimit()
        {
            if (view.stat.isLimitReached)
            {
                view.SwitchState(view.limitReachedState);
            }
        }
    }
    public class StateLimitReached : State
    {
        public StateLimitReached(StatView view) : base(view)
        {
            this.view = view;
        }

        public override void Setup()
        {
            TurnButtonOff();

            view.levelAddition.text = string.Empty;
            view.cost.text = "MAX LVL";
            view.cost.color = Color.blue;

            view.stat.limitGrowth.onMutationUpdated += CheckLimit;

            view.stat.chain.onRecalculateChain += UpdateValue;
        }
        public override void Uninstall()
        {
            view.stat.limitGrowth.onMutationUpdated -= CheckLimit;

            view.stat.chain.onRecalculateChain -= UpdateValue;
        }

        private void TurnButtonOff()
        {
            Color normalColor = view.button.colors.normalColor;

            view.button.interactable = true;
            view.button.transition = Button.Transition.None;
            view.button.image.color = normalColor;
        }

        void CheckLimit()
        {
            if (!view.stat.isLimitReached)
            {
                view.SwitchState(view.checkingVaultState);
            }
        }
    }
}

public abstract class ViewClass : MonoBehaviour
{
    public ViewState currentState;

    public void SwitchState(ViewState state)
    {
        currentState?.Uninstall();
        currentState = state;
        currentState.Setup();
    }
}

public abstract class ViewState
{
    public ViewClass view;

    public ViewState(ViewClass view)
    {
        this.view = view;
    }

    abstract public void Setup();
    abstract public void Uninstall();
}


// public abstract class ViewClass<ViewT, ViewStateT> : MonoBehaviour
//     where ViewT : ViewClass<ViewT, ViewStateT>
//     where ViewStateT : ViewState<ViewStateT, ViewT>
// {
//     public ViewStateT currentState;
// }

// public abstract class ViewState<ViewStateT, ViewT>
//     where ViewStateT : ViewState<ViewStateT, ViewT>
//     where ViewT : ViewClass<ViewT, ViewStateT>
// {
//     public ViewT view;

//     public void Setup(ViewT view)
//     {
//         this.view = view;

//         view.currentState?.Uninstall();
//         view.currentState = (ViewStateT)this;

//         ConcreteSetup();
//     }
//     abstract protected void ConcreteSetup();
//     abstract protected void Uninstall();
// }
