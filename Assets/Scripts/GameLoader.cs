using UnityEngine;

public class GameLoader : MonoBehaviour
{
    SaveSystem saveSystem;

    void Awake()
    {
        saveSystem = GameObject.FindObjectOfType<SaveSystem>();
    }

    void Start()
    {
        saveSystem.LoadGame();
    }
}
