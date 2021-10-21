using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;


[JsonObjectAttribute(MemberSerialization.OptIn)]
public class Range
{
    float val, max;
    public Action onLessThanZero;
    public Action<float> onRatioChanged;
    public Action<float> onValueChanged;


    
    [OnDeserializedAttribute]
    public void OnDeserialized(StreamingContext context)
    {
        onRatioChanged?.Invoke(GetRatio());
    }

    [JsonPropertyAttribute]
    public float _Max
    {
        get => max;
        private set => max = value;
    }

    [JsonPropertyAttribute]
    public float _Val
    {
        get => val;

        set
        {
            float diff = value - val;

            val = Mathf.Clamp(value, 0, _Max);

            onValueChanged?.Invoke(diff);
            onRatioChanged?.Invoke(GetRatio());

            if (value <= 0) onLessThanZero?.Invoke();;
        }
    }


    public Range(float max)
    {
        this._Max = max;
        this._Val = max;
    }

    public Range(float val, float max)
    {
        this._Max = max;
        this._Val = val;
    }

    public void Reinitialize(float max)
    {
        this._Max = max;
        this._Val = max;
    }

    public void ResetToMax()
    {
        _Val = _Max;
    }

    public void AddMax(float addition)
    {
        _Max += addition;

        _Val += addition;
    }
    public void UpgradeMax(float newMax)
    {
        _Max = newMax;
    }
    public void UpgradeMaxWithValRestoration(float newMax)
    {
        float difference = newMax - _Max;

        _Max = newMax;

        _Val += difference;
    }

    public float GetRatio() => val / _Max;


    public void SetNewMaximumValue(float max)
    {
        float ratio = GetRatio();

        _Max = max;
        _Val = max * ratio;
    }

    static public Range operator-(Range left, Range right)
    {
        left._Val -= right._Val;

        return left;
    }
}
