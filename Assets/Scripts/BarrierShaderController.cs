using UnityEngine;
using System.Collections;

public class BarrierShaderController : MonoBehaviour
{
    int timeProp;
    float time;

    [SerializeField] Material leftHalf, rightHalf;

    void Awake()
    {
        timeProp = Shader.PropertyToID("Time");
    }

    public void ResetTime()
    {
        time = 0f;
    }

    public void StartBarrierUpdate()
    {
        StartCoroutine(UpdateTime());
    }

    IEnumerator UpdateTime()
    {
        do
        {
            time += Time.deltaTime;

            leftHalf.SetFloat(timeProp, time);
            rightHalf.SetFloat(timeProp, time);

            yield return new WaitForEndOfFrame();
        }
        while(true);
    }
}
