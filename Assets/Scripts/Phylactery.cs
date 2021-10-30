using UnityEngine;

public class Phylactery : MonoBehaviour
{
    static Phylactery inst;
    static public Phylactery _Inst => inst??=GameObject.FindObjectOfType<Phylactery>();

    public Range capacity;


    void Awake()
    {
        capacity = new Range(1e4f);
    }

    void Start()
    {
        SoftReset.onReincarnation += ()=>
        {
            capacity._Val -= SoulStats.soulDamage;

            Vault.Soulshard.Earn(SoulStats.soulDamage);
        };
    }
}
