using System.Diagnostics;
using UnityEngine.EventSystems;

public class TouchKeyboardLauncher
{
    public static void ShowKeyboard()
    {
        string keyboardPath = @"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe";
        try
        {
            // 不再檢查進程是否存在，直接發送啟動請求。
            Process.Start(keyboardPath);
        }
        catch (System.Exception e)
        {
            // 處理路徑找不到或權限不足等罕見錯誤
            UnityEngine.Debug.LogError("無法啟動觸控鍵盤: " + e.Message);
        }
    }

    public static void HideKeyboard()
    {
        // 我們需要嘗試關閉這兩個進程，因為不同 Windows 版本
        // 會由不同的進程託管鍵盤 UI。
        // CloseMainWindow() 是 "禮貌" 的請求，不需要管理員權限。

        // 1. 嘗試關閉 'TextInputHost' (現代 Windows 10/11 的主要目標)
        TryCloseProcess("TextInputHost");

        // 2. 嘗試關閉 'TabTip' (舊版 Windows 或某些情境下的目標)
        TryCloseProcess("TabTip");
    }

    /// <summary>
    /// 輔助方法：嘗試尋找指定名稱的進程並禮貌地關閉
    /// </summary>
    /// <param name="processName">要關閉的進程名稱 (不含 .exe)</param>
    private static void TryCloseProcess(string processName)
    {
        try
        {
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                // 使用 CloseMainWindow() 發送關閉請求
                if (process.MainWindowHandle != System.IntPtr.Zero)
                {
                    process.CloseMainWindow();
                }
            }
        }
        catch (System.Exception e)
        {
            // 記錄錯誤，但不要讓程式崩潰
            UnityEngine.Debug.LogWarning($"嘗試關閉 {processName} 時發生錯誤: {e.Message}");
        }
    }
}