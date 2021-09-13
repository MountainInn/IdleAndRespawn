using UnityEngine;

public class SoulshardIncome : MonoBehaviour
{
    Timer second = new Timer(1);

    void Update()
    {
       if (second.Tick())
       {
           Vault.soulEnergy.Earn(Vault.soulshard._Val);
       }
    }
}
