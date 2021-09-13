using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MultipleTargetGraphicButton : Button
{
    [SerializeField] public List<Image> targetGraphics;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color targetColor =
            state == SelectionState.Disabled ? colors.disabledColor :
            state == SelectionState.Highlighted ? colors.highlightedColor :
            state == SelectionState.Normal ? colors.normalColor :
            state == SelectionState.Pressed ? colors.pressedColor :
            state == SelectionState.Selected ? colors.selectedColor : Color.white;

        foreach (var graphic in targetGraphics)
            graphic.CrossFadeColor(targetColor, instant ? 0 : colors.fadeDuration, true, true);
    }
}
