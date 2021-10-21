using System;
using UnityEngine;
using UnityEngine.UI;

public class NewStagesThisSession : MonoBehaviour
{
    Text self;
    int stagesStart,
        difference;

    void Awake()
    {
        self = GetComponent<Text>();

        SaveSystem.onAfterLoad += OnAfterLoaded;
        void OnAfterLoaded()
        {
            stagesStart = SoftReset.maxStage;
            UpdateText();

            SoftReset.onMaxStageChanged += (newMaxStage) =>
            {
                difference = newMaxStage - stagesStart;
                UpdateText();
            };

            SaveSystem.onAfterLoad -= OnAfterLoaded;
        }

    }


    private void UpdateText()
    {
        self.text = "+" + difference;
    }
}
