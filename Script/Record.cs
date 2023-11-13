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
        // �ˬd�ɮ׸��|�O�_�w�]�m�A�p�G�|���]�m�A�h��l���ɮ׸��|
        if (string.IsNullOrEmpty(logFilePath))
        {
            InitializeLogFilePath();
        }

        // �g�J�T����txt�ɮ�
        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    private static void InitializeLogFilePath()
    {
        // �b���ε{�������[��Ƹ��|�U�إߤ@�ӦW��"Logs"����Ƨ�
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Logs");
        Directory.CreateDirectory(folderPath);

        // �إߧ��㪺�ɮ׸��|
        logFilePath = Path.Combine(folderPath, "KPI.txt");
    }
}