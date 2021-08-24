using UnityEngine;
using UnityEngine.UI;

public class TalentTooltip : Tooltip
{
    [SerializeField] public Text
        lName,
        lDescription,
        lCost;


    public Talent talent;

    public void SetContent(Talent content)
    {
        this.talent = content;
        lName.text = talent.name;

        lCost.text = (talent.isBought) ? "Acquired" : talent.cost.ToString();
    }

    void UpdateTalentDescription()
    {
        lDescription.text = talent.description + talent.updatedDescription;
    }

    void Update()
    {
        if (talent != null)
            UpdateTalentDescription();
    }

}











