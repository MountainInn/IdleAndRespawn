using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public partial class Boss : Unit
{
    static Boss inst;
    static public Boss _Inst => inst ??= GameObject.FindObjectOfType<Boss>();
    new static public List<Boss> _Instances = new List<Boss>();

    static public List<Boss> GetAllInstances()
    {
        return new List<Boss>(){ _Inst };
    }


    public int bossSoulsReward = 1;

    BossView view;

    protected override void OnAwake()
    {
        _Instances.Add(this);

        doomFadeInOut = ReferenceHeap._Inst.curseOfDoomProgress.GetComponent<CanvasGroupFadeInOut>();
        view = GameObject.FindObjectOfType<BossView>();
        target = GameObject.FindObjectOfType<Hero>();


        AddStageMult();

        AwakeReincarnations();


        onTakeDamage +=
            (args) =>{
                BattleExpiriense.MakeExpiriense(args);
                PutUpShield(args);
            };

        stageFraction = 1f / MAX_STAGES_COUNT;
        stageSize = ((int) ( healthRange._Max * stageFraction ) );
        UpdateNextStageThreshold(healthRange._Val);

        takeDamageChain.Add(100, BossTakeDamage_New);

        SoftReset.onReset += ()=>{ ShieldHide(); };

        onDeathChain.Add(100, (unit)=>{ SoftReset.reincarnation.Invoke(); });

    }

    void Start()
    {
        UpdateStageMult();
    }

    public IEnumerator OnHeroDeath()
    {
        ableToFight = false;

        yield return RecoverOnRespawn();

        ableToFight = true;
    }
    static public Action onBossRespawned;
    new public IEnumerator OnDeath()
    {
        var wait = new WaitForSeconds(SoftReset.respawnDuration / 2);

        ableToFight = false;

        CutoffAttackTimer();

        yield return view.bossInterfaceFadeinout.FadingOut();


        Hero._Inst.frags++;

        yield return wait;

        Respawn();

        yield return wait;


        onBossRespawned?.Invoke();

        ableToFight = true;
    }


    public void Respawn()
    {
        healthRange.ResetToMax();

        view.bossInterfaceFadeinout.In();
    }

    override protected void FirstInitStats()
    {
        damage = new StatMultChain(// 2.1f

            60
            , 0, 0);

        attackSpeed = new StatMultChain(3, 0, 0){ isPercentage = true };

        critChance = new StatMultChain(.00f, 0.0f, 500){ isPercentage = true };

        critMult = new StatMultChain(1.5f, .01f, 500){ isPercentage = true };

        armor = new StatMultChain(
        2
        , 0, 0);

        InitHealth(1e6f, 0, 0);

        reflect = new StatMultChain(0, 0, 0);
    }




    [SerializeField] public CanvasGroupFadeInOut shield;
    [SerializeField] Text shieldText;
    CanvasGroupFadeInOut doomFadeInOut;

    bool heroCantPierce, followersCantPierce;

    [JsonPropertyAttribute]
    public bool shieldPutUp = false;

    void PutUpShield(DoDamageArgs dargs)
    {
        if (!dargs.isReflected && !dargs.isDiversion)
        {
            if (dargs.attacker is Followers) followersCantPierce = dargs.damage._Val < 1;
            else if (dargs.attacker is Hero) heroCantPierce = dargs.damage._Val < 1;
        }

        bool shieldStateNotchanged = shieldPutUp;

        shieldPutUp = (heroCantPierce && (!Followers._Inst.Alive || followersCantPierce ));

        shieldStateNotchanged = shieldStateNotchanged && shieldPutUp;

        if (!shieldStateNotchanged)
            if (shieldPutUp)
            {
                float damageToPenetrate = armor.Result + (1 + reflect.Result);

                shieldText.text = FloatExt.BeautifulFormatSigned(damageToPenetrate);
                ShieldShow();
            }
            else
            {
                ShieldHide();
            }
    }

    private void ShieldHide()
    {
        shield.Out();
        // doomFadeInOut.Out(); ;
    }

    void ShieldShow()
    {
        shield.In();
        // doomFadeInOut.In();
    }

}
