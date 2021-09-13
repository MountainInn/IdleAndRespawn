using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class AdProgressionView : MonoBehaviour
{
    [SerializeField] Image progBarUnder, progBarOver;
    [SerializeField] Image orbImage;
    [SerializeField] MilestoneView prefMilestone;
    [SerializeField]
    Text
        currentLevelText,
        percentMultText,
        percentDescriptionText,
        adCooldownText;

    int maxLevel;

    Dictionary<AdProgression.LiftedTalent, MilestoneView> milestones = new Dictionary<AdProgression.LiftedTalent, MilestoneView>();
    

    void SubscribeToAdvertisement()
    {
        Advertisement._Inst.cooldown.onRatioChanged += (ratio)=>{ UpdateAdCooldownText(); UpdateOrbFill(ratio); };

    }

    void Start()
    {
        UpdateAdCooldownText();
        UpdateOrbFill(Advertisement._Inst.cooldown.GetRatio());

        SubscribeToAdvertisement();
    }

    void UpdateAdCooldownText()
    {
        string str;

        if (Advertisement._Inst.cooldown.isFinished)
        {
            str = "Ready!";
        }
        else
        {
            var seconds = Advertisement._Inst.cooldown.endTime - Advertisement._Inst.cooldown.T;
            TimeSpan timespan = TimeSpan.FromSeconds(seconds);
            str = timespan.ToString();
        }
        
        adCooldownText.text = str;
    }

    public void UpdateOrbFill(float fillAmount)
    {
        orbImage.fillAmount = fillAmount;
    }

    public void InitMilestones()
    {
        maxLevel = AdProgression._Inst.liftTalents.MaxFloor;

        var floors = AdProgression._Inst.liftTalents.floors;

        foreach(var level in floors.Keys)
        {
            float levelFraction = (float)level / maxLevel;

            float yPos = progBarOver.rectTransform.rect.height * levelFraction;

            var position = new Vector3(20, yPos);

            MilestoneView mView = Instantiate(prefMilestone);
            mView.transform.SetParent(progBarOver.transform);
            mView.transform.localScale = Vector3.one;
            mView.transform.localPosition = position;

            mView.SetView(level, floors[level]);

            milestones.Add(floors[level], mView);
        }
    }

    public void UpdateMilestones(AdProgression.LiftedTalent liftedTalent)
    {
        milestones[liftedTalent].SetReachedColor();
        milestones[liftedTalent].UpdateView(liftedTalent);
    }

    public void UpdateLevel(int currentLevel)
    {
        progBarOver.fillAmount = (float)currentLevel / maxLevel;

        currentLevelText.text = $"Level {currentLevel}";

        string percentStr = ( AdProgression._Inst.Mult - 1f).ToString("P0");

        percentMultText.text = percentStr;

        percentDescriptionText.text = $"+{percentStr} attack & defense to hero & followers";
    }
}
