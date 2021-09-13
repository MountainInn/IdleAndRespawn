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
                    new CriticalHit(hero),
                    new TakeDamageBarrier(),
                    new TakeDamageFollowers(hero),
                    new TakeDamageArmor(hero),
                    new TakeDamageHealth(hero),
                    new TakeHeal(hero),

                    new CriticalHit(followers),
                    new TakeDamageArmor(followers),
                    new TakeDamageHealth(followers),
                    new TakeHeal(followers),

                    new CriticalHit(boss),
                    // new CurseOfDoom(ReferenceHeap._Inst.curseOfDoomProgress)
                },

                talents = new Talent[]
                {
                },
                views = heroInterface.GetComponentsInChildren<Transform>()
                .Where(tr => tr.GetComponent<TalentView>() == null).ToArray()
            },
            new Phase(11) {talents = new Talent[] {
                    new FullBlood(hero)
                },
            },
            new Phase(23) {talents = new Talent[] {
                    new Block(hero),
                },
            },
            new Phase(34) {talents = new Talent[] {
                    new Ressurection(hero),
                },
            },
            new Phase(45) {talents = new Talent[] {
                    new BattleExpirience(hero)
                },
            },new Phase(70) {talents = new Talent[] {
                    new Interruption(hero)
                },
            },new Phase(100) {talents = new Talent[] {
                    new TitansGrowth(hero),
                    new StaminaTraining(followers)
                },
            },new Phase(150) {talents = new Talent[] {
                    new BlindingLight(hero)
                },
            },new Phase(160) {talents = new Talent[] {
                    new Blitz()
                },
            },new Phase(190) {talents = new Talent[] {
                    new Regeneration(hero),
                },
            },new Phase(230) {talents = new Talent[] {
                    new CoordinatedActions(followers)
                },
            },new Phase(270) {talents = new Talent[] {
                    new Diversion(followers)
                },
            },new Phase(300) {talents = new Talent[] {
                    new EnfeeblingStrike(hero)
                },
            },new Phase(320) {talents = new Talent[] {
                    new CounterAttack(hero)
                },
            },new Phase(360) {talents = new Talent[] {
                    new Rebirth(hero)
                },
            },new Phase(400) {talents = new Talent[] {
                    new BloodMadness(hero)
                },
            }, new Phase(470) {talents = new Talent[] {
                    new Multicrit(hero)
                },
            }, new Phase(530) {talents = new Talent[] {
                    new VeteransOfThirdWar(),
                },
            },new Phase(560) {talents = new Talent[] {
                    new Cyclone(hero)
                },
            },new Phase(600) {talents = new Talent[] {
                    new CounterAttack(hero)
                },
            },new Phase(650) {talents = new Talent[] {
                    new HotHand(hero)
                },
            }
            ,new Phase(700) {talents = new Talent[] {
                    new Dejavu(hero)
                },
            }
        };

        foreach (var phase in phases)
        {
            phase.InitTalentCosts();
        }

        foreach(var item in TalentView.instances )
        {
            item.gameObject.SetActive(false);
        }

        Talent.onActivation += (tal)=>{ allActiveTalents.Add(tal); };

        CheckPhase();
        Boss._Inst.onStageChanged += CheckPhase;
    }

    public static Action<Phase> onPhaseActivated;

    void CheckPhase()
    {
        foreach (var currentPhase in phases)
        {
            if (!currentPhase.active && currentPhase.CanActivate())
            {
                currentPhase.ActivatePhase(true);

                onPhaseActivated?.Invoke(currentPhase);
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

        public void InitTalentCosts()
        {
            foreach (var item in talents)
            {
                item.SetPhase(border);
            }
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
        }

        private void ActivateTalents()
        {
            foreach (var item in talents)
            {
                item.Discover();

                allDiscoveredTalents.Add(item);
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
