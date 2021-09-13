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
        expPerHit = Mathf.Floor(flatExp + SoftReset.maxStage * maxStageToExpMult);
    }

    static public void MakeExpiriense(DoDamageArgs damageArgs)
    {
        Vault.expirience.Earn(expPerHit);
    }
}
