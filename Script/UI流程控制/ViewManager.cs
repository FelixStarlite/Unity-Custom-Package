using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<ViewManager>();
            return instance;
        }
    }

    private static ViewManager instance;

    // 回傳當前頁面，沒有的話就回傳Null
    public ViewController CurrentView
    {
        get => historyStack.Count > 0 ? historyStack.Peek() : null;
    }

    [SerializeField] private GameObject startView;

    private ViewController currentView;
    private Stack<ViewController> historyStack = new();
    private Dictionary<string, ViewController> views = new();

    private void Awake()
    {
        // 參數 true 表示包含隱藏(inactive)的物件
        ViewController[] viewControllers = GetComponentsInChildren<ViewController>(true);

        foreach (ViewController viewController in viewControllers)
        {
            // 跳過自己(如果 ViewManager 本身也有 ViewController 組件)
            if (viewController.transform == transform) continue;

            // 只取得直接子物件
            if (viewController.transform.parent == transform)
            {
                // 避免重複 Key 報錯
                if (!views.ContainsKey(viewController.name))
                {
                    views.Add(viewController.name, viewController);
                }
                else
                {
                    Debug.LogError("有頁面名稱重複: " + viewController.name);
                }
            }
        }
    }

    private void Start()
    {
        foreach (var view in views.Values)
        {
            // 沒有指定起始畫面，就使用第一個
            if (startView == null)
            {
                startView = view.gameObject;
            }

            // 初始化並隱藏
            view.Init();
        }

        // 啟動首頁
        Home();
    }

    /// <summary>
    /// 使用物件呼叫
    /// </summary>
    /// <param name="target">目標實體</param>
    [Sirenix.OdinInspector.Button]
    public void ToView(GameObject target)
    {
        if (target == null) return;
        ToView(target.name);
    }

    /// <summary>
    /// 使用View名稱呼叫
    /// </summary>
    /// <param name="targetName">目標實體名稱</param>
    public void ToView(string targetName)
    {
        if (views.TryGetValue(targetName, out ViewController targetView))
        {
            PushView(targetView);
        }
        else
        {
            Debug.LogWarning($"ViewManager: 找不到名為 {targetName} 的頁面");
        }
    }

    /// <summary>
    /// 返回上一頁
    /// </summary>
    public void Back()
    {
        PopView();
    }

    /// <summary>
    /// 返回首頁
    /// </summary>
    public void Home()
    {
        while (historyStack.Count > 0)
        {
            historyStack.Pop().gameObject.SetActive(false);
        }
        historyStack.Clear();

        if (startView != null && views.TryGetValue(startView.name, out ViewController firstView))
        {
            PushView(firstView, true); // true 表示快速啟動(無動畫)
        }
    }

    /// <summary>
    /// 切換到新頁面邏輯
    /// </summary>
    /// <param name="newView"></param>
    /// <param name="isFast"></param>
    private void PushView(ViewController newView, bool isFast = false)
    {
        // 防止重複進入同一頁，或者前一個動畫還在跑
        if (CurrentView == newView) return;
        if (CurrentView != null && CurrentView.IsPlaying()) return;

        // 隱藏當前頁面
        if (CurrentView != null)
        {
            CurrentView.HideView(isFast);
        }

        // 新頁面到堆疊
        historyStack.Push(newView);

        // 顯示新頁面
        newView.ShowView(isFast);
    }

    /// <summary>
    /// 返回上一頁邏輯
    /// </summary>
    private void PopView()
    {
        // 如果堆疊只剩 1 頁或空的，就不能再退了
        if (historyStack.Count <= 1) return;

        // 防止動畫還在跑的時候連點
        if (CurrentView != null && CurrentView.IsPlaying()) return;

        // 取出並移除當前頁面 (Pop)
        ViewController closingView = historyStack.Pop();

        // 執行退場
        closingView.HideView();

        // 取得「新的」當前頁面 (Peek 只是偷看，不移除)
        // 因為剛剛 Pop 掉最上層了，現在 Peek 看到的就是上一頁
        ViewController previousView = historyStack.Peek();

        // 顯示上一頁
        if (previousView != null)
        {
            previousView.ShowView();
        }
    }
}