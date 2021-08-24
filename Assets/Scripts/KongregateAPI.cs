using UnityEngine;
using System.Runtime.InteropServices;

public class KongregateAPI : MonoBehaviour
{
    static KongregateAPI inst;
    static public KongregateAPI _Inst => inst??=GameObject.FindObjectOfType<KongregateAPI>();

    void Start()
    {
        Object.DontDestroyOnLoad(gameObject);
        gameObject.name = "KongregateAPI";

        Application.ExternalEval(
            @"if(typeof(kongregateUnitySupport) != 'undefined'){
        kongregateUnitySupport.initAPI('KongregateAPI', 'OnKongregateAPILoaded');
      };"
        );
    }

    public void OnKongregateAPILoaded(string userInfoString) {
        Debug.Log("++++OnKongregateApiLoaded");
        OnKongregateUserInfo(userInfoString);
    }  

    public void OnKongregateUserInfo(string userInfoString) {
        var info = userInfoString.Split('|');
        var userId = System.Convert.ToInt32(info[0]);
        var username = info[1];
        var gameAuthToken = info[2];
        Debug.Log("Kongregate User Info: " + username + ", userId: " + userId);
    }
}
