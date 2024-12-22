using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class ViewController : MonoBehaviour
{
    [TabGroup("Tabs", "In Animation"), HideLabel]
    public ViewAnimation inAnimation;

    [TabGroup("Tabs", "Out Animation"), HideLabel]
    public ViewAnimation outAnimation;

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

    private CanvasGroup m_CanvasGroup;

    private Vector3 originPosition;
    private Vector3 originQuaternios;
    private Vector3 originScale;
    private Sequence sequence;

    public void Init()
    {
        gameObject.SetActive(false);
        originPosition = transform.localPosition;
        originQuaternios = transform.localEulerAngles;
        originScale = transform.localScale;
    }

    public bool IsPlaying()
    {
        if (sequence != null)
            return sequence.IsPlaying();

        return false;
    }

    [Button]
    public void ShowView(bool isFast = false)
    {
        if (sequence != null)
            sequence.Kill();

        if (isFast)
        {
            gameObject.SetActive(true);
            OnStartEnter?.Invoke();
            OnEnterFinish?.Invoke();
        }
        else
        {
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();

            if (inAnimation.mod == AnimationMod.None)
            {
                m_CanvasGroup.alpha = 0;
                transform.localPosition = originPosition;
                transform.localScale = originScale;

                gameObject.SetActive(true);
                OnStartEnter?.Invoke();
                sequence = DOTween.Sequence();
                sequence.SetDelay(inAnimation.delay)
                        .AppendCallback(() => m_CanvasGroup.alpha = 1);
                OnEnterFinish?.Invoke();
            }
            else
            {
                m_CanvasGroup.alpha = inAnimation.start.alpha;
                transform.localPosition = inAnimation.start.position;
                transform.localScale = inAnimation.start.scale;

                gameObject.SetActive(true);
                OnStartEnter?.Invoke();
                sequence = DOTween.Sequence();
                sequence.SetDelay(inAnimation.delay)
                        .Append(m_CanvasGroup.DOFade(inAnimation.end.alpha, inAnimation.duration))
                        .Insert(0, transform.DOLocalMove(inAnimation.end.position, inAnimation.duration))
                        .Insert(0, transform.DOScale(inAnimation.end.scale, inAnimation.duration))
                        .SetEase(inAnimation.ease)
                        .OnComplete(() =>
                        {
                            OnEnterFinish?.Invoke();
                        });
            }
        }
    }

    [Button]
    public void HideView(bool isFast = false)
    {
        if (sequence != null)
            sequence.Kill();

        if (isFast)
        {
            gameObject.SetActive(false);
            OnStartExit?.Invoke();
            OnExitFinish?.Invoke();
        }
        else
        {
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();

            if (outAnimation.mod == AnimationMod.None)
            {
                m_CanvasGroup.alpha = 1;
                transform.localPosition = originPosition;
                transform.localScale = originScale;

                OnStartExit?.Invoke();
                sequence = DOTween.Sequence();
                sequence.SetDelay(outAnimation.delay)
                        .AppendCallback(() =>
                        {
                            m_CanvasGroup.alpha = 0;
                            OnExitFinish?.Invoke();
                            gameObject.SetActive(false);
                        });
            }
            else
            {
                m_CanvasGroup.alpha = outAnimation.start.alpha;
                transform.localPosition = outAnimation.start.position;
                transform.localScale = outAnimation.start.scale;

                OnStartExit?.Invoke();
                sequence = DOTween.Sequence();
                sequence.SetDelay(outAnimation.delay)
                        .Append(m_CanvasGroup.DOFade(outAnimation.end.alpha, outAnimation.duration))
                        .Insert(0, transform.DOLocalMove(outAnimation.end.position, outAnimation.duration))
                        .Insert(0, transform.DOScale(outAnimation.end.scale, outAnimation.duration))
                        .SetEase(outAnimation.ease)
                        .OnComplete(() =>
                        {
                            OnExitFinish?.Invoke();
                            gameObject.SetActive(false);
                        });
            }
        }
    }

    private void Reset()
    {
        RectTransform rt = transform as RectTransform;
        Rect rect = rt.rect;
        inAnimation = new ViewAnimation((int)rect.width, (int)rect.height);
    }
}