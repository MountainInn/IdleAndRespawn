using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

abstract public class UnitView<TUnit> : MonoBehaviour
    where TUnit : Unit
{
    [SerializeField]
    public ProgressImage
        attackProgress,
        healthBar;

    [SerializeField] protected Text healthText;
    [SerializeField] FloatingTextMaker damageText;

    protected TUnit unit;


    protected void Awake()
    {
        unit = GameObject.FindObjectOfType<TUnit>();
    }

    protected void Start()
    {
        unit.onTakeDamage +=
            (DoDamageArgs dargs)=>{

                if (dargs.damage._Val == 0) return;

                Color col;

                if (dargs.isReflected) col = Color.blue;
                else if (dargs.isBlindingLight) col = Color.yellow;
                else if (dargs.isDoom) col = Color.magenta;
                else if (dargs.isDiversion) col = new Color(.5f, .1f, .1f);
                else if (dargs.isHotHanded) col = new Color(1f, 0.2f, 0.2f);
                else if (dargs.IsSimpleAttack) col = Color.red;
                else col = Color.black;

                string str = FloatExt.BeautifulFormatSigned(-dargs.damage._Val);

                damageText.SpawnText(str, col);
            };

        unit.onTakeHeal +=
            (DoHealArgs hargs)=>{
                if (hargs.heal == 0) return;

                Color col = Color.green;

                string str = FloatExt.BeautifulFormatSigned(hargs.heal);

                damageText.SpawnText(str, col);
            };

        unit.healthRange.onRatioChanged += UpdateHealthBar;

        unit.attackTimer.onRatioChanged += UpdateAttackProgress;
    }

    protected virtual void UpdateHealthBar(float ratio)
    {
        healthBar.SetValue(ratio);
        healthText.text = unit.healthRange._Val.ToStringFormatted() ;
    }

    protected void UpdateAttackProgress(float ratio)
    {
        attackProgress.SetValue(ratio);
    }
}
