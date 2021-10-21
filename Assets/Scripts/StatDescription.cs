using UnityEngine;
using UnityEngine.UI;

public class StatDescription : MonoBehaviour
{
    [SerializeField] protected string _statName, _description;
    [SerializeField] public Color statColor;


    public virtual string statName => _statName;
    public virtual string description => _description;


    public virtual void ConnectToView(StatInfoView view, StatMultChain stat)
    {
        view.statName.color = statColor;
        view.statName.text = statName;
        view.description.text = description;
    }
}
