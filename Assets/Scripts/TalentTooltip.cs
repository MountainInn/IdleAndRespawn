using UnityEngine;
using UnityEngine.UI;

public class TalentTooltip : Tooltip
{
    [SerializeField] public Text
        lName,
        lDescription,
        lCost;


    public Talent hoveredTalent;

    public void SetContent(Talent content)
    {
        this.hoveredTalent = content;
        lName.text = hoveredTalent.name;
    }

    void UpdateTalentDescription()
    {
        lDescription.text = hoveredTalent.description + hoveredTalent.updatedDescription;
    }

    void Update()
    {
        if (hoveredTalent != null)
            UpdateTalentDescription();
    }

}











