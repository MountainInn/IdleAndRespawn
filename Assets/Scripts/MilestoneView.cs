using UnityEngine;
using UnityEngine.UI;
using System;

public class MilestoneView :MonoBehaviour
{
    [SerializeField] Text levelTxt, nameTxt, descriptionTxt; 
    [SerializeField] Color reachedColor, notReachedColor;
    Image selfImage;

    void Awake()
    {
        selfImage = GetComponent<Image>();
    }

    public void SetView(int level, AdProgression.LiftedTalent ADTalent)
    {
        levelTxt.text = level.ToString();

        UpdateView(ADTalent);
    }

    public void UpdateView(AdProgression.LiftedTalent ADTalent)
    {
        nameTxt.text = ADTalent.name;

        descriptionTxt.text = ADTalent.description;
    }

    public void SetReachedColor()
    {
        selfImage.color =
            levelTxt.color=
            nameTxt.color=
            descriptionTxt.color = reachedColor;
    }

    public void SetNotReachedColor()
    {
        selfImage.color =
            levelTxt.color=
            nameTxt.color=
            descriptionTxt.color = notReachedColor;
    }
}
