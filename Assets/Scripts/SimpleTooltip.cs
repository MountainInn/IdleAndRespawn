using UnityEngine;
using UnityEngine.UI;

public class SimpleTooltip : Tooltip
{
    [SerializeField]
    public Text
        contentText;


    public void SetContent(string content)
    {
        contentText.text = content.ToString();
    }
}
