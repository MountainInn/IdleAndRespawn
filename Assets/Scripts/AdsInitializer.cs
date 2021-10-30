using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Networking;

public partial class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    static AdsInitializer inst;
    static public AdsInitializer _Inst => inst??=GameObject.FindObjectOfType<AdsInitializer>();

    [SerializeField] string _androidGameId = "4382947";
    [SerializeField] bool _enablePerPlacementMode = true;
    bool _testMode = false;
    private string _gameId;

    static public Action
        onInitComplete,
        onInitFailed,
        onReinitialize;

    static public bool succesfulyInitialized;

    void Awake()
    {
        _gameId = _androidGameId;
    }

    public void InitializeAds()
    {
        Advertisement.Initialize(_gameId, _testMode, _enablePerPlacementMode, this);

        StartWaiting();
    }
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");

        StopWaiting();
    }


    void StopWaiting()
    {
        if (waiter != null)
        {
            StopCoroutine(waiter);
            waiter = null;

            onInitFailed?.Invoke();
        }
    }
    void StartWaiting()
    {
        waiter = StartCoroutine(WaitForInitialized());
    }

    Coroutine waiter;

    IEnumerator WaitForInitialized()
    {
        float waiting = 0;
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSecondsRealtime(.5f);

            waiting += .5f;
            if (waiting > 5)
            {
                StopWaiting();
            }
        }

        onInitComplete?.Invoke();
    }

}
