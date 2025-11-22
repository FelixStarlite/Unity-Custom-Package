using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class DOTweenViewAnimator : MonoBehaviour, IViewAnimator
{
    private CanvasGroup m_CanvasGroup;
    private RectTransform m_RectTransform;
    private Sequence sequence;

    // 緩存初始狀態，這部分也可以從 Controller 傳過來，視需求而定
    private Vector3 originPosition;
    private Vector3 originScale;
    private bool isInitialized = false;

    private void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_RectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="pos">原來的位置</param>
    /// <param name="scale">原來的大小</param>
    public void Init(Vector3 pos, Vector3 scale)
    {
        originPosition = pos;
        originScale = scale;
        isInitialized = true;
    }

    /// <summary>
    /// 返回動畫播放狀態
    /// </summary>
    /// <returns>是否正在播放</returns>
    public bool IsPlaying()
    {
        return sequence != null && sequence.IsPlaying();
    }

    /// <summary>
    /// 中斷動畫
    /// </summary>
    public void Stop()
    {
        if (sequence != null) sequence.Kill();
    }

    /// <summary>
    /// 進場動畫
    /// </summary>
    /// <param name="anim">動畫實作</param>
    /// <param name="onComplete">完成回調事件</param>
    public void PlayIn(ViewAnimationBase anim, UnityEvent onComplete, bool isFast)
    {
        Stop(); // 先停止舊的動畫

        // 先將 UI 還原到「原始乾淨狀態」
        // 避免上一次是 Slide(改了位置)，這一次是 Fade(只改透明度)，導致位置回不來
        RestoreToOriginalState();

        // 如果沒有設定動畫資料，直接結束
        if (anim == null)
        {
            m_CanvasGroup.alpha = 1;
            onComplete?.Invoke();
            return;
        }

        // 建立 Sequence 並委派邏輯
        sequence = DOTween.Sequence();
        anim.AppendToSequence(sequence, m_CanvasGroup, m_RectTransform, true);

        // 設定完成回調
        sequence.OnComplete(() => onComplete?.Invoke());

        // 處理快速模式 (直接跳到動畫結尾)
        if (isFast)
        {
            sequence.Complete(true);
        }
    }

    /// <summary>
    /// 離場動畫
    /// </summary>
    /// <param name="anim">動畫實作</param>
    /// <param name="onComplete">完成回調事件</param>
    public void PlayOut(ViewAnimationBase anim, UnityEvent onComplete, bool isFast)
    {
        Stop();

        if (anim == null)
        {
            m_CanvasGroup.alpha = 0;
            onComplete?.Invoke();
            return;
        }

        // 退場通常直接從「當前狀態」開始播，不需要 ResetToStart

        sequence = DOTween.Sequence();
        // false 代表退場
        anim.AppendToSequence(sequence, m_CanvasGroup, m_RectTransform, false);

        sequence.OnComplete(() => onComplete?.Invoke());

        if (isFast)
        {
            sequence.Complete(true);
        }
    }

    /// <summary>
    /// 輔助方法：將 UI 強制還原到 Init 時的原始狀態
    /// </summary>
    private void RestoreToOriginalState()
    {
        if (!isInitialized) return;

        // 還原位置與大小
        transform.localPosition = originPosition;
        transform.localScale = originScale;

        // 預設 Alpha 為 1 (顯示狀態)
        // 進場動畫會在隨後的 ResetToStart 把它設為 0 (如果需要的話)
        m_CanvasGroup.alpha = 1;
    }
}