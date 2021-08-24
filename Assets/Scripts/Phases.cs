using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Phases : MonoBehaviour
{
    public static List<Phases> _Instances = new List<Phases>();
    public static Phases _Inst;

    public static List<Phase> phases;

    public static int checkPhaseID;

    static Hero hero;
    static Boss boss;
    static Followers followers;

    [JsonPropertyAttribute]
    static public List<Talent> allActiveTalents = new List<Talent>();
    static public List<Talent> allDiscoveredTalents = new List<Talent>();
    static public List<DamageProcessing> allActiveDamageProcessings = new List<DamageProcessing>();

    public GameObject
        tier1,
        tier2,
        tier3,
        heroInterface;


    void Awake()
    {
        _Inst = this;
        _Instances.Add(this);

        hero = GameObject.FindObjectOfType<Hero>();
        boss = GameObject.FindObjectOfType<Boss>();
        followers = GameObject.FindObjectOfType<Followers>();

        
        checkPhaseID = 0;
    }

    void Start()
    {
        phases = new List<Phase>()
        {
            /// Phase 1
            new Phase(0)
            {
                damageProcessings = new DamageProcessing[]
                {
                    new Attack(hero),
                    new CriticalHit(hero),
                    new TakeDamageFollowers(hero),
                    new TakeDamageReflect(hero),
                    new TakeDamageArmor(hero),
                    new TakeDamageHealth(hero),
                    new Healing(hero),
                    new TakeHeal(hero),

                    new Attack(followers),
                    new TakeDamageArmor(followers),
                    new TakeDamageHealth(followers),
                    new TakeHeal(followers),

                    new Attack(boss),
                    new CriticalHit(boss),
                    new CurseOfDoom(ReferenceHeap._Inst.curseOfDoomProgress)
                },

                talents = new Talent[]
                {
                    new DoubleReflect(hero),

                },

                views = heroInterface.GetComponentsInChildren<Transform>()
                .Where(tr => tr.GetComponent<TalentView>() == null).ToArray()
            },

            /// Phase 2
            new Phase(
                //6
                0)
            {
                damageProcessings = new DamageProcessing[]
                {
                },

                talents = new Talent[]
                {
                    new LastStand(hero)
                },
                
            },

            /// Phase 3
            new Phase(
                //12
                0)
            {
                talents = new Talent[]
                {
                    new StaminaTraining(followers),
                    new FullBlood(hero),
                },
                

            },

            /// Phase 4
            new Phase(
                //14
                0)
            {
                talents = new Talent[]
                {
                    new Infirmary(followers),
                },
            },
            new Phase(
                //16
                0)
            {
                talents = new Talent[]
                {
                    new TitansGrowth(hero),
                },
            },new Phase(
                //18
                0)
            {
                talents = new Talent[]
                {
                    new Interruption(hero)
                },
            },new Phase(
                //20
                0)
            {
                talents = new Talent[]
                {
                    new BlindingLight(hero)
                },
            },new Phase(
                //22
                0)
            {
                talents = new Talent[]
                {
                    new BattleExpirience(hero)
                },
            },new Phase(
                //24
                0)
            {
                talents = new Talent[]
                {
                    new FindWeakness(hero)
                },
            },new Phase(
                //26
                0)
            {
                talents = new Talent[]
                {
                },
            },new Phase(
                //28
                0)
            {
                talents = new Talent[]
                {
                    new DoubleJudgement(hero)
                    , new Regeneration(hero)
                },
            },new Phase(
                //30
                0)
            {
                talents = new Talent[]
                {
                    new CoordinatedActions(followers)
                },
            },new Phase(
                //32
                0)
            {
                talents = new Talent[]
                {
                    new Diversion(followers)
                },
            },new Phase(
                //34
                0)
            {
                talents = new Talent[]
                {
                    new VeteransOfThirdWar(hero)
                },
            },new Phase(
                //36
                0)
            {
                talents = new Talent[]
                {
                    new EnfeeblingStrike(hero)
                },
            },new Phase(
                //38
                0)
            {
                talents = new Talent[]
                {
                    new BloodMadness(hero)
                },
            },new Phase(
                //40
                0)
            {
                talents = new Talent[]
                {
                    new Ressurection(hero)
                },
            },new Phase(
                //42
                0)
            {
                talents = new Talent[]
                {
                    new Rebirth(hero)
                },
            },new Phase(
                //44
                0)
            {
                talents = new Talent[]
                {
                    new HotHand(hero)
                },
            },new Phase(
                //46
                0)
            {
                talents = new Talent[]
                {
                    new Cyclone(hero)
                },
            },new Phase(
                //48
                0)
            {
                talents = new Talent[]
                {
                    new CounterAttack(hero)
                },
            },new Phase(
                //50
                0)
            {
                talents = new Talent[]
                {
                    new Multicrit(hero)
                },
            }
            ,new Phase(
                //52
                0)
            {
                talents = new Talent[]
                {
                    new Dejavu(hero)
                },
            }
        };

        foreach(var item in TalentView.instances )
        {
            item.gameObject.SetActive(false);
        }


        CheckPhase();
        Boss._Inst.onStageChanged += CheckPhase;
    }


    void CheckPhase()
    {
        foreach (var currentPhase in phases)
        {
            if (!currentPhase.active && currentPhase.CanActivate())
            {
                currentPhase.ActivatePhase(true);
            }
        }
    }




    public class Phase
    {
        public int border;
        public bool active;
        public Action<bool> onSwitch;
        public Talent[] talents;
        public DamageProcessing[] damageProcessings;
        public Transform[] views;

        CanvasGroupFadeInOut fadeInOut;

        public Phase(int border)
        {
            this.border = border;

            active = false;
            
            onSwitch = default(Action<bool>);
        }


        public bool CanActivate()
        {
            return !active && SoftReset.maxStage >= border;
        }

        public void SyncViewsActivity()
        {
            if (views != null)
                ActivateViews(active);
        }

        public void ActivatePhase(bool toggle)
        {
            active = toggle;

            if (damageProcessings != null) ActivateDamageProcessing(toggle);
            
            if (views != null) ActivateViews(toggle);

            if (talents != null) ActivateTalents();

            onSwitch?.Invoke(toggle);

            GameLogger.Logg("phase", $"{border} reached");
        }

        private void ActivateTalents()
        {
            foreach (var item in talents)
            {
                item.Discover();
            }
        }

        public void ActivateViews(bool toggle)
        {
            foreach (var item in views)
            {
                if (item.gameObject.activeInHierarchy != toggle)
                    item.gameObject.SetActive(toggle);

                var focus = item.GetComponent<CanvasGroupFadeInOut>();

                if (focus != null) fadeInOut = focus;
            }

            if (toggle == true && fadeInOut != null)
                fadeInOut.In();
        }

        

        void ActivateDamageProcessing(bool toggle)
        {
            for (int i = 0; i < damageProcessings.Length; i++)
            {
                var item = damageProcessings[i];

                if (toggle)
                {
                    item.Activate();

                    allActiveDamageProcessings.Add(item);
                }
                else
                {
                    item.Deactivate();
                    Debug.Log("Deactivate "+item);
                    allActiveDamageProcessings.Remove(item);
                }
            }
        }

    }
}
