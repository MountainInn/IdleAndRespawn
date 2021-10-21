using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MorfText : MonoBehaviour
{
    [SerializeField] public int normalFontsize;

    Text self;
    int
        fontsize,
        bigFontsize;

    GoTweenConfig goConfig;
    GoTween goTween;


    void Awake()
    {
        goConfig = new GoTweenConfig()
            .setIterations(1)
            .setDelay(.2f)
            .setEaseType(GoEaseType.Linear);

        self = GetComponent<Text>();

        fontsize = normalFontsize;
        bigFontsize = (int)(normalFontsize * 1.5f);
    }

    public void SetBigFontsize()
    {
        fontsize = bigFontsize;
    }
    public void SetNormalFontsize()
    {
        fontsize = normalFontsize;
    }

    public void Morf(string str, Color color)
    {
        self.text = str;
        self.color = color;

        goConfig.clearProperties();

        if (goTween != null)
            Go.removeTween(goTween);

        goTween = Go.to(self,
                        .5f,
                        goConfig.colorProp("color", color.SetA(0)));
    }
}
