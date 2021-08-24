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

    [SerializeField] Text healthText;
    [SerializeField] FloatingTextMaker damageText;

    protected TUnit unit;


    protected void Awake()
    {
        unit = GameObject.FindObjectOfType<TUnit>();
    }

    protected void Start()
    {
        unit.healthRange.onValueChanged +=
            (diff) =>{
                if (!unit.ableToFight || diff == 0) return;

                string str = FloatExt.BeautifulFormatSigned(diff);

                if (diff < 0) damageText.SpawnTextDamage(str);
                else damageText.SpawnTextHeal(str);
            };
    }

    protected void Update()
    {
        healthBar.SetValue(unit.healthRange.GetRatio());

        attackProgress.SetValue(unit.attackTimer.GetRatio());

        healthText.text = FloatExt.BeautifulFormat( unit.healthRange._Val );
    }
    
}
