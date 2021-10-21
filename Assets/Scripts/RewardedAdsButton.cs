using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class RewardedAdsButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] Button _showAdButton;
    [SerializeField] Text watchAdButtonText;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;

        SwitchToGray();

        AdsInitializer.onInitComplete += ()=>
        {
            SwitchToBlue();
        };
        AdsInitializer.onInitFailed += ()=>
        {
            SwitchToGray();
        };


        // _showAdButton.onClick.AddListener(LoadAd);
        _showAdButton.onClick.AddListener(LoadAd);
    }

#region LoadAd

    public void LoadAd()
    {
        if (AdsInitializer.succesfulyInitialized)
        {
            _showAdButton.interactable = false;

            Advertisement.Load(_adUnitId, this);
        }
    }
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        ShowAd();;
    }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");

        MyOnUnityAdsFailedToLoad();
    }
    void MyOnUnityAdsFailedToLoad()
    {
        SwitchToGray();
        watchAdButtonText.text = "Ads Failed To Load"; // Здесь можно 

        // AdsREInitializer._Inst.StartReinitialization();
    }

    #endregion


    public void ShowAd()
    {
        Advertisement.Show(_adUnitId, this);
    }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) &&  showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            MyOnUnityAdsShowComplete();
        }
    }

    public static void MyOnUnityAdsShowComplete()
    {
        AdProgression._Inst.LevelUp();

        --AdCharges.CurrentCharges;
    }
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        _showAdButton.interactable = true;
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        // #if UNITY_EDITOR
        // OnUnityAdsShowComplete(_adUnitId, UnityAdsShowCompletionState.COMPLETED);
        // #endif
    }
    public void OnUnityAdsShowClick(string adUnitId) { }

    void OnDestroy()
    {
        _showAdButton.onClick.RemoveAllListeners();
    }


    private void SwitchToBlue()
    {
        AdCharges._Inst.cooldown.onRatioChanged += DisplayCooldown;
        AdCharges.onChargesChanged += InteractableIfChargesGTZ;

        DisplayCooldown();
        InteractableIfChargesGTZ();

        AdProgressionView._Inst.SwitchToBlue();
    }
    private void SwitchToGray()
    {
        AdCharges._Inst.cooldown.onRatioChanged -= DisplayCooldown;
        AdCharges.onChargesChanged -= InteractableIfChargesGTZ;

        _showAdButton.interactable = false;
        watchAdButtonText.text = "Connect to the Internet and try again";
        AdProgressionView._Inst.SwitchToGray();
    }

    void DisplayCooldown(float ratio=0)
    {
        if (AdCharges.IsGreaterThanZero)
        {
            watchAdButtonText.text = "Watch Ad";
        }
        else
            watchAdButtonText.text = AdCharges.StringTimeToCharge();
    }

    void InteractableIfChargesGTZ()
    {
        _showAdButton.interactable = AdCharges.IsGreaterThanZero;
    }
}
