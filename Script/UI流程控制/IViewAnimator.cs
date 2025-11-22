using System;
using UnityEngine.Events;

// 定義動畫播放器的標準
public interface IViewAnimator
{
    // 播放進場動畫
    void PlayIn(ViewAnimationBase animationData, UnityEvent onComplete,bool isFast);

    // 播放退場動畫
    void PlayOut(ViewAnimationBase animationData, UnityEvent onComplete, bool isFast);

    // 強制停止當前動畫
    void Stop();

    // 查詢動畫播放狀態
    bool IsPlaying();
}