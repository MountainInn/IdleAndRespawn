using UnityEngine;
using UnityEngine.UI;
using System;

public class MilestoneView :MonoBehaviour
{
    [SerializeField] Text levelTxt, nameTxt, descriptionTxt;
    [SerializeField] Color reachedColor, notReachedColor;
    [SerializeField] Image icon;
    Image selfImage;

    Sprite reachedIcon;

    void Awake()
    {
        selfImage = GetComponent<Image>();
    }

    public void SetView(int level, AdTalent ADTalent)
    {
        levelTxt.text = level.ToString();
        reachedIcon = ADTalent.buffIcon.sprite;
        icon.sprite = ReferenceHeap._Inst.milestoneNotReachedIcon;

        UpdateView(ADTalent);
    }

    public void UpdateView(AdTalent ADTalent)
    {
        nameTxt.text = ADTalent.name;

        descriptionTxt.text = ADTalent.description;
    }

    public void SetReached()
    {
        selfImage.color =
            levelTxt.color=
            nameTxt.color=
            descriptionTxt.color = reachedColor;

        icon.sprite = reachedIcon;
    }

    public void SetNotReached()
    {
        selfImage.color =
            levelTxt.color =
            nameTxt.color =
            descriptionTxt.color = notReachedColor;

        icon.sprite = ReferenceHeap._Inst.milestoneNotReachedIcon;
    }
}
