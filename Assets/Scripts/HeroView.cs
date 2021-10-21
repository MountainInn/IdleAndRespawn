using UnityEngine;
using UnityEngine.UI;

public class HeroView : UnitView<Hero>
{
    static HeroView inst;
    static public HeroView _Inst => inst??=GameObject.FindObjectOfType<HeroView>();

    [SerializeField]
    ProgressImage
        healProgress;
    [SerializeField]
    GameObject
        barrier;
    [SerializeField]
    Material
        leftBarrier,
        rightBarrier;
    int
        fillID;


    new void Start()
    {
        base.Start();

        unit.healingTimer.onRatioChanged += healProgress.SetValue;
        unit.barrierRange.onRatioChanged += UpdateBarrierView;

        fillID = Shader.PropertyToID("Fill");
    }


    void UpdateBarrierView(float ratio)
    {
        ratio = Mathf.Clamp( 1 - ratio, .01f, .99f );
        leftBarrier.SetFloat(fillID, ratio);
        rightBarrier.SetFloat(fillID, ratio);
    }

    public void ShowBarrier(bool toggle)
    {
       barrier.SetActive(toggle);
    }
}
