using UnityEngine;

public class GameSaver : MonoBehaviour
{
    SaveSystem saveSystem;

    void Awake()
    {
        saveSystem = GameObject.FindObjectOfType<SaveSystem>();
    }

    Timer autoSave = new Timer(30);

    void FixedUpdate()
    {
        if (autoSave.Tick())
        {
            saveSystem.SaveGame();
        }
    }

    void OnDisable()
    {
        saveSystem.SaveGame();
    }
}
