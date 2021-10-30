using UnityEngine;
using UnityEngine.UI;

public class LiftedBar : MonoBehaviour
{
    float height;


    void Awake()
    {
        height = GetComponent<RectTransform>().rect.height;
    }


    float MilestoneParentY(float ratio)
    {
        return GetComponent<RectTransform>().rect.yMax + MilestoneBarY(ratio);
    }

    float MilestoneBarY(float ratio)
    {
        return height * ratio;
    }
}
