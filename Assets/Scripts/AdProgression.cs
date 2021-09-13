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
    public Lift<LiftedTalent>
        liftTalents = new Lift<LiftedTalent>();

    [SerializeField] AdProgressionView view;
    Animation flareAnimation;


    [System.Runtime.Serialization.OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext sc)
    {
        liftTalents.CheckFloors(Level);
    }



    void Start()
    {
        InitMult();

        InitTalents();

        InitAnimation();
        
        SubscribeToAdvertisement();

        Level = 0;
    }

    private void InitAnimation()
    {
        flareAnimation = GetComponent<Animation>();
        flareAnimation.wrapMode = WrapMode.Loop;
    }

    private void SubscribeToAdvertisement()
    {
        Advertisement.onReady += ()=>{ flareAnimation.Play(); };
        Advertisement.onWatched += flareAnimation.Stop ;
        Advertisement.onWatched += LevelUp;
    }

    private void InitTalents()
    {
        liftTalents.Add(05, new Inspiration(inspirationIcon));
        liftTalents.Add(10, new Vengence(vengenceIcon, this));
        liftTalents.Add(20, new SharingLight(sharingLightIcon));
        liftTalents.Add(30, new LastWishes(lastWishesIcon));

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




    abstract public class LiftedTalent : Talent, ILifted
    {
        protected Image buffIcon;

        public LiftedTalent(Unit unit) : base(unit){}

        public override string updatedDescription => throw new NotImplementedException();

        public string realDescription, hiddenDescription;
        public void InitializeViewValues(string name, string description)
        {
            this.name = name;

            realDescription = description;

            hiddenDescription = realDescription.HideString();

            base.description = hiddenDescription;
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

        abstract protected void Connect();
        abstract protected void Disconnect();
    }

    public class Inspiration : LiftedTalent
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
                                 $"+{mutation-1f:P0} attack damage to hero and followers while scrying is on cooldown");

            buffIcon = inspirationIcon;
            buffIcon.gameObject.SetActive(false);
        }

        override protected void Connect()
        {
            Advertisement.onWatched += StartBuff;
            Advertisement.onReady += EndBuff;
        }

        override protected void Disconnect()
        {
            Advertisement.onWatched -= StartBuff;
            Advertisement.onReady -= EndBuff;
            
            EndBuff();
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

    public class Vengence : LiftedTalent
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

        public Vengence(Image vengenceIcon, MonoBehaviour parent) : base(Hero._Inst)
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

            if (endBuffCoroutine != null) parent.StopCoroutine(endBuffCoroutine);

            EndBuff();
        }


        private void StartBuff()
        {
            heroAttspeedMult.Mutation = mutation;
            followersAttspeedMult.Mutation = mutation;

            endBuffCoroutine = parent.StartCoroutine(CoroutineExtension.InvokeAfter(EndBuff, duration));

            buffIcon.gameObject.SetActive(true);
        }
        private void EndBuff()
        {
            heroAttspeedMult.Mutation = 1f;
            followersAttspeedMult.Mutation = 1f;

            buffIcon.gameObject.SetActive(false);
        }


    }

    public class GoodFortune : LiftedTalent
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

    public class SharingLight : LiftedTalent
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


    public class LastWishes : LiftedTalent
    {
        float
            healthToShieldRatio = 10f,
            degradationRatio = .25f,
            ratio;

        public LastWishes(Image lastWishesIcon) : base(Hero._Inst)
        {
            InitializeViewValues("Last Wishes",
                                 $"After Followers' death Hero recieves protecting barrier.\n"+
                                 $"Barrier's capacity at first activation is {healthToShieldRatio:P0} of Followers' health\n"+
                                 $"and it grows {healthToShieldRatio * degradationRatio:P0} weaker with consecutive Followers' deaths."
                                 );

            buffIcon = lastWishesIcon;
            buffIcon.gameObject.SetActive(false);

            RestoreBarrierRatio();
        }

        protected override void Connect()
        {
            unit.followers.onDeathChain.Add(MakeBarrier);
            SoftReset.onReset += RestoreBarrierRatio;
            Hero._Inst.barrierRange.onLessThanZero += ()=>{ HeroView._Inst.ShowBarrier(false); };
            buffIcon.gameObject.SetActive(true);
        }

        protected override void Disconnect()
        {
            unit.followers.onDeathChain.Remove(MakeBarrier);
            SoftReset.onReset -= RestoreBarrierRatio;
            Hero._Inst.barrierRange.onLessThanZero -= () =>{ HeroView._Inst.ShowBarrier(false); };
            buffIcon.gameObject.SetActive(false);
        }


        void MakeBarrier(Unit followers)
        {
            if (ratio <= 0) return;

            HeroView._Inst.ShowBarrier(true);

            float barrierCapacity = followers.healthRange._Max* ratio;

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
