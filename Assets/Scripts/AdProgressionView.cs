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
    
    Advertisement advertisement;


    void SubscribeToAdvertisement()
    {
        advertisement.cooldown.onRatioChanged += (ratio)=>{ UpdateAdCooldownText(); UpdateOrbFill(ratio); };
    }

    void Start()
    {
        advertisement = GameObject.FindObjectOfType<Advertisement>();

        UpdateAdCooldownText();
        UpdateOrbFill(advertisement.cooldown.GetRatio());

        SubscribeToAdvertisement();
    }

    void UpdateAdCooldownText()
    {
        string str;

        if (advertisement.cooldown.isFinished)
        {
            str = "Ready!";
        }
        else
        {
            var seconds = advertisement.cooldown.endTime - advertisement.cooldown.t;
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
        Debug.Log("Lifted "+liftedTalent.name);
        milestones[liftedTalent].SetReachedColor();
        milestones[liftedTalent].UpdateView(liftedTalent);
    }

    public void UpdateLevel(int currentLevel)
    {
        progBarOver.fillAmount = (float)currentLevel / maxLevel;

        currentLevelText.text = $"Level {currentLevel}";

        string percentStr = ( AdProgression._Inst.mult - 1f).ToString("P0");

        percentMultText.text = percentStr;

        percentDescriptionText.text = $"+{percentStr} attack & defense to hero & followers";
    }
}
