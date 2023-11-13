using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Record
{
    private static string logFilePath;

    public static void Log(string message)
    {
        // 檢查檔案路徑是否已設置，如果尚未設置，則初始化檔案路徑
        if (string.IsNullOrEmpty(logFilePath))
        {
            InitializeLogFilePath();
        }

        // 寫入訊息到txt檔案
        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    private static void InitializeLogFilePath()
    {
        // 在應用程式的持久資料路徑下建立一個名為"Logs"的資料夾
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Logs");
        Directory.CreateDirectory(folderPath);

        // 建立完整的檔案路徑
        logFilePath = Path.Combine(folderPath, "KPI.txt");
    }
}