using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Объединяет все дочерние кнопки в радио группу
public class RadioButtonGroup : MonoBehaviour
{
    List<Button> buttons;

    void Awake()
    {
        buttons = new List<Button>(transform.AllImmediateChildrenOfType<Button>());

        foreach (var item in buttons)
        {
            item.onClick.AddListener(()=>{ RadioToggle(item); });
        }
    }

    void RadioToggle(Button clickedButton)
    {
        foreach (var item in buttons)
        {
            item.interactable = (item != clickedButton);
        }
    }
}
