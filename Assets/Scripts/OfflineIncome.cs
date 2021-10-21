using UnityEngine;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class OfflineIncome : MonoBehaviour
{
    static OfflineIncome inst;
    static public OfflineIncome _Inst => inst ??= GameObject.FindObjectOfType<OfflineIncome>();

    [JsonPropertyAttribute]
    DateTime exitDate;
    DateTime comebackDate;


    int respawnsPerMinute = 2;


    void OnEnable()
    {
        UpdateComebackDate();
    }

    private void UpdateComebackDate() => comebackDate = DateTime.UtcNow;


    [OnSerializingAttribute]
    void OnSerializing(StreamingContext sc)
    {
        UpdateExitDate();
    }

    private void UpdateExitDate() => exitDate = DateTime.UtcNow;


    [OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext sc)
    {
        RecieveOfflineIncome();
    }

    private void RecieveOfflineIncome()
    {
        if (exitDate == default || comebackDate.CompareTo(exitDate) <= 0) return;

        TimeSpan difference = comebackDate.Subtract(exitDate);

        float income = EarnOfflineIncome(difference);

        int respawns = EarnOfflineRespawns(difference);

        AdCharges._Inst.AddOfflineTime(difference);

        OfflineIncomeView._Inst.Show(difference, income, respawns);
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            UpdateExitDate();
        }
        else
        {
            UpdateComebackDate();
            RecieveOfflineIncome();
        }
    }

    float EarnOfflineIncome(TimeSpan difference)
    {
        float incomeMult = AdProgression._Inst.Mult;

        float income = (float)
            (difference.TotalSeconds * BattleExpiriense._Inst.maxExpPerHit * incomeMult);

        Vault.Expirience.Earn(income);

        return income;
    }

    int EarnOfflineRespawns(TimeSpan difference)
    {
        int respawns = Mathf.FloorToInt( (float)difference.TotalMinutes * respawnsPerMinute * AdProgression._Inst.Mult );

        SoftReset.respawnCount += respawns;

        return respawns;
    }


}
