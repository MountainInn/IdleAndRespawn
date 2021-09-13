using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class SoftReset : MonoBehaviour
{
    static public SoftReset _Inst;
    static public Action
        reset, reincarnation,
        onReset, onReincarnation;

    static public Action<int>
        onMaxStageChanged;

    static public float respawnDuration = 2;

    [JsonPropertyAttribute] static public int respawnCount = 0;

    [JsonPropertyAttribute] static public DateTime lastRespawn;

    [JsonPropertyAttribute]
    static public int maxStage = 1, lastStage = 1;


    [OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext context)
    {
        UpdateMaxStage();
    }

    void Awake()
    {
        _Inst = this;


        reset = () =>
        {
            StagesToTalentPoints();

            lastStage = Boss._Inst._StageNumber;
            UpdateMaxStage();


            Boss._Inst.ResetStages();
            Boss._Inst.shield.Out();

            // Hero._Inst.CutoffAttackTimer();
            StartCoroutine(Hero._Inst.OnDeath());
            StartCoroutine(Followers._Inst.OnDeath());
            StartCoroutine(Boss._Inst.OnHeroDeath());


            lastRespawn = DateTime.UtcNow;

            respawnCount++;

            onReset?.Invoke();
        };

        reincarnation = ()=>
        {
            Boss._Inst._StageNumber =
            lastStage =
                maxStage = 1;


            Boss._Inst.ResetStages();
            Boss._Inst.shield.Out();
            UpdateMaxStage();

            Hero._Inst.CutoffAttackTimer();
            StartCoroutine(Hero._Inst.OnDeath());
            StartCoroutine(Followers._Inst.OnDeath());
            StartCoroutine(Boss._Inst.OnDeath());


            lastRespawn = DateTime.UtcNow;

            onReset?.Invoke();
            onReincarnation?.Invoke();
        };

    }

    public static void UpdateMaxStage()
    {
        if (maxStage < Boss._Inst._StageNumber)
        {
            maxStage = Boss._Inst._StageNumber;

            onMaxStageChanged?.Invoke(maxStage);
        }
    }

    void Start()
    {
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

    static public TimeSpan TimeSinceLastReset => DateTime.UtcNow.Subtract(lastRespawn);
}
