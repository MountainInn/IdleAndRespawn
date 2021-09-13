using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    static public Tooltip _Inst;
    
    RectTransform rectTransform;

    public CanvasGroupFadeInOut fadeInOut;


    void Awake()
    {
        _Inst = this;

        rectTransform = GetComponent<RectTransform>();

        fadeInOut = GetComponent<CanvasGroupFadeInOut>();
    }

    public void PositionTooltip(Vector2 ttPosition)
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        bool
            isLeftside = ttPosition.x < screenCenter.x,
            isBottomside = ttPosition.y < screenCenter.y;


        Vector2 pivot
            = new Vector2(
                isLeftside ? 0 : 1,
                isBottomside ? 0 : 1
            );

        rectTransform.pivot = pivot;
        rectTransform.position = ttPosition ;
    }
}
