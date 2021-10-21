using UnityEngine;
using System.Collections.Generic;

public class FloatingTextMaker : MonoBehaviour
{
    [SerializeField] FloatingText preFloatingText;

    [SerializeField] public Vector3 direction;
    [SerializeField] public float speed;
    [SerializeField] public int normalFontsize;

    float textHeight;

    Pool<FloatingText> myPool;
    int
        fontsize,
        bigFontsize;

    void Awake()
    {
        myPool = new Pool<FloatingText>(transform, preFloatingText);
        myPool.onInstantiated += ft =>
        {
            ft.onLifetimeEnd += ()=>
            {
                myPool.Release(ft);
            };

            ft.transform.SetParent(transform);
            ft.transform.localScale = Vector3.one;

        };

        fontsize = normalFontsize;
        bigFontsize = (int)(normalFontsize * 1.5f);
    }

    void Start()
    {
    }

    public void SpawnTextHeal(string text)
    {
        SpawnText(text, Color.green);
    }

    public void SpawnTextDamage(string text)
    {
        SpawnText(text, Color.red);
    }

    public void SetBigFontsize()
    {
        fontsize = bigFontsize;
    }
    public void SetNormalFontsize()
    {
        fontsize = normalFontsize;
    }

    public void SpawnText(string text, Color color)
    {
        var ft = myPool.AcquireAndParent(transform);

        textHeight = ft.text.fontSize * transform.localScale.y;

        Vector3 offset = myPool.activeNumber * textHeight * -direction;


        ft.SetText(text);
        ft.transform.localPosition = offset;
        ft.text.color = color;
        ft.text.fontSize = fontsize;
        ft.velocity = direction * speed;
    }
}
