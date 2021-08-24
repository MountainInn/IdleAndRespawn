using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleEndedProgress : ProgressImage
{
    Image left, right;

    void Awake()
    {
        Init();;

        SetType(Type.filled);
    }

    public void Init()
    {
        foreach (var item in GetComponentsInChildren<Image>())
        {
            if (item.name.ToLower().Contains("left")) left = item;
            else if (item.name.ToLower().Contains("right")) right = item;
        }

        if (left == null) Debug.LogWarning("[WARNING] Left not found!");
        if (right == null) Debug.LogWarning("[WARNING] Right not found!");
    }

    override protected void SetFillAmount(float val)
    {
        left.fillAmount = val;
        right.fillAmount = val;
    }
}
