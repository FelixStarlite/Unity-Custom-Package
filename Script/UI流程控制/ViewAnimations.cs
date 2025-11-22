using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// 抽象基底類別：定義所有動畫共有的屬性和行為
/// </summary>
[Serializable]
public abstract class ViewAnimationBase
{
    [HorizontalGroup("Common", Width = 0.5f), LabelWidth(60)]
    public float duration = 0.5f;

    [HorizontalGroup("Common"), LabelWidth(45)]
    public float delay = 0;

    [LabelWidth(60)]
    public Ease ease = Ease.OutQuad;

    /// <summary>
    /// 核心方法：讓每個子類別自己決定如何把動畫加入 Sequence
    /// </summary>
    /// <param name="seq">DOTween 序列</param>
    /// <param name="cg">UI 的 CanvasGroup (用於淡入淡出)</param>
    /// <param name="t">UI 的 Transform (用於位移縮放)</param>
    /// <param name="isIn">True=進場, False=退場</param>
    public abstract void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn);

    // 輔助方法：立即設定到初始狀態 (避免動畫開始前閃爍)
    protected abstract void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn);
}

/// <summary>
/// 淡入淡出 (Fade)
/// </summary>
[Serializable]
[LabelText("Fade (淡入淡出)")] // 在下拉選單顯示的名稱
public class FadeAnimation : ViewAnimationBase
{
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        float endValue = isIn ? toAlpha : fromAlpha;
        seq.Insert(delay, cg.DOFade(endValue, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        cg.alpha = isIn ? fromAlpha : toAlpha;
    }
}

/// <summary>
/// 縮放 (Scale)
/// </summary>
[Serializable]
[LabelText("Scale (縮放彈跳)")]
public class ScaleAnimation : ViewAnimationBase
{
    public Vector3 fromScale = Vector3.zero;
    public Vector3 toScale = Vector3.one;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        Vector3 endValue = isIn ? toScale : fromScale;
        seq.Insert(delay, t.DOScale(endValue, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        t.localScale = isIn ? fromScale : toScale;
    }
}

/// <summary>
/// 滑入滑出 (Slide)
/// </summary>
[Serializable]
[LabelText("Slide (滑動)")]
public class SlideAnimation : ViewAnimationBase
{
    public Vector2 offset = new Vector2(0, -100); // 預設從下方滑入

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        // 如果是進場：從 (offset) 移動到 (0,0)
        // 如果是退場：從 (目前位置) 移動到 (offset)
        Vector2 startPos = t.anchoredPosition + offset;
        Vector2 endPos = t.anchoredPosition; // 假設 UI 原始位置都在正確的地方

        ResetToStart(cg, t, isIn);

        if (isIn)
        {
            // 進場：目標是歸位
            seq.Insert(delay, t.DOAnchorPos(endPos, duration).SetEase(ease));
        }
        else
        {
            // 退場：目標是偏移
            seq.Insert(delay, t.DOAnchorPos(startPos, duration).SetEase(ease));
        }
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        if (isIn)
        {
            // 進場前瞬間設定到偏移位置
            t.anchoredPosition += offset;
        }
        // 退場通常直接從當前位置開始，不需要 Reset
    }
}

/// <summary>
/// 旋轉淡入 (Rotate Fade)
/// </summary>
[Serializable]
[LabelText("Rotate Fade (旋轉淡入)")]
public class RotateFadeAnimation : ViewAnimationBase
{
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;
    public Vector3 fromRotation = new Vector3(0, 0, 90);
    public Vector3 toRotation = Vector3.zero;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        float endAlpha = isIn ? toAlpha : fromAlpha;
        Vector3 endRotation = isIn ? toRotation : fromRotation;

        seq.Insert(delay, cg.DOFade(endAlpha, duration).SetEase(ease));
        seq.Insert(delay, t.DORotate(endRotation, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        cg.alpha = isIn ? fromAlpha : toAlpha;
        t.localEulerAngles = isIn ? fromRotation : toRotation;
    }
}

/// <summary>
/// 彈性縮放 (Bounce Scale)
/// </summary>
[Serializable]
[LabelText("Bounce Scale (彈性縮放)")]
public class BounceScaleAnimation : ViewAnimationBase
{
    public Vector3 fromScale = new Vector3(1.5f, 1.5f, 1f);
    public Vector3 toScale = Vector3.one;
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        Vector3 endScale = isIn ? toScale : fromScale;
        float endAlpha = isIn ? toAlpha : fromAlpha;

        seq.Insert(delay, t.DOScale(endScale, duration).SetEase(ease));
        seq.Insert(delay, cg.DOFade(endAlpha, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        t.localScale = isIn ? fromScale : toScale;
        cg.alpha = isIn ? fromAlpha : toAlpha;
    }
}

/// <summary>
/// 縮放滑動 (Scale Slide)
/// </summary>
[Serializable]
[LabelText("Scale Slide (縮放滑動)")]
public class ScaleSlideAnimation : ViewAnimationBase
{
    public Vector2 offset = new Vector2(0, 100);
    public Vector3 fromScale = new Vector3(0.8f, 0.8f, 1f);
    public Vector3 toScale = Vector3.one;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        Vector2 startPos = t.anchoredPosition + offset;
        Vector2 endPos = t.anchoredPosition;

        ResetToStart(cg, t, isIn);

