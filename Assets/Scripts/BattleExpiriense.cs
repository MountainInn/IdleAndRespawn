using UnityEngine;

public class BattleExpiriense : MonoBehaviour
{
    static float
        flatExp = 100,
        maxStageToExpMult = 10f,
        expPerHit;

    void Awake()
    {
        UpdateExpPerHit();

        SoftReset.onMaxStageChanged += (newMaxStage)=> UpdateExpPerHit();
    }
    void UpdateExpPerHit()
    {
        float
            stageComponent = SoftReset.maxStage * maxStageToExpMult,
            fragsComponent = 1 + Hero._Inst.frags * .1f;

        expPerHit = Mathf.Floor(flatExp + stageComponent * fragsComponent);
    }

    static public void MakeExpiriense(DoDamageArgs damageArgs)
    {
        Vault.expirience.Earn(expPerHit);
    }
}
