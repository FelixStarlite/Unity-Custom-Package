using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 將Unity中的場景儲存成360圖片
/// - 該腳本依賴 Unity360ScreenshotCapture 插件，可以在Unit Store中下載，或者到
/// https://github.com/yasirkula/Unity360ScreenshotCapture 下載
/// - 該腳本依賴 Odin 插件，可至 https://odininspector.com/ 下載
/// </summary>
public class CaptureController : MonoBehaviour
{
    public string fileName; // 設定檔案名稱(可選)
    [FolderPath(AbsolutePath = true)]
    public string path; // 設定儲存路徑
    public int resolution; // 設定解析度(2:1)(可選)

    [Button("Play")]
    private void Start()
    {
#if UNITY_EDITOR
        // 檔案名稱或解析度欄位是空的話，就會使用預設值
        fileName = string.IsNullOrEmpty(fileName) ? "360_Screenshot" : fileName;
        resolution = resolution == 0 ? 1024 : resolution;

        // 儲存路徑是空的就出現檔案總管視窗
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.SaveFolderPanel(
                "Save 360 Screenshot",
                "",
                ""
            );
        }

        // 如果儲存路徑中的檔案名稱和fileName不一樣的話，將檔案名稱修改成一致

        if (!string.IsNullOrEmpty(path))
        {
            string fullPath = $"{path}\\{fileName}.jpg";
            byte[] bytes = I360Render.Capture(resolution);
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"檔案儲存成功 {fullPath}");
        }
#endif
    }


    private void SetPath()
    {

    }
}