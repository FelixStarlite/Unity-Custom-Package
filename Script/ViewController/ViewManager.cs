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

    [SerializeField]
    private GameObject startView;

    private ViewController currentView;
    private List<string> pagePath = new List<string>();
    private Dictionary<string, ViewController> views = new Dictionary<string, ViewController>();

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            ViewController viewController = transform.GetChild(i).GetComponent<ViewController>();
            if (viewController == null) continue;

            views.Add(viewController.name, viewController);
            viewController.Init();
        }
    }

    private void Start()
    {
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