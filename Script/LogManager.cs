using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 日誌記錄工具類，用於管理和記錄應用程序的各種日誌信息
/// </summary>
public static class LogManager
{
    private static string logFilePath;
    private static readonly object _sync = new();
    private static bool _initialized;

    static LogManager()
    {
        try
        {
            Initialize();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Record initialization failed: {e}");
        }
    }



    /// <summary>
    /// 記錄一般信息日誌
    /// </summary>
    /// <param name="message">要記錄的信息內容</param>
    public static void Log(string message)
    {
        Initialize();
        WriteLine($"[Info] {message}");
    }

    public static void LogInfo(string message) => Log(message);

    /// <summary>
    /// 記錄警告級別的日誌信息
    /// </summary>
    /// <param name="message">警告信息內容</param>
    public static void LogWarning(string message)
    {
        Initialize();
        WriteLine($"[Warning] {message}");
    }

    /// <summary>
    /// 記錄錯誤級別的日誌信息
    /// </summary>
    /// <param name="message">錯誤信息內容</param>
    public static void LogError(string message)
    {
        Initialize();
        WriteLine($"[Error] {message}");
    }

    /// <summary>
    /// 記錄異常信息，包括異常詳細信息和堆棧跟踪
    /// </summary>
    /// <param name="ex">異常對象</param>
    /// <param name="extraMessage">額外的描述信息</param>
    public static void LogException(Exception ex, string extraMessage = null)
    {
        Initialize();
        var sb = new StringBuilder();
        sb.Append("[Exception] ");
        if (!string.IsNullOrEmpty(extraMessage))
        {
            sb.Append(extraMessage).Append(" - ");
        }
        if (ex != null)
        {
            sb.Append(ex.GetType().Name).Append(": ").Append(ex.Message).AppendLine();
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                sb.AppendLine(ex.StackTrace);
            }
            if (ex.InnerException != null)
            {
                sb.AppendLine("---> Inner Exception:");
                sb.AppendLine(ex.InnerException.ToString());
            }
        }
        else
        {
            sb.AppendLine("<null exception>");
        }
        WriteLine(sb.ToString());
    }

    /// <summary>
    /// 記錄帶有Unity對象上下文的日誌信息
    /// </summary>
    /// <param name="message">日誌信息</param>
    /// <param name="context">Unity對象上下文</param>
    public static void LogWithContext(string message, UnityEngine.Object context)
    {
        Initialize();
        var ctxInfo = context == null ? "<no context>" : $"name='{context.name}', type='{context.GetType().Name}'";
        WriteLine($"[Info][Context] {message} | {ctxInfo}");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AutoInitialize()
    {
        // 這個方法的內容甚至可以是空的！
        // 因為只要這個方法被 Unity 自動呼叫，
        // 就構成了對 LogManager 類別的「第一次存取」，
        // 從而自動觸發上面的靜態建構函式。
        Debug.Log("AutoInitialize called by Unity, triggering static constructor.");
    }

    /// <summary>
    /// 初始化日誌系統，設置日誌文件路徑並訂閱Unity日誌事件
    /// </summary>
    private static void Initialize()
    {
        if (_initialized) return;
        lock (_sync)
        {
            if (_initialized) return;
            InitializeLogFilePath();
            WriteHeader();
            SubscribeToUnityLogs();
            _initialized = true;
        }
    }

    /// <summary>
    /// 初始化日誌文件的保存路徑
    /// </summary>
    private static void InitializeLogFilePath()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Logs");
        Directory.CreateDirectory(folderPath);
        logFilePath = Path.Combine(folderPath, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_Log.txt");
    }

    /// <summary>
    /// 寫入日誌文件的頭部信息，包括會話開始時間和環境信息
    /// </summary>
    private static void WriteHeader()
    {
        var header = new StringBuilder();
        header.AppendLine(new string('=', 80));
        header.AppendLine($"Log Session Start: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        header.AppendLine($"Unity: {Application.unityVersion} | Platform: {Application.platform}");
        header.AppendLine($"Product: {Application.productName} v{Application.version}");
#if UNITY_EDITOR
        header.AppendLine("Environment: Editor");
#else
        header.AppendLine("Environment: Player");
#endif
        header.AppendLine(new string('=', 80));
        WriteLine(header.ToString(), includeTimestamp: false);
    }

    /// <summary>
    /// 訂閱Unity的日誌事件，以捕獲Unity產生的日誌信息
    /// </summary>
    private static void SubscribeToUnityLogs()
    {
        Application.logMessageReceived -= HandleUnityLog;
        Application.logMessageReceivedThreaded -= HandleUnityLog;
        Application.logMessageReceived += HandleUnityLog;
        Application.logMessageReceivedThreaded += HandleUnityLog;
    }

    /// <summary>
    /// 處理Unity的日誌消息
    /// </summary>
    /// <param name="condition">日誌條件</param>
    /// <param name="stackTrace">堆棧跟踪</param>
    /// <param name="type">日誌類型</param>
    private static void HandleUnityLog(string condition, string stackTrace, LogType type)
    {
        Initialize();
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                WriteLine($"[{type}] {condition}\n{stackTrace}");
                break;
            case LogType.Warning:
                WriteLine($"[Warning] {condition}");
                break;
            default:
                WriteLine($"[Log] {condition}");
                break;
        }
    }

    /// <summary>
    /// 將文本寫入日誌文件
    /// </summary>
    /// <param name="text">要寫入的文本</param>
    /// <param name="includeTimestamp">是否包含時間戳</param>
    private static void WriteLine(string text, bool includeTimestamp = true)
    {
        try
        {
            lock (_sync)
            {
                if (string.IsNullOrEmpty(logFilePath))
                {
                    InitializeLogFilePath();
                }
                var prefix = includeTimestamp ? $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " : string.Empty;
                File.AppendAllText(logFilePath, prefix + text + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception e)
        {
            // Prevent logging failures from crashing app; surface a minimal warning in console
            Debug.LogWarning($"Record failed to write log: {e.Message}");
        }
    }
}