using UnityEngine;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Advertisement : MonoBehaviour
{
    public Action onWatched, onReady;

    [JsonPropertyAttribute, HideInInspector]
    public Timer cooldown;

    [JsonPropertyAttribute]
    DateTime exitTime;

    protected void Awake()
    {
        cooldown = new Timer(10);
        cooldown.t = cooldown.endTime;

        onWatched += () =>{ cooldown.Reset(); };
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
