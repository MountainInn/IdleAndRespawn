using UnityEngine;
using Newtonsoft.Json;

[JsonObjectAttribute(MemberSerialization.OptIn)]
public class PlayerStats : MonoBehaviour
{
    static PlayerStats inst;
    static public PlayerStats _Inst => inst??=GameObject.FindObjectOfType<PlayerStats>();
}
