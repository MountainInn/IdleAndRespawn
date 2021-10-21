using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsREInitializer : MonoBehaviour
{
    static AdsREInitializer inst;
    static public AdsREInitializer _Inst => inst??=GameObject.FindObjectOfType<AdsREInitializer>();

    Coroutine coroutine;

    static public bool isConnected = false;
    static public bool isAdReady => isConnected && Advertisement.isInitialized;

    void Awake()
    {
        AdsInitializer.onInitFailed += ()=>
        {
            StartReinitialization();
        };

        StartReinitialization();
    }

    public void StartReinitialization()
    {
        if (coroutine == null)
            coroutine = StartCoroutine(
                CheckInternetConnection((isConnected) =>{

                    AdsREInitializer.isConnected = isConnected;

                    coroutine = null;

                    if (isConnected)
                    {
                        if (Advertisement.isInitialized)
                            AdsInitializer.onInitComplete?.Invoke();
                        else
                            AdsInitializer._Inst.InitializeAds();
                    }
                    else
                    {
                        this.StartInvokeAfter(StartReinitialization, 5);
                    }
                }));
    }

    public void FullCheck(Action onSuccess, Action onFail)
    {
        if (coroutine == null)
            coroutine = StartCoroutine(
                CheckInternetConnection((isConnected) =>{

                    AdsREInitializer.isConnected = isConnected;

                    coroutine = null;

                    if (isConnected && Advertisement.isInitialized)
                    {
                        onSuccess.Invoke();
                    }
                    else
                    {
                        onFail.Invoke();
                    }
                }));
    }


    IEnumerator CheckInternetConnection(Action<bool> action)
    {
        WWW www = new WWW("http://google.com");
        yield return www;
        if (www.error != null) {
            action (false);
        } else {
            action (true);
        }
    }
}
