using UnityEngine;
public class Accelerator : MonoBehaviour
{
    public float timeScale = 10f;

    void Awake() { Time.timeScale = timeScale; }
}
