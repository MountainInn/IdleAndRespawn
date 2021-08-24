using System;
using System.Globalization;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

public partial class Boss
{
    int stageNumber = 1;

    [JsonPropertyAttribute]
    public int _StageNumber
    {
        get => stageNumber;
        set
        {
            stageNumber = value;
            OnStageChanged();

            GameLogger.Logg("bossstage", $"{stageNumber}");
        }
    }

    float
        stageFraction = 0.0002f,
        stageMult,
        nextStageHealthThreshold,
        toNextStage;

    int
        stageSize;

    [OnDeserializedAttribute]
    public void OnDeserialize_UpdateStages(StreamingContext context)
    {
        onStageChanged?.Invoke();

        OnStageChanged();
    }

    [JsonPropertyAttribute]
    ArithmeticNode
        damageMult = new ArithmeticNode(new ArithmMult(), 1),
        armorMult = new ArithmeticNode(new ArithmMult(), 1),
        reflectMult = new ArithmeticNode(new ArithmMult(), 1)
        ;

    public event Action onStageChanged;


    void AddStageMult()
    {
        damage.chain.Add(damageMult);
        armor.chain.Add(armorMult);
        reflect.chain.Add(reflectMult);
    }

    public void ResetStages()
    {
        _StageNumber = 1;

        UpdateStageMult();
    }


    public void BossTakeDamage_New(DoDamageArgs dargs)
    {
        bool
            transcendedStage;
        float
            healthSnapshot = healthRange._Val,
            damage = dargs.damage._Val,
            takenDamage = 0;

        UpdateNextStageThreshold(healthSnapshot);

        do
        {
            if (!dargs.isReflected)
            {
                damage -= Mathf.Min(reflect.Result, .35f) * damage;
                damage = Mathf.Max(0, damage - armor.Result);
            }

            if (transcendedStage = (damage > toNextStage))
            {
                damage = Mathf.Max(0, damage - toNextStage);
                takenDamage += toNextStage;

                healthSnapshot -= toNextStage;


                _StageNumber++;


                UpdateNextStageThreshold(healthSnapshot);
            }
        }
        while (damage > 0 && transcendedStage);


        takenDamage += damage;

        AffectHP(-takenDamage);

        dargs.damage._Val = takenDamage;
    }

    public void OnStageChanged()
    {
        SoftReset.UpdateMaxStage();

        onStageChanged?.Invoke(); ;

        UpdateStageMult();
    }

    void UpdateNextStageThreshold(float currentHealth)
    {
        nextStageHealthThreshold = healthRange._Max * (1.0f - stageFraction * stageNumber);

        toNextStage = currentHealth - nextStageHealthThreshold;
    }
    

    void UpdateStageMult()
    {
        int stage = _StageNumber;

        float a = (1 + Mathf.Floor(stage)) / 8f;
        float b = (1 + Mathf.Floor(stage)) / 2f;

        stageMult = a * b;

        string multstr = stageMult.ToString("##.##", CultureInfo.InvariantCulture);
        // stage_mult_output += $"({stage},{multstr}),";

        damageMult.Mutation =
            armorMult.Mutation =
            reflectMult.Mutation = stageMult;

    }
}
