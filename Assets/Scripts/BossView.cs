using UnityEngine;
using UnityEngine.UI;

public class BossView : UnitView<Boss>
{
    [SerializeField] Text stageText, reincarnationText;
    [SerializeField] public CanvasGroupFadeInOut bossInterfaceFadeinout;

    new void Start()
    {
        base.Start();

        UpdateStageNumber();

        unit.onStageChanged += UpdateStageNumber;

        SoftReset.onReset += UpdateStageNumber;

        SoftReset.onBossSpawn += ()=>
        {
            if (!reincarnationText.gameObject.activeSelf)
                reincarnationText.gameObject.SetActive(true);

                
            UpdateReincarnationNumber();
        };
    }

    void UpdateStageNumber()
    {
        stageText.text = "Stage: " + unit._StageNumber.ToString();
    }

    void UpdateReincarnationNumber()
    {
        reincarnationText.text = "Reincarnation: " + PlayerStats._Inst.bossKilled + 1;
    }

}
