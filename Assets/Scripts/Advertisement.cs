using UnityEngine;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine.UI;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Advertisement : MonoBehaviour
{
    static Advertisement inst;
    static public Advertisement _Inst => inst??=GameObject.FindObjectOfType<Advertisement>();

    static public Action onWatched, onReady;

    [JsonPropertyAttribute, HideInInspector]
    public Timer cooldown;

    [JsonPropertyAttribute]
    DateTime exitTime;

    protected void Awake()
    {
        cooldown = new Timer(10);
        cooldown.T = cooldown.endTime;

        onWatched += () =>{ cooldown.Reset(); };
	}

    void Start()
    {
        /// Засчитать просмотр рекламы без просмотра рекламы
        GetComponentInChildren<Button>().onClick.AddListener(AdWatched);
    }

    public void AdWatched()
    {
        Debug.Log("AdWATCHED");
        onWatched?.Invoke();
    }

    protected void Update()
    {
        if( cooldown.Countup() )
        {
            onReady?.Invoke();
        }
    }

    [OnDeserializedAttribute]
    protected void OnDeserialized(StreamingContext streamingContext)
    {
        DateTime enterTime = DateTime.UtcNow;

        var diff = (enterTime - exitTime).Seconds;

        cooldown.AddSeconds(diff);

        if (cooldown.isFinished) onReady?.Invoke();
    }

    protected void OnDisable()
    {
        exitTime = DateTime.UtcNow;
    }
}
