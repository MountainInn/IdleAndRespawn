using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;

public class TalentViewContainer : MonoBehaviour
{
    static TalentViewContainer inst;
    static public TalentViewContainer _Inst => inst??=GameObject.FindObjectOfType<TalentViewContainer>();

    [SerializeField] Image talentTabBeaconImage;
    [SerializeField] RectTransform talentTab;
    [SerializeField] TabView tabView;

    NoveltyBeaconColor beacon;
    void Awake()
    {
        beacon = new NoveltyBeaconColor(this,
                                        talentTabBeaconImage,
                                        talentTabBeaconImage.color,
                                        talentTabBeaconImage.color.SetA(1),
                                        ()=>{ return tabView.currentTab != talentTab; });
        beacon.checkInterval = .5f;

        Talent.onDiscoveredFirstTime += (tal) => StartBeacon();
    }

    void StartBeacon()
    {
        if (tabView.currentTab != talentTab)
        {
            beacon.StartSignal();
        }
    }

    public void StopBeacon()
    {
        beacon.StopSignal();
    }
}
