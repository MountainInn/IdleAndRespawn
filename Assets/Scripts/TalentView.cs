using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;


[RequireComponent(typeof(TooltipTarget))]
public class TalentView : ViewClass
{
    static public List<TalentView> instances = new List<TalentView>();

    [SaveField] bool isNewopened = true;

    [SerializeField]
    Image newopenFlareImage, checkmark;
    public Button button;

    TooltipTarget tooltipTarget;
    TalentTooltip talentTooltip;
    EventTrigger eventTrigger;


    public Talent thisTalent;


    public State
        coveredState,
        discoveredState,
        boughtState;

    public Action<float> updateOnCurrencyChanged;
    public Action updateOnPointerEnter;

    public void ConnectToTalent(Talent tal)
    {
        thisTalent = tal;

        tooltipTarget = GetComponent<TooltipTarget>();
        
        tooltipTarget.tooltip = talentTooltip = GameObject.FindObjectOfType<TalentTooltip>();

        tooltipTarget.onPointerEnter +=
            () =>{
                talentTooltip.SetContent(thisTalent);
                updateOnPointerEnter?.Invoke();;
            };


        eventTrigger = button.GetComponent<EventTrigger>();

        eventTrigger.AddTrigger(EventTriggerType.PointerEnter,
                                (args)=>
                                {
                                    FirstTimeHover();
                                });

        tooltipTarget.InitializeEventTrigger(eventTrigger);



        Vault.talentPoints.onChanged_Amount += updateOnCurrencyChanged;

        gameObject.SetActive(true);
    }



    void Awake()
    {
        instances.Add(this);

        button = GetComponentInChildren<Button>();

        if (button == null) Debug.LogWarning("button is null");


        coveredState = new CoveredState(this);
        discoveredState = new DiscoveredState(this);
        boughtState = new BoughtState(this);

        SwitchState(coveredState);
    }

    public void FirstTimeHover()
    {
        if (isNewopened)
        {
            isNewopened = false;

            NewopenFlare._Inst.OnNewTalentHovered(newopenFlareImage);
        }
    }

    public abstract class State : ViewState
    {
        new public TalentView view;

        public State(TalentView view) : base(view)
        {
            this.view = view;
        }
    }

    public class CoveredState : State
    {
        public CoveredState(TalentView view) : base(view) {}

        public override void Setup()
        {
            view.gameObject.SetActive(false);
            view.checkmark.gameObject.SetActive(false);
        }

        public override void Uninstall()
        {
        }
    }

    public class DiscoveredState : State
    {
        public DiscoveredState(TalentView view) : base(view) {}

        public override void Setup()
        {
            Debug.Log(view.thisTalent.name +"'s view set to Discovered");

            view.button.onClick.AddListener(BuyTalent);
            view.updateOnPointerEnter = OnPointerEnter;
            view.updateOnCurrencyChanged = OnCurrencyChanged;


            if (view.isNewopened)
                NewopenFlare._Inst.OnNewTalentOpened(view.newopenFlareImage);


            view.gameObject.SetActive(true);

            view.checkmark.gameObject.SetActive(false);
        }

        public override void Uninstall()
        {
            view.button.onClick.RemoveListener(BuyTalent);
            view.updateOnPointerEnter = null;
            view.updateOnCurrencyChanged = null;
        }

        private void BuyTalent()
        {
            view.thisTalent.vendible.Buy();
        }
        void OnPointerEnter()
        {
            view.talentTooltip.lCost.text = view.thisTalent.vendible.price.ToStringFormatted();
        }

        void OnCurrencyChanged(float currencyChanged)
        {
            bool canAfford = view.thisTalent.vendible.CanBuy();

            if (canAfford)
            {
                view.talentTooltip.lCost.color = Color.green;
            }
            else
            {
                view.talentTooltip.lCost.color = Color.red;
            }

            view.button.interactable = canAfford;
        }

    }

    public class BoughtState : State
    {
        public BoughtState(TalentView view) : base(view) {}

        public override void Setup()
        {
            Debug.Log(view.thisTalent.name + "'s view set to Bought");


            view.updateOnPointerEnter = OnPointerEnter;
            view.updateOnCurrencyChanged = null;

            TurnButtonOff();

            view.gameObject.SetActive(true);

            view.checkmark.gameObject.SetActive(true);

            view.FirstTimeHover();

            OnPointerEnter();
        }

        public override void Uninstall()
        {
            view.updateOnPointerEnter = null;
        }

        private void TurnButtonOff()
        {
            Color normalColor = view.button.colors.normalColor;

            view.button.interactable = true;
            view.button.transition = Button.Transition.None;
            view.button.image.color = normalColor;
        }

        void OnPointerEnter()
        {
            view.talentTooltip.lCost.color = Color.green;
            view.talentTooltip.lCost.text = "Owned";
        }
    }

}

