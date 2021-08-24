using UnityEngine;
using System;
using System.IO;
using System.Text;

public class GameLogger : MonoBehaviour
{
    static GameLogger inst;
    static public GameLogger _Inst => inst??=GameObject.FindObjectOfType<GameLogger>();

    bool usingGamelogger = true;

    string logPath;

    FileStream file;


    void Awake()
    {
        if (usingGamelogger)
            OpenFileStream(DateTime.UtcNow);
    }

    void OpenFileStream(DateTime time)
    {
        var path = Application.persistentDataPath + $"\\logs\\{time:dd_hh_mm_ss}.txt";

        if (!File.Exists(path)) file = File.Create(path);

        else file = File.OpenWrite(path);
    }


    void OnDisable()
    {
        file.Close();
    }

    static public void Logg(string tag, string message)
    {
        if (_Inst == null || !_Inst.usingGamelogger) return;

        _Inst.Log(tag, message);
    }

    public void Log(string tag, string message)
    {
        DateTime time = DateTime.UtcNow;

        if (file == null) OpenFileStream(time);


        string output = $"[{time:dd hh:mm:ss}] [{tag.ToUpper()}] {message}\n";

        file.Write(Encoding.ASCII.GetBytes(output), 0, output.Length);
    }


}

