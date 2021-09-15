using UnityEngine;
using UnityEngine.UI;

public class BossView : UnitView<Boss>
{
    [SerializeField] Text stageText, reincarnationText;
    [SerializeField] public CanvasGroupFadeInOut bossInterfaceFadeinout;
    [SerializeField] Material hpMaterial;
    int oneMinusHealthRatio;

    new void Start()
    {
        base.Start();

        UpdateStageNumber();
        unit.onStageChanged += UpdateStageNumber;
        SoftReset.onReset += UpdateStageNumber;
        Hero.onFragsUpdated += UpdateReincarnationNumber;

        oneMinusHealthRatio = Shader.PropertyToID("One_Minus_Health_Ratio");
    }

    protected override void UpdateHealthBar(float ratio)
    {
        float fill = Mathf.Clamp(1f - ratio, 0.001f, 0.999f);
        hpMaterial.SetFloat(oneMinusHealthRatio, fill);

        healthText.text = FloatExt.BeautifulFormat(unit.healthRange._Val);
    }

    void UpdateStageNumber()
    {
        stageText.text = "Stage: " + unit._StageNumber.ToString();
    }


    void UpdateReincarnationNumber(int frags)
    {
        if (!reincarnationText.gameObject.activeSelf)
                reincarnationText.gameObject.SetActive(true);

        reincarnationText.text = "Reincarnation: " + frags;
    }
}
