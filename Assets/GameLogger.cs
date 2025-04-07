using System;
using System.IO;
using UnityEngine;
public class GameLogger : MonoBehaviour
{
    private StreamWriter logWriter;
    private string logFilePath;

    void Awake()
    {
        // Create Logs folder if it doesn't exist
        // C:/Users/Username/AppData/LocalLow
        string folderPath = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique log file name with timestamp
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(folderPath, $"log_{timestamp}.txt");

        logWriter = new StreamWriter(logFilePath);
        logWriter.AutoFlush = true;

        // Hook into Unity's log system
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logWriter.WriteLine($"{DateTime.Now:HH:mm:ss} [{type}] {logString}");
        if (type == LogType.Exception || type == LogType.Error)
        {
            logWriter.WriteLine(stackTrace);
        }
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;

        if (logWriter != null)
        {
            logWriter.Close();
        }
    }

    // persist across the scenes
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
