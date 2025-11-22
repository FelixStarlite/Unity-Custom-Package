using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[RequireComponent(typeof(DOTweenViewAnimator))] // 強制依賴 Animator
public class ViewController : MonoBehaviour
{
    [TabGroup("Tabs", "In Animation")]
    [SerializeReference]
    public ViewAnimationBase inAnimation; // 預設給一個淡入

    [TabGroup("Tabs", "Out Animation")]
    [SerializeReference]
    public ViewAnimationBase outAnimation; // 預設給一個淡出

    [TabGroup("Tabs/In Animation/SupTab", "Start Enter")]
    [GUIColor(0, 1, 0)]
    public UnityEvent OnStartEnter;

    [TabGroup("Tabs/In Animation/SupTab", "Enter Finish")]
    [GUIColor(0, 1, 0)]
    public UnityEvent OnEnterFinish;

    [TabGroup("Tabs/Out Animation/SupTab", "Start Exit")]
    [GUIColor(1, 0.6f, 0.4f)]
    public UnityEvent OnStartExit;

    [TabGroup("Tabs/Out Animation/SupTab", "Exit Finish")]
    [GUIColor(1, 0.6f, 0.4f)]
    public UnityEvent OnExitFinish;

    private IViewAnimator animator; // 持有介面，而不是具體實作

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        gameObject.SetActive(false);
        OnStartEnter.AddListener(() => gameObject.SetActive(true));
        OnExitFinish.AddListener(() => gameObject.SetActive(false));

        var animImpl = GetComponent<DOTweenViewAnimator>();
        animImpl.Init(transform.localPosition, transform.localScale);
        animator = animImpl;
    }

    /// <summary>
    /// 返回動畫播放狀態
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        if (animator == null) return false;
        return animator.IsPlaying();
    }

    /// <summary>
    /// 顯示UI
    /// </summary>
    /// <param name="isFast">是否跳過動畫</param>
    [Button]
    public void ShowView(bool isFast = false)
    {
        if (animator == null) Init();

        OnStartEnter?.Invoke();

        animator.PlayIn(inAnimation, OnEnterFinish, isFast);
    }

    /// <summary>
    /// 隱藏UI
    /// </summary>
    /// <param name="isFast">是否跳過動畫</param>
    [Button]
    public void HideView(bool isFast = false)
    {
        if (animator == null) Init();

        OnStartExit?.Invoke();

        animator.PlayOut(outAnimation, OnExitFinish, isFast);
    }
}