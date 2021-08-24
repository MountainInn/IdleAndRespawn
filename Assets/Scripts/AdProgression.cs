using System;
using UnityEngine;
using UnityEngine.UI;

public class AdProgression : MonoBehaviour
{
    static AdProgression inst;
    static public AdProgression _Inst => inst??=GameObject.FindObjectOfType<AdProgression>();

    [HideInInspector] public int level;
    [HideInInspector] public float mult;
    [SpaceAttribute, SerializeField] VerticalLayoutGroup buffsLayout;
    [SerializeField]
    Image
        inspirationIcon,
        vengenceIcon,
        goodFortuneIcon,
        sharingLightIcon;

    ArithmeticNode
        armorMult, damageMult;

    public Lift<LiftedTalent> liftTalents = new Lift<LiftedTalent>();

    static public Advertisement advertisement;
    AdProgressionView view;
    Animation flareAnimation;

    void Start()
    {
        view = GameObject.FindObjectOfType<AdProgressionView>();
        advertisement = GetComponent<Advertisement>();
        
        InitMult();

        InitTalents();

        InitAnimation();
        
        SubscribeToAdvertisement();

        SetLevel(0);
    }

    private void InitAnimation()
    {
        flareAnimation = GetComponent<Animation>();
        flareAnimation.wrapMode = WrapMode.Loop;
    }

    private void SubscribeToAdvertisement()
    {
        advertisement.onReady += () => flareAnimation.Play();
        advertisement.onWatched += () => flareAnimation.Stop(); ;
        advertisement.onWatched += () => { LevelUp(); };
    }

    private void InitTalents()
    {
        liftTalents.Add(5, new Inspiration(inspirationIcon));
        liftTalents.Add(10, new Vengence(vengenceIcon, this));
        liftTalents.Add(20, new GoodFortune(goodFortuneIcon));
        liftTalents.Add(30, new SharingLight(sharingLightIcon));

        view.InitMilestones();
        liftTalents.onLifted += (tal) => { view.UpdateMilestones(tal); };
    }

    private void InitMult()
    {
        armorMult = new ArithmeticNode(new ArithmMult(), mult);
        damageMult = new ArithmeticNode(new ArithmMult(), mult);

        Hero._Inst.armor.chain.Add(4, armorMult);
        Hero._Inst.damage.chain.Add(4, damageMult);
    }

    public void LevelUp()
    {
        level++;

        SetLevel(level);
    }

    void SetLevel(int level)
    {
        mult = 1f + level * .01f;
        SetMult(mult);

        liftTalents.CheckFloors(level);

        view.UpdateLevel(level);
    }

    public void SetMult(float mult)
    {
        armorMult.Mutation = mult;
        damageMult.Mutation = mult;
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
        ArithmeticNode damageMult;

        public Inspiration(Image inspirationIcon) : base(Hero._Inst)
        {
            damageMult = new ArithmeticNode(new ArithmMult(), 1);

            Hero._Inst.damage.chain.Add(5, damageMult);
            Followers._Inst.damage.chain.Add(5, damageMult);

            InitializeViewValues("Inspiration",
                                 $"+{mutation-1f:P0} attack damage to hero and followers while scrying is on cooldown");

            buffIcon = inspirationIcon;
            buffIcon.gameObject.SetActive(false);
        }

        override protected void Connect()
        {
            advertisement.onWatched += StartBuff;
            advertisement.onReady += EndBuff;
        }

        override protected void Disconnect()
        {
            advertisement.onWatched -= StartBuff;
            advertisement.onReady -= EndBuff;
            
            EndBuff();
        }


        private void StartBuff()
        {
            damageMult.Mutation = mutation;
            buffIcon.gameObject.SetActive(true);
        }

        private void EndBuff()
        {
            damageMult.Mutation = 1f;
            buffIcon.gameObject.SetActive(false);
        }


        public override string updatedDescription => throw new System.NotImplementedException();
    }

    public class Vengence : LiftedTalent
    {
        float
            mutation = 2f,
            duration;
        ArithmeticNode
            attspeedMult;
        MonoBehaviour
            parent;

        public Vengence(Image vengenceIcon, MonoBehaviour parent) : base(Hero._Inst)
        {
            this.parent = parent;

            attspeedMult = new ArithmeticNode(new ArithmMult(), 1f);

            Hero._Inst.attackSpeed.chain.Add(10, attspeedMult);
            Followers._Inst.attackSpeed.chain.Add(10, attspeedMult);

            int minutes = 3;
            duration = minutes * 60;

            InitializeViewValues("Vengeance",
                                 $"+{mutation-1:P0} attack speed to hero and followers for {minutes} minutes after respawn");

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
            EndBuff();
            parent.CancelInvoke("EndBuff");
        }


        private void StartBuff()
        {
            attspeedMult.Mutation = mutation;

            parent.Invoke("EndBuff", duration);

            buffIcon.gameObject.SetActive(true);
        }
        private void EndBuff()
        {
            attspeedMult.Mutation = 1f;
            buffIcon.gameObject.SetActive(false);
        }
    }

    public class GoodFortune : LiftedTalent
    {

        public GoodFortune(Image goodFortuneIcon): base(Hero._Inst)
        {
            InitializeViewValues("Good Fortune",
                                 $"20% chance for doom to be reflected back at The Boss.");

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
            if (dargs.isDoom && UnityEngine.Random.value <= .2f)
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
            healSpeed = new ArithmeticNode(new ArithmMult(), mutation);

            unit.healSpeed.chain.Add(10, healSpeed);

            InitializeViewValues("SharingLight",
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

}
