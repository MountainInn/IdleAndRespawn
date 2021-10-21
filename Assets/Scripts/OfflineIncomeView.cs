using System;
using UnityEngine;
using UnityEngine.UI;

public class OfflineIncomeView : MonoBehaviour
{
    static OfflineIncomeView inst;
    static public OfflineIncomeView _Inst => inst??=GameObject.FindObjectOfType<OfflineIncomeView>();

    [SerializeField] CanvasGroup panel;
    [SerializeField] Text greetings;
    [SerializeField] Button @continue;


    void Awake()
    {
        @continue.onClick.AddListener(Hide);
    }

    void Hide()
    {
        panel.alpha = 0;
        panel.interactable = panel.blocksRaycasts = false;
    }

    public void Show(TimeSpan offlineTime, float income, int respawns)
    {
        greetings.text =
            "Welcome Back!\n"+
            "\n"+
            $"In {offlineTime.ToStringFormattedWithDays()} of offline time\n"+
            "\n"+
            "Hero respawned:\n"+
            $"{respawns.ToStringFormatted()} times"+
            "\n"+
            "and earned:\n"+
            $"{income.ToStringFormatted()} Expirience";

        panel.alpha = 1;
        panel.interactable = panel.blocksRaycasts = true;
    }

}
