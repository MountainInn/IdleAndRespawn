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
		}
	}

	public const short MAX_STAGES_COUNT = 1000;

	float
		 stageFraction,
		 stageMult,
		 nextStageHealthThreshold,
		 toNextStage;


	int
		  stageSize;


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
			 damageLeft = dargs.damage._Val,
			 takenDamage = 0;

		UpdateNextStageThreshold(healthSnapshot);

		do
		{
			if (!(dargs.isDiversion || dargs.isReflected || dargs.isBlindingLight))
			{
				damageLeft = Mathf.Max(0, damageLeft - armor.Result);
			}

			if (transcendedStage = (damageLeft > toNextStage))
			{
				damageLeft = Mathf.Max(0, damageLeft - toNextStage);

				takenDamage += toNextStage;

				healthSnapshot -= toNextStage;

				_StageNumber++;

				UpdateNextStageThreshold(healthSnapshot);
			}
		}
		while (damageLeft > 0 && healthSnapshot > 0 && transcendedStage);


		takenDamage += damageLeft;

		float nonOverkill = Mathf.Min(healthRange._Val, takenDamage);

		AffectHP(-nonOverkill);

		dargs.damage._Val = nonOverkill;

		if (dargs.attacker.vampirism != null && dargs.IsSimpleAttack)
			dargs.attacker.Vamp(dargs);

		onTakeDamage.Invoke(dargs);
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
		stageMult = 1 + _StageNumber * Mathf.Log(_StageNumber + 2, 10);

		damageMult.Mutation =
			 armorMult.Mutation =
			 reflectMult.Mutation = stageMult;

	}
}
