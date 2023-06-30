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
                instance = FindObjectOfType<ViewManager>();
            return instance;
        }
    }

    private static ViewManager instance;

    [SerializeField]
    private GameObject startView;

    private ViewController currentView;
    private Dictionary<string, ViewController> views = new Dictionary<string, ViewController>();

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            ViewController viewController = transform.GetChild(i).GetComponent<ViewController>();
            if (viewController == null) continue;

            views.Add(viewController.name, viewController);
            viewController.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        currentView = startView.GetComponent<ViewController>();
        currentView.ShowView(true);
    }

    // 使用物件呼叫
    public void ToView(GameObject target)
    {
        if (views.TryGetValue(target.name, out ViewController view))
        {
            if (currentView.name == target.name || currentView.IsPlaying()) return;

            if (currentView != null)
                currentView.HideView();

            view.ShowView();
            currentView = view;
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

            view.ShowView();
            currentView = view;
        }
    }
}