using UnityEngine;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.Advertisements;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class AdCharges : MonoBehaviour
{
    static AdCharges inst;
    static public AdCharges _Inst => inst??=GameObject.FindObjectOfType<AdCharges>();

    [SerializeField]
    Animation
        flareAnimation;
    [SerializeField]
    Image
        flare1,
        flare2;

    [SerializeField]
    int
        cooldownInMinutes,
        maxCharges;
    int
        _currentCharges;

    [JsonPropertyAttribute]
    static public int CurrentCharges
    {
        get => _Inst._currentCharges;
        set
        {
            _Inst._currentCharges = Mathf.Min( value, _Inst.maxCharges );

            if (IsGreaterThanZero)
            {
                _Inst.SetFlaresActive(true);
                _Inst.flareAnimation.Play();
            }
            else
            {
                _Inst.flareAnimation.Stop();
                _Inst.SetFlaresActive(false);
            }

            onChargesChanged?.Invoke();;
        }
    }

    public void SetFlaresActive(bool active)
    {
        flare1.gameObject.SetActive(active);
        flare2.gameObject.SetActive(active);
    }
    static public Action onChargesChanged;
    static public string StringTimeToCharge()
    {
        var seconds = _Inst.cooldown.endTime - _Inst.cooldown.T;
        TimeSpan timespan = TimeSpan.FromSeconds(seconds);

        return timespan.ToStringFormatted();
    }


    [JsonPropertyAttribute, HideInInspector]
    public CallbackTimer cooldown;
    [JsonPropertyAttribute]
    DateTime exitTime;

    static public bool IsFull => CurrentCharges == _Inst.maxCharges;
    static public bool IsGreaterThanZero => CurrentCharges > 0;

    protected void Awake()
    {
        SaveSystem.onTheFirstLaunch += ()=>{ CurrentCharges = maxCharges; };
        cooldown = new CallbackTimer(cooldownInMinutes * 60);
        cooldown.onFinished += ()=>{ CurrentCharges++;  };

        flareAnimation.wrapMode = WrapMode.Loop;
    }
    void Start()
    {
        AdProgressionView._Inst.UpdateOrbFill(cooldown.GetRatio());

    }

    protected void Update()
    {
        if (CurrentCharges < maxCharges)
        {
            cooldown.Tick(Time.deltaTime);

            AdProgressionView._Inst.UpdateOrbFill(cooldown.GetRatio());
        }
    }

    public void AddOfflineTime(TimeSpan difference)
    {
        cooldown.Tick((float)difference.TotalSeconds);
    }

    protected void OnDisable()
    {
        exitTime = DateTime.UtcNow;
    }


    public string AdChargesToString()
    {
        return _currentCharges.ToString() +" / "+maxCharges.ToString();
    }
}
