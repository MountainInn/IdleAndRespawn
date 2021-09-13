using UnityEngine;
using UnityEngine.UI;

public class SoftResetView : MonoBehaviour
{
    [SerializeField]
    Text
    highestStageText,
    lastStageText,
    respawnCountText;

    void Awake()
    {
    }
    void Start()
    {
        Boss._Inst.onStageChanged += UpdateText;
        SoftReset.onReset += UpdateText;
        UpdateText();
    }

    void UpdateText()
    {
        highestStageText.text = $"Highest Stage: {SoftReset.maxStage}";
        lastStageText.text = $"Last Stage: {SoftReset.lastStage}";
        respawnCountText.text = $"Respawns: {SoftReset.respawnCount}";
    }



}
