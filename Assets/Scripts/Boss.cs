using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

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

        InitReincarnationMult();
        AddReincarnationMult();


        onTakeDamage +=
            (args) =>{
                BattleExpiriense.MakeExpiriense(args);
                PutUpShield(args);
            };

        stageSize = ((int) ( healthRange._Max * stageFraction ) );
        UpdateNextStageThreshold(healthRange._Val);

        takeDamageChain.Add(100, BossTakeDamage_New);

        SoftReset.onReset += ()=>{ ShieldHide(); };
    }

    void Start()
    {
        UpdateReincarnationMult();
        UpdateStageMult();
    }

    public IEnumerator OnHeroDeath()
    {
        ableToFight = false;

        yield return RecoverOnRespawn();

        ableToFight = true;
    }

    new public IEnumerator OnDeath()
    {
        float waitTime = SoftReset.respawnDuration / 2;

        view.bossInterfaceFadeinout.Out();

        ableToFight = false;

        RegisterDeath();

        yield return new WaitForSeconds(waitTime);

        Respawn();

        yield return new WaitForSeconds(waitTime);

        ableToFight = true;
    }

    void RegisterDeath()
    {
        Vault.bossSouls.Earn(bossSoulsReward);

        PlayerStats._Inst.bossKilled++;
    }

    public void Respawn()
    {
        healthRange.ResetToMax();

        view.bossInterfaceFadeinout.In();
    }

    override protected void FirstInitStats()
    {
        damage = new StatMultChain(// 2.1f
            70f, 0, 0);

        attackSpeed = new StatMultChain(5, 0, 0){ isPercentage = true };

        critChance = new StatMultChain(.00f, 0.0f, 500){ isPercentage = true };

        critMult = new StatMultChain(1.5f, .01f, 500){ isPercentage = true };

        armor = new StatMultChain(20, 0, 0);

        InitHealth(1e6f, 0, 0);

        reflect = new StatMultChain(0.01f, .0f, 0){ isPercentage = true };
    }




    [SerializeField] public CanvasGroupFadeInOut shield;
    [SerializeField] Text shieldText;
    CanvasGroupFadeInOut doomFadeInOut;

    bool heroCantPierce, followersCantPierce;

    [JsonPropertyAttribute]
    public bool shieldPutUp = false;

    void PutUpShield(DoDamageArgs dargs)
    {
        if (!dargs.isReflected)
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
        doomFadeInOut.Out(); ;
    }

    void ShieldShow()
    {
                shield.In();
                doomFadeInOut.In();
    }

}
