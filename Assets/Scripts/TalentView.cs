using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;


[RequireComponent(typeof(TooltipTarget))]
public class TalentView : Register<TalentView>
{
    [SaveField] bool isNewopened = true;

    Image newopenFlareImage;
    Button button;

    TooltipTarget tooltipTarget;
    TalentTooltip talentTooltip;
    EventTrigger eventTrigger;


    Talent thisTalent;


    public TalviewState state;
    public Action<float> updateView_onCurrencyChanged;
    public Action updateView;

    public void ConnectToTalent(Talent tal)
    {
        thisTalent = tal;

        tooltipTarget = GetComponent<TooltipTarget>();
        
        tooltipTarget.tooltip = talentTooltip = GameObject.FindObjectOfType<TalentTooltip>();

        talentTooltip.lCost.text = thisTalent.cost.ToString();

        tooltipTarget.onPointerEnter +=
            () =>{
                talentTooltip.SetContent(thisTalent);
                updateView?.Invoke();;
            };


        eventTrigger = button.GetComponent<EventTrigger>();

        eventTrigger.AddTrigger(EventTriggerType.PointerEnter,
                                (args)=>
                                {
                                    OnHovered();
                                });

        tooltipTarget.InitializeEventTrigger(eventTrigger);


        button.onClick.AddListener(() =>{
            thisTalent.Buy(Vault.talentPoints);
            });

        Vault.talentPoints.onChanged += updateView_onCurrencyChanged;

        gameObject.SetActive(true);
    }



    void Awake()
    {
        RegisterSelf();

        button = GetComponentInChildren<Button>();

        if (button == null) Debug.LogWarning("button is null");

        newopenFlareImage =
            GetComponentsInChildren<Image>()
            .First(child => child.name == "NewopenFlare");

        
        new CoveredState().Setup(this);
    }

    void OnHovered()
    {
        if (isNewopened)
        {
            isNewopened = false;

            NewopenFlare._Inst.OnNewTalentHovered(newopenFlareImage);
        }
    }

    public abstract class TalviewState
    {
        public TalentView view;

        public void Setup(TalentView view)
        {
            this.view = view;
            view.state = this;

            ConcreteSetup();
        }

        abstract protected void ConcreteSetup();

        abstract public TalviewState NextState();

        abstract protected bool CanChangeState();

        abstract public void Update();
    }

    public class CoveredState : TalviewState
    {
        protected override void ConcreteSetup()
        {
            view.gameObject.SetActive(false);
        }

        public override void Update() {}

        public override TalviewState NextState()
        {
            if (CanChangeState()) return new DiscoveredState();
            else return this;
        }

        protected override bool CanChangeState()
        {
            return view.thisTalent.isDiscovered;
        }
    }

    public class DiscoveredState : TalviewState
    {
        protected override void ConcreteSetup()
        {
            view.talentTooltip.lCost.text = view.thisTalent.cost.ToString();

            if (view.isNewopened)
                NewopenFlare._Inst.OnNewTalentOpened(view.newopenFlareImage);


            view.updateView = Update;
            view.updateView_onCurrencyChanged = UpdateView;


            Update();

            view.gameObject.SetActive(true);
        }

        void UpdateView(float currencyChanged) => Update();

        public override void Update()
        {
            bool canAfford = view.thisTalent.CanAfford();

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

        public override TalviewState NextState()
        {
            if (CanChangeState()) return new BoughtState();
            else return this;
        }

        protected override bool CanChangeState()
        {
            return view.thisTalent.isBought;
        }
    }

    public class BoughtState : TalviewState
    {
        protected override void ConcreteSetup()
        {
            view.button.transition = Button.Transition.None;
            view.button.interactable = false;


            view.updateView = Update;
            view.updateView_onCurrencyChanged = null;


            view.gameObject.SetActive(true);
        }

        public override void Update()
        {
        }

        public override TalviewState NextState()
        {
            return this;
        }

        protected override bool CanChangeState()
        {
            return false;
        }
    }

}

