using UnityEngine;

public class BattleExpiriense : MonoBehaviour
{
    static float
        flatExp = 100,
        damageToExpMult = .5f;

    static public void MakeExpiriense(DoDamageArgs damageArgs)
    {
        Vault.expirience.Earn( Mathf.Floor( flatExp +
                                 damageArgs.damage._Val * damageToExpMult ) );
    }
}
