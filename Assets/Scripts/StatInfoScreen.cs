using UnityEngine;
using UnityEngine.UI;

public class StatInfoScreen : MonoBehaviour
{
    static StatInfoScreen inst;
    static public StatInfoScreen _Inst => inst??=GameObject.FindObjectOfType<StatInfoScreen>();

    [SerializeField] StatInfoView prefStatInfoView;

    public void InstStatInfo(StatDescription statDescription, StatMultChain stat)
    {
        var newView = Instantiate(prefStatInfoView, transform);
        statDescription.ConnectToView(newView, stat);
    }
}
