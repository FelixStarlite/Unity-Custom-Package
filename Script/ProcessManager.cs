using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

/// <summary>
/// 處理程序使用簡化腳本
/// </summary>
public class ProcessManager
{
    private Process _managedProcess;
    private readonly string _processPath;
    private readonly string _workDirectory;
    private readonly string _arguments;

    public ProcessManager(string processPath, string workDirectory = "", string arguments = "")
    {
        _processPath = processPath;
        _workDirectory = workDirectory;
        _arguments = arguments;

        Start();
    }

    /// <summary>
    /// 執行處理程序
    /// </summary>
    public void Start()
    {
        try
        {
            if (!File.Exists(_processPath))
            {
                Debug.Log($"找不到檔案 {_processPath} 。");
                return;
            }

            // 先嘗試關閉同名的處理程序
            CloseExistingProcesses();

            // 建立新的 Process 實例
            _managedProcess = new Process();
            // 設定要執行的檔案
            _managedProcess.StartInfo.FileName = _processPath;
            // 可以設定其他 StartInfo 屬性，例如工作目錄、參數等

            if (!string.IsNullOrEmpty(_workDirectory))
                _managedProcess.StartInfo.WorkingDirectory = _workDirectory;

            //參數格式 "/param1 /param2"
            if (!string.IsNullOrEmpty(_arguments))
                _managedProcess.StartInfo.Arguments = _arguments;

            _managedProcess.Start(); // 啟動處理程序
            UnityEngine.Debug.Log($"處理程序 '{_processPath}' 已啟動。ID: {_managedProcess.Id}");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"啟動處理程序 '{_processPath}' 失敗：{ex.Message}");
            _managedProcess?.Dispose();
            _managedProcess = null;
        }
    }

    /// <summary>
    /// 關閉所有同名的現有處理程序
    /// </summary>
    private void CloseExistingProcesses()
    {
        try
        {
            string processName = Path.GetFileNameWithoutExtension(_processPath);
            var existingProcesses = Process.GetProcessesByName(processName);

            foreach (var process in existingProcesses)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(3000); // 等待最多3秒讓程序關閉
                        Debug.Log($"已關閉現有處理程序：{processName} (ID: {process.Id})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"關閉處理程序時發生錯誤：{ex.Message}");
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"清理現有處理程序時發生錯誤：{ex.Message}");
        }
    }

    /// <summary>
    /// 中止處理程序
    /// </summary>
    public void Stop()
    {
        if (_managedProcess != null)
            return;
        Debug.Log(222);
        try
        {
            if (!_managedProcess.HasExited)
                return;

            _managedProcess.CloseMainWindow();

            if (!_managedProcess.WaitForExit(3000))
            {
                _managedProcess.Kill(); // 強制終止處理程序
                Debug.Log($"處理程序 '{_processPath}' (ID: {_managedProcess.Id}) 已強制終止。");
            }
            else
            {
                Debug.Log($"處理程序 '{_processPath}' (ID: {_managedProcess.Id}) 已正常關閉。");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"終止處理程序 '{_processPath}' 失敗：{ex.Message}");
        }
        finally
        {
            _managedProcess.Dispose();
            _managedProcess = null;
        }
    }


    /// <summary>
    /// 等待處理程序結束
    /// </summary>
    public void WaitForExit()
    {
        if (_managedProcess != null && !_managedProcess.HasExited)
        {
            try
            {
                _managedProcess.WaitForExit();
                Debug.Log($"處理程序 '{_processPath}' 已自行結束。");
            }
            finally
            {
                _managedProcess.Dispose();
                _managedProcess = null;
            }
        }
    }
}