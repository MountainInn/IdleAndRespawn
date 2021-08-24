using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(EventTrigger))]
public class TooltipTarget : MonoBehaviour
{
    public Tooltip tooltip;

    public Action onPointerEnter, onPointerExit;

    EventTrigger eventTrigger;


    protected void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();

        InitializeEventTrigger(eventTrigger);
    }

    public void InitializeEventTrigger(EventTrigger eventTrigger)
    {
        eventTrigger.AddTrigger(EventTriggerType.PointerEnter,
                                (args)=>
                                {
                                    onPointerEnter?.Invoke();

                                        
                                    var pos = new Vector2(transform.position.x, transform.position.y) ;


                                    tooltip.PositionTooltip(pos);
                                    tooltip.fadeInOut.In();
                                });

        eventTrigger.AddTrigger(EventTriggerType.PointerExit,
                                (args) =>
                                {
                                    onPointerExit?.Invoke();

                                    tooltip.fadeInOut.Out();
                                });
    }

}
