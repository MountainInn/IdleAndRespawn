using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceHeap : MonoBehaviour
{
    static ReferenceHeap inst;
    static public ReferenceHeap _Inst => inst ??= GameObject.FindObjectOfType<ReferenceHeap>();
    

    Canvas _canvas;
    static public float canvasScale
    {
        get
        {
            if (_Inst._canvas == null) _Inst._canvas = GameObject.FindObjectOfType<Canvas>();
            return _Inst._canvas.transform.localScale.x;
        }
    }
    static public Canvas canvas => _Inst._canvas;
    public Transform bossHpBar;

    public ProgressImage curseOfDoomProgress;

    void Awake()
    {
    }


}
