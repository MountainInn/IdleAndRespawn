using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabView : MonoBehaviour
{
    [SerializeField] float translationDuration = .25f;
    [SerializeField,SpaceAttribute] RectTransform viewport;
    [SerializeField] Button prefTabButton;
    [SerializeField] Transform tabButtons;

    List<RectTransform> tabs;

    float width;

    int currentTabId = 0;

    public RectTransform currentTab {get; private set;}

    [SerializeField] Button initialTabButton;

    void Awake()
    {
        width = viewport.rect.width;

        tabs = new List<RectTransform>();


        for (int i = 0; i < viewport.childCount; i++)
        {
            var child = viewport.GetChild(i).GetComponent<RectTransform>();
            tabs.Add(child);

            /// Fit children to viewport
            // child.sizeDelta = new Vector2(viewport.rect.width, viewport.rect.height);

            child.localPosition = child.localPosition.SetY(10000);
        }

        foreach (var item in GetComponentsInChildren<MaskableGraphic>())
        {
            item.maskable = true;
        }

        currentTab = tabs[0];
        currentTab.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;;
    }

    void Start()
    {
        initialTabButton.onClick.Invoke();;
    }

    public void SwitchTab(RectTransform tab)
    {
        if (currentTab == tab) return;

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = StartCoroutine(TweenTabPositions(tab));
    }

    Coroutine coroutine;

    IEnumerator TweenTabPositions(RectTransform tab)
    {
        float step = width;

        RectTransform nextTab = tab;

        int nextTabId = tabs.FindIndex(rt => rt == tab);

        if (currentTabId < nextTabId) step *= -1;

        Vector3 nextTabStartPosition = nextTab.anchoredPosition = new Vector3(-step, 0, 0);

        float t = 0;

        currentTabId = nextTabId;

        RectTransform previousTab = currentTab.GetComponent<RectTransform>();
        currentTab = nextTab;

        while (t < 1f)
        {
            t += Time.deltaTime / translationDuration;

            previousTab.anchoredPosition = Vector3.Lerp(Vector3.zero, -nextTabStartPosition, t);
            currentTab.anchoredPosition = Vector3.Lerp(nextTabStartPosition, Vector3.zero, t);

            yield return new WaitForEndOfFrame();
        }

    }

}
