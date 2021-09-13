using UnityEngine;
using System;
using System.IO;
using System.Text;

public class FileWriter : IDisposable
{
    DateTime startTime;
    FileStream file;
    string dirPath;

    public FileWriter(string dirName)
    {
        OpenFileStream(DateTime.Now, dirName);
    }

    ~FileWriter()
    {
        Dispose();
    }

    void OpenFileStream(DateTime time, string dirName, string fileName)
    {
        startTime = time;

        dirPath = Application.persistentDataPath + $"\\{dirName}";

        if (!System.IO.Directory.Exists(dirPath)) System.IO.Directory.CreateDirectory(dirPath);

        var path = dirPath + $"\\fileName.txt";

        file = RecreateCurrentFile(path);
    }

    void OpenFileStream(DateTime time, string dirName)
    {
        startTime = time;

        dirPath = Application.persistentDataPath + $"\\{dirName}";

        if (!System.IO.Directory.Exists(dirPath)) System.IO.Directory.CreateDirectory(dirPath);

        var path = dirPath + $"\\current_log.txt";

        file = RecreateCurrentFile(path);
    }


    public void Log(string message)
    {
        file.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
    }

    virtual public void Dispose()
    {
        string path = dirPath + $"\\{startTime:dd_hh_mm_ss}.txt";

        File.Copy(file.Name, path);

        file.Close();
        file.Dispose();
    }

    public FileStream RecreateCurrentFile(string path)
    {
        File.Delete(path);

        return File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
    }
}
