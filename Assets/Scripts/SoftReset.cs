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
        onReset, onReincarnation,
        onRespawnCountChanged;

    static public Action<int>
        onMaxStageChanged;

    static public float respawnDuration = 1f;

    [JsonPropertyAttribute] static public int respawnCount
    {
        get => _Inst._respawnCount;
        set
        {
            _Inst._respawnCount = value;

            onRespawnCountChanged?.Invoke();
        }
    }
    int _respawnCount;

    [JsonPropertyAttribute] static public DateTime lastRespawn;

    [JsonPropertyAttribute]
    static public int maxStage = 1, lastStage = 1, stageAcum = 0;


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
            Boss.ResetStageToOne();
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

            Hero._Inst.frags++;

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
        var talentPoints = Boss._Inst._StageNumber * 4;

        Vault.TalentPoints.Earn(talentPoints);
    }

    static public TimeSpan TimeSinceLastReset => DateTime.UtcNow.Subtract(lastRespawn);
}
