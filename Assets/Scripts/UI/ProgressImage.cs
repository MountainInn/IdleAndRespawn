using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressImage : MonoBehaviour
{
    [SerializeField] Type progType;
    [SerializeField] public Image image;

    protected Action<float> setValue;

    void Awake()
    {
        SetType(progType);

        SetValue(1);
    }

    protected void SetType(ProgressImage.Type progType)
    {
        switch (progType) {

            case ProgressImage.Type.filled: setValue = SetFillAmount; break;

            case ProgressImage.Type.scaleX: setValue = SetScaleX; break;

            case ProgressImage.Type.scaleAll: setValue = SetScaleAll; break;

            default: Debug.LogError($"[ERROR {GetType()}] Function for type {progType} not specified"); break;
        }
    }


    public void SetValue(float val)
    {
        setValue(val);
    }


    virtual protected void SetFillAmount(float val)
    {
        image.fillAmount = val;
    }
    void SetScaleX(float val)
    {
        image.transform.localScale = image.transform.localScale.SetX(val);
    }
    void SetScaleAll(float val)
    {
        image.transform.localScale = new Vector3( val, val, val );
    }


    public enum Type
        {
            filled, scaleX, scaleY, scaleAll
        }
}
