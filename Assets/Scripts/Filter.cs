using UnityEngine;
using System.Collections.Generic;
using System;

public class Filter : MonoBehaviour
{
    Func<TalentStripView, bool>
        showAll = view => true,
        hideAvailable = view => view.thisTalent.vendible.isOwned,
        hideOwned = view => !view.thisTalent.vendible.isOwned,
        currentFilter;

    void Awake()
    {
        currentFilter = showAll;

        Talent.onStripViewInitialized += FilterChild;
    }

    public void ShowAll()
    {
        currentFilter = showAll;
        FilterChildren();
    }

    public void HideAvailable()
    {
        currentFilter = hideAvailable;
        FilterChildren();
    }

    public void HideOwned()
    {
        currentFilter = hideOwned;
        FilterChildren();
    }

    void FilterChildren()
    {
        foreach (var item in transform.AllImmediateChildrenOfType<TalentStripView>())
        {
            item.gameObject.SetActive(currentFilter.Invoke(item));
        }
    }

    void FilterChild(TalentStripView view)
    {
        view.gameObject.SetActive(currentFilter.Invoke(view));

        view.thisTalent.vendible.onBought += ()=>{ FilterChild(view); };
    }
}
