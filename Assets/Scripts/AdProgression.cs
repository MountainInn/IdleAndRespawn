using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class AdProgression : MonoBehaviour
{
    static AdProgression inst;
    static public AdProgression _Inst => inst??=GameObject.FindObjectOfType<AdProgression>();

    int _level = 0;
    [JsonPropertyAttribute]
    public int Level
    {
        get => _level;
        private set
        {
            _level = value;

            Mult = 1f + _level * oneWatchBonus;

            liftTalents.CheckFloors(_level);

            view.UpdateLevel(_level);
        }
    }

    float _mult = 1f;
    [JsonPropertyAttribute] public float Mult
    {
        get => _mult;
        private set
        {
            _mult = value;

            heroArmorMult.Mutation = _mult;
            heroDamageMult.Mutation = _mult;
            followersArmorMult.Mutation = _mult;
            followersDamageMult.Mutation = _mult;
        }
    }

    public const float oneWatchBonus = .02f;
    [SpaceAttribute, SerializeField] VerticalLayoutGroup buffsLayout;
    [SerializeField]
    Image
        inspirationIcon,
        vengenceIcon,
        goodFortuneIcon,
        lastWishesIcon,
        sharingLightIcon;
    ArithmeticNode
        heroArmorMult, heroDamageMult,
        followersArmorMult, followersDamageMult;
    public Lift<AdTalent>
        liftTalents = new Lift<AdTalent>();
    static public AdTalent
        inspiration,
        vengeance,
        sharingLight,
        lastWishes;

    [SerializeField] AdProgressionView view;


    [System.Runtime.Serialization.OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext sc)
    {
        liftTalents.CheckFloors(Level);
    }



    void Start()
    {
        InitMult();

        InitTalents();
    }

    private void InitTalents()
    {
        liftTalents.Add(15, inspiration = new Inspiration(inspirationIcon));
        liftTalents.Add(25, vengeance = new Vengeance(vengenceIcon, this));
        liftTalents.Add(35, sharingLight = new SharingLight(sharingLightIcon));
        liftTalents.Add(50, lastWishes = new LastWishes(lastWishesIcon));

        view.InitMilestones();
        liftTalents.onLifted += (tal) => { view.UpdateMilestones(tal); };
    }

    private void InitMult()
    {
        Hero._Inst.armor.chain.Add(4000, heroArmorMult = ArithmeticNode.CreateMult(1f));
        Hero._Inst.damage.chain.Add(4000, heroDamageMult = ArithmeticNode.CreateMult(1f));
        Followers._Inst.armor.chain.Add(4000, followersArmorMult = ArithmeticNode.CreateMult(1f));
        Followers._Inst.damage.chain.Add(4000, followersDamageMult = ArithmeticNode.CreateMult(1f));
    }

    public void LevelUp()
    {
        Level++;
    }

    public class Inspiration : AdTalent
    {
        float mutation = 1.1f;
        ArithmeticNode heroDamageMult, followersDamageMult;

        public Inspiration(Image inspirationIcon) : base(Hero._Inst)
        {
            heroDamageMult = ArithmeticNode.CreateMult();
            Hero._Inst.damage.chain.Add(5000, heroDamageMult);

            followersDamageMult = ArithmeticNode.CreateMult();
            Followers._Inst.damage.chain.Add(5000, followersDamageMult);

            InitializeViewValues("Inspiration",
                                 $"+{mutation-1f:P0} attack to Hero and Followers while scrying is on cooldown");

            buffIcon = inspirationIcon;
            buffIcon.gameObject.SetActive(false);
        }

        override protected void Connect()
        {
            AdCharges.onChargesChanged += CheckAdCharges;
            CheckAdCharges();
        }

        override protected void Disconnect()
        {
            AdCharges.onChargesChanged -= CheckAdCharges;

            EndBuff();
        }

        public void CheckAdCharges()
        {
            if (AdCharges.IsFull) EndBuff();
            else StartBuff();
        }


        private void StartBuff()
        {
            heroDamageMult.Mutation = mutation;
            followersDamageMult.Mutation = mutation;

            buffIcon.gameObject.SetActive(true);
        }

        private void EndBuff()
        {
            heroDamageMult.Mutation = 1f;
            followersDamageMult.Mutation = 1f;

            buffIcon.gameObject.SetActive(false);
        }


        public override string updatedDescription => throw new System.NotImplementedException();
    }

    public class Vengeance : AdTalent
    {
        float
            mutation = .5f,
            duration;
        ArithmeticNode
            heroAttspeedMult,
            followersAttspeedMult;
        MonoBehaviour
            parent;
        Coroutine
            endBuffCoroutine;

        public Vengeance(Image vengenceIcon, MonoBehaviour parent) : base(Hero._Inst)
        {
            this.parent = parent;

            heroAttspeedMult = ArithmeticNode.CreateMult(1f);
            followersAttspeedMult = ArithmeticNode.CreateMult(1f);

            Hero._Inst.attackSpeed.chain.Add(100, heroAttspeedMult);
            Followers._Inst.attackSpeed.chain.Add(100, followersAttspeedMult);

            int minutes = 1;
            duration = minutes * 60;

            InitializeViewValues("Vengeance",
                                 $"{1/mutation-1f:P0} attack speed to hero and followers for {minutes} minutes after respawn");

            buffIcon = vengenceIcon;
            buffIcon.gameObject.SetActive(false);
        }

        protected override void Connect()
        {
            SoftReset.onReset += StartBuff;
        }
        protected override void Disconnect()
        {
            SoftReset.onReset -= StartBuff;

            if (endBuffCoroutine != null)
            {
                parent.StopCoroutine(endBuffCoroutine);
                endBuffCoroutine = null;
            }

            EndBuff();
        }


        private void StartBuff()
        {
            heroAttspeedMult.Mutation = mutation;
            followersAttspeedMult.Mutation = mutation;

            if (endBuffCoroutine != null)
                parent.StopCoroutine(endBuffCoroutine);

            endBuffCoroutine = parent.StartCoroutine(CoroutineExtension.InvokeAfter(EndBuff, duration));

            buffIcon.gameObject.SetActive(true);
        }

        private void EndBuff()
        {
            heroAttspeedMult.Mutation = 1f;
            followersAttspeedMult.Mutation = 1f;

            buffIcon.gameObject.SetActive(false);

            endBuffCoroutine = null;
        }


    }

    public class GoodFortune : AdTalent
    {
        float reflectChance = .2f;

        public GoodFortune(Image goodFortuneIcon): base(Hero._Inst)
        {
            InitializeViewValues("Good Fortune",
                                 $"{reflectChance:P0} chance for doom to be reflected back at The Boss.");

            buffIcon = goodFortuneIcon;
            buffIcon.gameObject.SetActive(false);
        }

        protected override void Connect()
        {
            unit.takeDamageChain.Add(-100, ReflectDoom);
            buffIcon.gameObject.SetActive(true);
        }

        protected override void Disconnect()
        {
            unit.takeDamageChain.Remove(-100, ReflectDoom);
            buffIcon.gameObject.SetActive(false);
        }


        private void ReflectDoom(DoDamageArgs dargs)
        {
            if (dargs.isDoom && UnityEngine.Random.value <= reflectChance)
            {
                DoDamageArgs reflectArgs = new DoDamageArgs(dargs.attacker, dargs.damage._Val){ isReflected = true };

                dargs.attacker.TakeDamage(reflectArgs);
            }
        }
    }

    public class SharingLight : AdTalent
    {
        static public bool isSharing;

        float mutation = 0.7f;
        ArithmeticNode healSpeed;

        public SharingLight(Image sharedHealIcon) : base(Hero._Inst)
        {
            unit.healSpeed.chain.Add(10, (healSpeed = ArithmeticNode.CreateMult(1f)));

            InitializeViewValues("Sharing Light",
                                 $"Healing always affects both Hero and Followers. Also +30% to heal speed.");

            buffIcon = sharedHealIcon;
            buffIcon.gameObject.SetActive(false);

        }

        protected override void Connect()
        {
            isSharing = true;
            healSpeed.Mutation = mutation;
            buffIcon.gameObject.SetActive(true);
        }

        protected override void Disconnect()
        {
            isSharing = false;
            healSpeed.Mutation = 1;
            buffIcon.gameObject.SetActive(false);
        }
    }


    public class LastWishes : AdTalent
    {
        float
            healthToShieldRatio = 1f,
            degradationRatio = .3f,
            ratio;

        BarrierShaderController shaderController ;

        public LastWishes(Image lastWishesIcon) : base(Hero._Inst)
        {
            InitializeViewValues("Last Wishes",
                                 $"After Followers' death Hero recieves protecting barrier.\n"+
                                 $"Barrier's capacity at first activation is {healthToShieldRatio:P0} of Followers' health\n"+
                                 $"and it grows {healthToShieldRatio * degradationRatio:P0} weaker with consecutive Followers' deaths."
                                 );

            buffIcon = lastWishesIcon;
            buffIcon.gameObject.SetActive(false);

            shaderController = GameObject.FindObjectOfType<BarrierShaderController>();

            RestoreBarrierRatio();
        }

        protected override void Connect()
        {
            shaderController.StartBarrierUpdate();
            unit.followers.onDeathChain.Add(MakeBarrier);
            buffIcon.gameObject.SetActive(true);

            SoftReset.onReset += RestoreBarrierRatio;
            Hero._Inst.barrierRange.onLessThanZero += HideBarrier;
        }

        protected override void Disconnect()
        {
            unit.followers.onDeathChain.Remove(MakeBarrier);
            SoftReset.onReset -= RestoreBarrierRatio;
            Hero._Inst.barrierRange.onLessThanZero -= HideBarrier;
            buffIcon.gameObject.SetActive(false);
        }

        void HideBarrier()
        {
            HeroView._Inst.ShowBarrier(false);
        }


        void MakeBarrier(Unit followers)
        {
            if (ratio <= 0) return;

            shaderController.ResetTime();
            HeroView._Inst.ShowBarrier(true);

            float barrierCapacity = followers.healthRange._Max * ratio;

            unit.barrierRange.Reinitialize(barrierCapacity);

            DegradeBarrierRatio();
        }

        void DegradeBarrierRatio()
        {
            if (ratio > 0)
                ratio -= healthToShieldRatio * degradationRatio;
        }
        void RestoreBarrierRatio() => ratio = healthToShieldRatio;
    }
}

