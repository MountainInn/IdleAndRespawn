using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class AdProgressionView : MonoBehaviour
{
    static AdProgressionView inst;
    static public AdProgressionView _Inst => inst??=GameObject.FindObjectOfType<AdProgressionView>();


    [SerializeField] Image progBarUnder, progBarOver;
    [SerializeField] Image orbImage;
    [SerializeField] MilestoneView prefMilestone;
    [SerializeField]
    Text
        currentLevelText,
        percentMultText,
        percentDescriptionText,
        watchAdButtonText,
        adChargesCounter;
    public Text adCooldownText;

    [SerializeField]
    Sprite
        blueOrb,
        grayOrb;

    int maxLevel;

    bool adIsOk;

    Dictionary<AdTalent, MilestoneView> milestones = new Dictionary<AdTalent, MilestoneView>();

    void Awake()
    {
        SwitchToGray();
    }

    void Start()
    {
        AdCharges.onChargesChanged += UpdateChargesText;

        UpdateChargesText();
    }


    private void UpdateChargesText()
    {
        adChargesCounter.text = AdCharges._Inst.AdChargesToString();
    }


    public void SwitchToGray()
    {
        AdCharges._Inst.cooldown.onRatioChanged -= UpdateAdCooldownText;

        adCooldownText.text = "No Internet";

        orbImage.sprite = grayOrb;
    }

    public void SwitchToBlue()
    {
        AdCharges._Inst.cooldown.onRatioChanged += UpdateAdCooldownText;

        UpdateAdCooldownText();

        orbImage.sprite = blueOrb;
    }


    void UpdateAdCooldownText(float ratio = 0)
    {
        if (AdCharges.IsGreaterThanZero)
        {
            adCooldownText.text = "Ad is Ready!";
        }
        else
        {
            adCooldownText.text = AdCharges.StringTimeToCharge();
        }
    }

    public void UpdateOrbFill(float fillAmount)
    {
        if (!AdCharges.IsFull)
            orbImage.fillAmount = fillAmount;
    }

    public void InitMilestones()
    {
        maxLevel = AdProgression._Inst.liftTalents.MaxFloor;

        var floors = AdProgression._Inst.liftTalents.floors;

        foreach(var level in floors.Keys)
        {
            float levelFraction = (float)level / maxLevel;

            float overHeight = progBarOver.rectTransform.rect.height;
            float yPos = progBarUnder.rectTransform.anchoredPosition.y + overHeight * levelFraction;

            var position = new Vector3(0, yPos);

            MilestoneView mView = Instantiate(prefMilestone);
            mView.transform.SetParent(transform);
            mView.transform.localScale = Vector3.one;
            RectTransform mViewRect = mView.GetComponent<RectTransform>();
            mViewRect.anchoredPosition = position;
            mViewRect.sizeDelta = mViewRect.sizeDelta.SetX(-40);

            mView.SetView(level, floors[level]);

            milestones.Add(floors[level], mView);
        }
    }
    public void UpdateMilestones(AdTalent liftedTalent)
    {
        milestones[liftedTalent].SetReached();
        milestones[liftedTalent].UpdateView(liftedTalent);
    }
    public void UpdateLevel(int currentLevel)
    {
        progBarOver.fillAmount = (float)currentLevel / maxLevel;

        currentLevelText.text = $"Level {currentLevel}";

        string percentStr = ( AdProgression._Inst.Mult - 1f).ToString("P0");

        percentMultText.text = percentStr;

        percentDescriptionText.text = $"+{percentStr} attack & defense to hero & followers\n+{percentStr} offline income";
    }
}
