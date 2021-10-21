using UnityEngine;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class BattleExpiriense : MonoBehaviour
{
    static BattleExpiriense inst;
    static public BattleExpiriense _Inst => inst??=GameObject.FindObjectOfType<BattleExpiriense>();

    static float
        flatExp = 100,
        maxStageToExpMult = 6.5f,
        expPerHit;

    [JsonPropertyAttribute]
    public float
        maxExpPerHit;

    Timer
        eachSecond = new Timer(1);

    void Awake()
    {
        UpdateExpPerHit();


        SaveSystem.onAfterLoad += OnAfterLoad;
        void OnAfterLoad()
        {
            SoftReset.onMaxStageChanged += (newMaxStage)=> UpdateExpPerHit();
            SaveSystem.onAfterLoad -= OnAfterLoad;
        }
    }

    void UpdateExpPerHit()
    {
        float
            stageComponent = SoftReset.maxStage * maxStageToExpMult;

        expPerHit = Mathf.Floor(flatExp + stageComponent);

        maxExpPerHit = Mathf.Max(expPerHit, maxExpPerHit);
    }


    void Update()
    {
        if (eachSecond.Tick())
        {
            MakeExpiriense();
        }
    }

    private void MakeExpiriense()
    {
        Vault.Expirience.Earn(maxExpPerHit);
    }
}
