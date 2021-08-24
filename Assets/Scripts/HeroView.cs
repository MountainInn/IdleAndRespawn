using UnityEngine;
using UnityEngine.UI;

public class HeroView : UnitView<Hero>
{
    [SerializeField]
    ProgressImage
        healProgress;

    new void Update()
    {
        base.Update();

        healProgress.SetValue(unit.healingTimer.GetRatio());
    }
}
