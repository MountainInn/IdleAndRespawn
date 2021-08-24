using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceHeap : MonoBehaviour
{
    static ReferenceHeap inst;
    static public ReferenceHeap _Inst => inst ??= GameObject.FindObjectOfType<ReferenceHeap>();
    

    Canvas canvas;
    static public float canvasScale
    {
        get
        {
            if (_Inst.canvas == null) _Inst.canvas = GameObject.FindObjectOfType<Canvas>();
            return _Inst.canvas.transform.localScale.x;
        }
    }
    public Transform bossHpBar;

    public ProgressImage curseOfDoomProgress;

    void Awake()
    {
    }


}
