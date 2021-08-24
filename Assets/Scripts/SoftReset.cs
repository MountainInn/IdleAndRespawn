using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class SoftReset : MonoBehaviour
{
    static public SoftReset _Inst;
    static public Action reset;
    static public Action onReset, onBossSpawn;
    static public float respawnDuration = 2;

    [JsonPropertyAttribute] static public float lastReset;

    [JsonPropertyAttribute]
    static public int maxStage = 1, lastStage=1;


    public void OnDeserialize()
    {
        UpdateMaxStage();
    }

    void Awake()
    {
        _Inst = this;


        reset += () =>
        {
            lastStage = Boss._Inst._StageNumber;
            UpdateMaxStage();

            StagesToTalentPoints();

            Boss._Inst.ResetStages();
            Boss._Inst.shield.Out();

            Hero._Inst.CutoffAttackTimer();
            StartCoroutine(Hero._Inst.OnDeath());
            StartCoroutine(Followers._Inst.OnDeath());
            StartCoroutine(Boss._Inst.OnHeroDeath());


            lastReset = Time.time;

            GameLogger.Logg("respawn", "");

            onReset?.Invoke();
        };

        onBossSpawn += ()=>
        {
            lastStage =
                maxStage = 1;

            Boss._Inst.ResetStages();
            Boss._Inst.shield.Out();

            Hero._Inst.CutoffAttackTimer();
            StartCoroutine(Hero._Inst.OnDeath());
            StartCoroutine(Followers._Inst.OnDeath());
            StartCoroutine(Boss._Inst.OnDeath());
        };

    }

    public static void UpdateMaxStage()
    {
        maxStage = Mathf.Max(maxStage, Boss._Inst._StageNumber);
    }

    void Start()
    {
        lastReset = Time.time;

        Hero._Inst.onDeathChain.Add(int.MaxValue, (unit) => reset.Invoke());
    }

    
    static void StagesToTalentPoints()
    {
        // var halfMaxStage = maxStage / 2;

        // var talentPoints =
        //     Mathf.Pow(Boss._Inst._StageNumber - halfMaxStage, 3)
        //     / Mathf.Pow(maxStage, 2) * 10;

        // talentPoints = Mathf.Ceil(Mathf.Clamp(talentPoints, 1, float.MaxValue));

        var talentPoints = Boss._Inst._StageNumber;

        Vault.talentPoints.Earn(talentPoints);
    }

    static public float TimeSinceLastReset => Time.time - lastReset;
}

