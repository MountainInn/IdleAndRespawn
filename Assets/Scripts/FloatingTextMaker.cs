using UnityEngine;
using System.Collections.Generic;

public class FloatingTextMaker : MonoBehaviour
{
    [SerializeField] FloatingText preFloatingText;

    [SerializeField] public Vector3 direction;
    [SerializeField] public float speed;

    float textHeight;

    Pool<FloatingText> myPool;

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
        textHeight = preFloatingText.GetComponent<RectTransform>().sizeDelta.y * ReferenceHeap.canvasScale;
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


    public void SpawnText(string text, Color color)
    {
        Vector3 offset = myPool.activeNumber * textHeight * -direction ;

        var ft = myPool.AcquireAndPlace(transform, transform.position + offset, Quaternion.identity);

        ft.SetText(text);
        ft.text.color = color;
        ft.velocity = direction * speed;
    }
}