[JsonObjectAttribute(MemberSerialization.OptIn)]
abstract public class AdTalent : Talent, ILifted
    {
        public Image buffIcon;

        public AdTalent(Unit unit) : base(unit){}

        public override string updatedDescription => throw new NotImplementedException();


        public string realDescription, hiddenDescription;
        new public void InitializeViewValues(string name, string description)
        {
            this.name = name;

            realDescription = description;

            hiddenDescription = realDescription.HideString();

            base.description = hiddenDescription;
        }

        public int floor { get; set; }
        bool _islifted;
        [JsonPropertyAttribute]
        public bool isLifted
        { get => _islifted; set
            {
                _islifted = value;

                if (_islifted) OnLifted();
            }
        }

        public void OnLifted()
        {
            Connect();

            base.description = realDescription;
        }

        public void OnDropped()
        {
            Disconnect();

            base.description = hiddenDescription;
        }

}

[JsonObjectAttribute(MemberSerialization.OptIn)]
abstract public class LiftedTalent : Talent, ILifted
{
    public LiftedTalent(Unit unit) : base(unit){}

    public override string updatedDescription => throw new NotImplementedException();


    public int floor { get; set; }
    [JsonPropertyAttribute]
    public bool isLifted {get; set;}

    [OnDeserializedAttribute]
    new public void OnDeserialized(StreamingContext streamingContext)
    {
        base.OnDeserialized(streamingContext);

        if (isLifted && !vendible.isOwned) OnLifted();
    }

    public void OnLifted()
    {
        Discover();
    }

    public void OnDropped() {}

}

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Lifted : ILifted
{
    public event Action onLifted;

    public Lifted() {}
    public Lifted(int floor)
    {
        this.floor = floor;
    }


    public int floor { get; set; }
    [JsonPropertyAttribute]
    public bool isLifted {get; set;}


    public void OnLifted()
    {
        onLifted.Invoke();
    }
}
