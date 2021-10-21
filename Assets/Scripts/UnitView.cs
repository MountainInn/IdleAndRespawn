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
    [SerializeField] MorfText morfText;

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
                else if (dargs.isDiversion) col = new Color(.6f, .4f, .4f);
                else if (dargs.isHotHanded) col = new Color(1f, 0.2f, 0.2f);
                else if (dargs.IsSimpleAttack || dargs.isInterrupted) col = Color.red;
                else col = Color.black;

                if (dargs.isCritical)
                    morfText.SetBigFontsize();
                else
                    morfText.SetNormalFontsize();


                string str = FloatExt.BeautifulFormatSigned(-dargs.damage._Val);

                morfText.Morf(str, col);
            };

        unit.onTakeHeal +=
            (DoHealArgs hargs)=>{
                if (hargs.heal == 0) return;

                Color col = Color.green;

                morfText.SetNormalFontsize();

                string str = FloatExt.BeautifulFormatSigned(hargs.heal);

                morfText.Morf(str, col);
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
