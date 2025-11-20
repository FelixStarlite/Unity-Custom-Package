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

    public ViewController CurrentView
    {
        get => currentView;
    }

    [SerializeField]private GameObject startView;

    private ViewController currentView;
    private List<string> pagePath = new();
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
                views.Add(viewController.name, viewController);
            }
        }
    }

    private void Start()
    {
        foreach (KeyValuePair<string, ViewController> view in views)
        {
            // 沒有指定起始畫面，就使用第一個
            if (startView == null)
            {
                startView = view.Value.gameObject;
            }

            // 初始化並隱藏
            view.Value.Init();
        }

        currentView = startView.GetComponent<ViewController>();
        currentView.ShowView(true);
        pagePath.Add(currentView.name);
    }

    // 使用物件呼叫
    public void ToView(GameObject target)
    {
        if (views.TryGetValue(target.name, out ViewController view))
        {
            if (currentView.name == target.name || currentView.IsPlaying()) return;

            if (currentView != null)
                currentView.HideView();

            currentView = view;
            currentView.ShowView();

            pagePath.Add(target.name);
        }
    }

    // 使用View名稱呼叫
    public void ToView(string target)
    {
        if (views.TryGetValue(target, out ViewController view))
        {
            if (currentView.name == target || currentView.IsPlaying()) return;

            if (currentView != null)
                currentView.HideView();

            currentView = view;
            currentView.ShowView();

            pagePath.Add(target);
        }
    }

    // 返回上一頁
    public void Back()
    {
        if (pagePath.Count <= 1) return;

        string target = pagePath[pagePath.Count - 2];
        if (views.TryGetValue(target, out ViewController view))
        {
            if (currentView != null)
                currentView.HideView();

            currentView = view;
            currentView.ShowView();

            pagePath.RemoveAt(pagePath.Count - 1);
        }
    }
}