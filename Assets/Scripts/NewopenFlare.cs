using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class NewopenFlare : MonoBehaviour
{
    public static NewopenFlare _Inst;
    
    List<Image> flares;

    Coroutine coroutine;

    WaitForEndOfFrame wait = new WaitForEndOfFrame();


    float t = 0;
    float updateT;

    void Awake()
    {
        _Inst = this;
        flares = new List<Image>();
    }
    


    public void OnNewTalentOpened(Image newopenFlareImage)
    {
        flares.Add(newopenFlareImage);
    }


    public void OnNewTalentHovered(Image hoveredFlareImage)
    {
        flares.Remove(hoveredFlareImage);

        hoveredFlareImage.gameObject.SetActive(false);
    }

     
    public void StopBlink()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }

    void Update()
    {
        if (flares.Count > 0)
        {
            for (int i = 0; i < flares.Count; i++)
            {
                var item = flares[i];


                item.color = item.color.SetA(( Mathf.Sin(t) +1)*.5f);
            }

            t += Time.deltaTime * 4;
            
        }
    }

    public void StartBlink()
    {
        if (coroutine != null) return;
        
        coroutine = StartCoroutine(Blink());

        t = 0;

        IEnumerator Blink()
        {
            while (true)
            {

                yield return wait;
            }
        }
    }
}
