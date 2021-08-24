using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleVisibilityButton : MonoBehaviour
{
    [SerializeField] CanvasGroupFadeInOut target;

    Button self;

    void Awake()
    {
        self = GetComponent<Button>();

        self.onClick.AddListener(()=>{
            if (target.visible) target.Out();
            else target.In();
        });
    }
}

