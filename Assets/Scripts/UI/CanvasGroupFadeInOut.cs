using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFadeInOut : MonoBehaviour
{
    [SerializeField] float transitionDuration;
    [SerializeField] bool startVisible;
    [SerializeField] bool affectBlocksRaycast;

    public bool visible {private set; get;}
    
    CanvasGroup canvasGroup;

    Coroutine coroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();


        canvasGroup.alpha = (startVisible) ? 1 : 0;

        if (affectBlocksRaycast) canvasGroup.blocksRaycasts = startVisible;

        canvasGroup.interactable = startVisible;
    }

    public void Out()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(FadingOut());

        if (affectBlocksRaycast) canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

    }
    public void OutImmediate()
    {
        if (affectBlocksRaycast) canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0f;
        visible = false;
    }
    public IEnumerator FadingOut()
    {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime / transitionDuration;

                yield return new WaitForEndOfFrame();
            }

            visible = false;
        }

    public void In()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(FadingIn());

        if (affectBlocksRaycast) canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        IEnumerator FadingIn()
        {
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime / transitionDuration;

                yield return new WaitForEndOfFrame();
            }

            visible = true;
        }
    }

    public void InImmediate()
    {
        if (affectBlocksRaycast) canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1f;
        visible = true;
    }
}
