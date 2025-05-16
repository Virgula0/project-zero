using System;
using System.IO;
using UnityEngine;

public class GameLogger : MonoBehaviour
{
    private StreamWriter _logWriter;
    private string _logFilePath;
    public static GameLogger Instance { get; private set; }

    private void Awake()
    {
        // If another instance already exists, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // This is the one true logger
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Prepare log file
        string folderPath = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _logFilePath = Path.Combine(folderPath, $"log_{timestamp}.txt");

        _logWriter = new StreamWriter(_logFilePath) { AutoFlush = true };

        // Hook into Unity’s log system
        Application.logMessageReceived += HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        _logWriter.WriteLine($"{time} [{type}] {logString}");
        if (type == LogType.Exception || type == LogType.Error)
            _logWriter.WriteLine(stackTrace);
    }

    private void OnDestroy()
    {
        // Only the singleton instance should tear down the subscription
        if (Instance == this)
            Application.logMessageReceived -= HandleLog;

        _logWriter?.Close();
    }

    // Optional public method for manual logging
    public void Log(string message)
    {
        Debug.Log(message);
    }
}
