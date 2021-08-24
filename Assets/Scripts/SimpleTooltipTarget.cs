using UnityEngine;
using System;

public class SimpleTooltipTarget : TooltipTarget
{
    [SerializeField,
    MultilineAttribute(3)]
        protected string content;
    
    new protected void Awake()
    {
        base.Awake();;
        
        tooltip = GameObject.FindObjectOfType<SimpleTooltip>();

        var simpleTooltip = tooltip as SimpleTooltip;

        onPointerEnter = () => simpleTooltip.SetContent(content);
    }
}
