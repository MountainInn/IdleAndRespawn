using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalentStripView : ViewClass
{
    [SerializeField] Text name, description, buyText, cost;
    [SerializeField] public Button buyButton;
    [SerializeField] Image icon, checkmark, talentPointsIcon, beaconImage;

    public Talent thisTalent;

    TalentScriptedObject talentSO;

    Camera _camera;

    public State
        coveredState,
        discoveredState,
        boughtState;


    RectTransform rectTransform;
    NoveltyBeaconColor beacon ;

    static public List<TalentStripView> instances = new List<TalentStripView>();
    void Awake()
    {
        instances.Add(this);

        coveredState = new CoveredState(this);
        discoveredState = new DiscoveredState(this);
        boughtState = new BoughtState(this);

        _camera = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        Talent.onDiscoveredFirstTime += (tal) => { if (tal == thisTalent) StartBeacon(); };

        beacon = new NoveltyBeaconColor(TalentViewContainer._Inst,
                                        beaconImage,
                                        beaconImage.color,
                                        beaconImage.color.SetA(1),
                                        () =>{ return !rectTransform.IsFullyVisibleFrom_Optimized(_camera); });
        beacon.checkInterval = 1f;
    }

    void StartBeacon()
    {
        beacon.StartSignal();
    }

    public void ConnectToTalent(Talent tal)
    {
        thisTalent = tal;

        string talName = tal.GetType().Name;

        gameObject.name = talName+"_Talview";
        icon.sprite = ReferenceHeap._Inst.talentSOs[talName].sprite;

        name.text = thisTalent.name;
        UpdateDescription();
    }


    public void UpdateDescription()
    {
            description.text = thisTalent.description + thisTalent.updatedDescription;
        }

    public abstract class State : ViewState
    {
        public TalentStripView view;

        public State(TalentStripView view)
        {
            this.view = view;
        }

    }

    public class CoveredState : State
    {
        public CoveredState(TalentStripView view) : base(view) {}

        public override void Setup()
        {
        }

        public override void Uninstall()
        {
        }
    }

    public class DiscoveredState : State
    {
        public DiscoveredState(TalentStripView view) : base(view) {}

        public override void Setup()
        {
            view.cost.text = view.thisTalent.vendible.price.ToStringFormatted();


            view.checkmark.gameObject.SetActive(false);



            view.buyButton.onClick.AddListener(view.thisTalent.vendible.Buy);

            CheckCanBuy();
            view.UpdateDescription();;

            Vault.TalentPoints.onChanged += CheckCanBuy;

            view.thisTalent.onRecalculated += view.UpdateDescription;
        }

        public override void Uninstall()
        {
            view.buyButton.onClick.RemoveListener(view.thisTalent.vendible.Buy);

            Vault.TalentPoints.onChanged -= CheckCanBuy;

            view.thisTalent.onRecalculated -= view.UpdateDescription;
        }

        private void BlinkExclamationMarkOnTabButton()
        {
            throw new NotImplementedException();
        }

        void CheckCanBuy()
        {
            bool canAfford = view.thisTalent.vendible.CanBuy();

            if (canAfford)
            {
                view.cost.color = Color.green;
            }
            else
            {
                view.cost.color = Color.red;
            }

            view.buyButton.interactable = canAfford;
        }

    }

    public class BoughtState : State
    {
        public BoughtState(TalentStripView view) : base(view) {}

        public override void Setup()
        {
            view.beacon.StopSignal();

            TurnButtonOff();

            DisplayOwned();

            view.checkmark.gameObject.SetActive(true);

            view.thisTalent.onRecalculated += view.UpdateDescription;
        }

        public override void Uninstall()
        {
            view.thisTalent.onRecalculated -= view.UpdateDescription;
        }

        private void TurnButtonOff()
        {
            // Color normalColor = view.buyButton.colors.normalColor;

            view.buyButton.interactable = false;
            view.buyButton.transition = Button.Transition.None;
            // view.buyButton.image.color = normalColor;
        }

        void DisplayOwned()
        {
            view.cost.color = Color.black;
            view.cost.text = "Owned";

            view.buyText.text = "";
            view.talentPointsIcon.gameObject.SetActive(false);
        }
    }
}
