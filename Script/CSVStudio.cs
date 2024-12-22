using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

/// <summary>
/// 簡單的對 CSV 檔案進行讀寫的腳本。
/// </summary>
public static class CSVStudio
{
    /// <summary>
    /// 讀取 CSV 檔案
    /// </summary>
    /// <typeparam name="T">泛型;單筆資料的類別</typeparam>
    /// <param name="filePath">讀取檔案的路徑</param>
    /// <returns>將檔案裡的資料回傳到資料陣列</returns>
    public static List<T> ReadCSV<T>(string filePath) where T : new()
    {
        List<T> chocolateInfo = new List<T>();
        using (var reader = new StreamReader(filePath))
        {
            // 標題列不要，所以先讀取一次
            string headerLine = reader.ReadLine();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');
                DateTime.TryParseExact(values[3], "G", null, DateTimeStyles.None, out DateTime parsedDateTime);
                var data = new T();
                var properties = typeof(T).GetProperties();
                {
                    properties[0].SetValue(data, values[0]);
                    properties[1].SetValue(data, int.Parse(values[1]));
                    properties[2].SetValue(data, bool.Parse(values[2]));
                    properties[3].SetValue(data, parsedDateTime);
                };
                chocolateInfo.Add(data);
            }
        }
        return chocolateInfo;
    }

    /// <summary>
    /// 寫入 CSV 檔案
    /// </summary>
    /// <typeparam name="T">泛型;單筆資料的類別</typeparam>
    /// <param name="filePath">要寫入的路徑和檔名</param>
    /// <param name="table">資料陣列</param>
    public static void WriteCSV<T>(string filePath, List<T> table)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            var properties = typeof(T).GetProperties();
            writer.WriteLine(string.Join(",", properties.Select(p => p.Name)));

            foreach (var data in table)
            {
                var values = properties.Select(p => p.GetValue(data)?.ToString()).ToArray();
                writer.WriteLine(string.Join(",", values));
            }
        }
    }
}