        if (isIn)
        {
            seq.Insert(delay, t.DOAnchorPos(endPos, duration).SetEase(ease));
            seq.Insert(delay, t.DOScale(toScale, duration).SetEase(ease));
        }
        else
        {
            seq.Insert(delay, t.DOAnchorPos(startPos, duration).SetEase(ease));
            seq.Insert(delay, t.DOScale(fromScale, duration).SetEase(ease));
        }
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        if (isIn)
        {
            t.anchoredPosition += offset;
            t.localScale = fromScale;
        }
        else
        {
            t.localScale = toScale;
        }
    }
}

/// <summary>
/// 翻轉效果 (Flip)
/// </summary>
[Serializable]
[LabelText("Flip (翻轉)")]
public class FlipAnimation : ViewAnimationBase
{
    public enum FlipAxis { Horizontal, Vertical }
    public FlipAxis flipAxis = FlipAxis.Horizontal;
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        Vector3 fromScale = flipAxis == FlipAxis.Horizontal ? new Vector3(0, 1, 1) : new Vector3(1, 0, 1);
        Vector3 toScale = Vector3.one;
        Vector3 endScale = isIn ? toScale : fromScale;
        float endAlpha = isIn ? toAlpha : fromAlpha;

        seq.Insert(delay, t.DOScale(endScale, duration).SetEase(ease));
        seq.Insert(delay, cg.DOFade(endAlpha, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        Vector3 fromScale = flipAxis == FlipAxis.Horizontal ? new Vector3(0, 1, 1) : new Vector3(1, 0, 1);
        t.localScale = isIn ? fromScale : Vector3.one;
        cg.alpha = isIn ? fromAlpha : toAlpha;
    }
}

/// <summary>
/// 打字機效果 (配合位移)
/// </summary>
[Serializable]
[LabelText("Typewriter (打字機)")]
public class TypewriterAnimation : ViewAnimationBase
{
    public Vector2 offset = new Vector2(-50, 0);
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        Vector2 startPos = t.anchoredPosition + offset;
        Vector2 endPos = t.anchoredPosition;

        ResetToStart(cg, t, isIn);

        if (isIn)
        {
            seq.Insert(delay, t.DOAnchorPos(endPos, duration).SetEase(ease));
            seq.Insert(delay, cg.DOFade(toAlpha, duration).SetEase(ease));
        }
        else
        {
            seq.Insert(delay, t.DOAnchorPos(startPos, duration).SetEase(ease));
            seq.Insert(delay, cg.DOFade(fromAlpha, duration).SetEase(ease));
        }
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        if (isIn)
        {
            t.anchoredPosition += offset;
            cg.alpha = fromAlpha;
        }
        else
        {
            cg.alpha = toAlpha;
        }
    }
}

/// <summary>
/// 震動淡入 (Shake Fade)
/// </summary>
[Serializable]
[LabelText("Shake Fade (震動淡入)")]
public class ShakeFadeAnimation : ViewAnimationBase
{
    public float shakeStrength = 20f;
    public int vibrato = 10;
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        float endAlpha = isIn ? toAlpha : fromAlpha;

        if (isIn)
        {
            seq.Insert(delay, t.DOShakeAnchorPos(duration, shakeStrength, vibrato).SetEase(ease));
        }
        else
        {
            seq.Insert(delay, t.DOShakeAnchorPos(duration, shakeStrength, vibrato).SetEase(ease));
        }
        seq.Insert(delay, cg.DOFade(endAlpha, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        cg.alpha = isIn ? fromAlpha : toAlpha;
    }
}

/// <summary>
/// 圓形擴散 (Radial)
/// </summary>
[Serializable]
[LabelText("Radial (圓形擴散)")]
public class RadialAnimation : ViewAnimationBase
{
    public Vector3 fromScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 toScale = Vector3.one;
    public Vector3 fromRotation = new Vector3(0, 0, 180);
    public Vector3 toRotation = Vector3.zero;
    [Range(0, 1)] public float fromAlpha = 0f;
    [Range(0, 1)] public float toAlpha = 1f;

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        ResetToStart(cg, t, isIn);

        Vector3 endScale = isIn ? toScale : fromScale;
        Vector3 endRotation = isIn ? toRotation : fromRotation;
        float endAlpha = isIn ? toAlpha : fromAlpha;

        seq.Insert(delay, t.DOScale(endScale, duration).SetEase(ease));
        seq.Insert(delay, t.DORotate(endRotation, duration).SetEase(ease));
        seq.Insert(delay, cg.DOFade(endAlpha, duration).SetEase(ease));
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        t.localScale = isIn ? fromScale : toScale;
        t.localEulerAngles = isIn ? fromRotation : toRotation;
        cg.alpha = isIn ? fromAlpha : toAlpha;
    }
}

/// <summary>
/// 彈簧效果 (Spring)
/// </summary>
[Serializable]
[LabelText("Spring (彈簧)")]
public class SpringAnimation : ViewAnimationBase
{
    public Vector2 offset = new Vector2(0, 150);
    public Vector3 overshootScale = new Vector3(1.2f, 1.2f, 1f);

    public override void AppendToSequence(Sequence seq, CanvasGroup cg, RectTransform t, bool isIn)
    {
        Vector2 startPos = t.anchoredPosition + offset;
        Vector2 endPos = t.anchoredPosition;

        ResetToStart(cg, t, isIn);

        if (isIn)
        {
            seq.Insert(delay, t.DOAnchorPos(endPos, duration).SetEase(Ease.OutBack));
            seq.Insert(delay, t.DOScale(Vector3.one, duration).SetEase(Ease.OutBack));
        }
        else
        {
            seq.Insert(delay, t.DOAnchorPos(startPos, duration).SetEase(Ease.InBack));
            seq.Insert(delay, t.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
        }
    }

    protected override void ResetToStart(CanvasGroup cg, RectTransform t, bool isIn)
    {
        if (isIn)
        {
            t.anchoredPosition += offset;
            t.localScale = Vector3.zero;
        }
    }
}