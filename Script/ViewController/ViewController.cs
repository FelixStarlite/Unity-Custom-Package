using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using static ViewController;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class ViewController : MonoBehaviour
{
    [TabGroup("In Animation"), HideLabel]
    public ViewAnimation inAnimation = new ViewAnimation();

    [TabGroup("Out Animation"), HideLabel]
    public ViewAnimation outAnimation = new ViewAnimation();

    [TabGroup("In Animation")]
    public UnityEvent onEnter;

    [TabGroup("Out Animation")]
    public UnityEvent onExit;

    private CanvasGroup m_CanvasGroup;

    private void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    private void Start()
    {
    }

    [Button]
    public void ShowView()
    {
        if (inAnimation.mod == AnimationMod.None)
        {
            m_CanvasGroup.alpha = 0;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            gameObject.SetActive(true);
            m_CanvasGroup.DOFade(1, 0).SetDelay(inAnimation.delay);
            onEnter?.Invoke();
        }
        else
        {
            m_CanvasGroup.alpha = inAnimation.start.alpha;
            transform.localPosition = inAnimation.start.position;
            transform.localScale = inAnimation.start.scale;

            gameObject.SetActive(true);
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(inAnimation.delay)
                    .Append(m_CanvasGroup.DOFade(inAnimation.end.alpha, inAnimation.duration))
                    .Insert(0, transform.DOLocalMove(inAnimation.end.position, inAnimation.duration))
                    .Insert(0, transform.DOScale(inAnimation.end.scale, inAnimation.duration))
                    .OnComplete(() =>
                    {
                        onEnter?.Invoke();
                    });
        }
    }

    [Button]
    public void HideView()
    {
        if (outAnimation.mod == AnimationMod.None)
        {
            m_CanvasGroup.alpha = 1;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            m_CanvasGroup.DOFade(0, 0).SetDelay(outAnimation.delay).OnComplete(() =>
            {
                onEnter?.Invoke();
                gameObject.SetActive(false);
            });
        }
        else
        {
            m_CanvasGroup.alpha = outAnimation.start.alpha;
            transform.localPosition = outAnimation.start.position;
            transform.localScale = outAnimation.start.scale;

            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(outAnimation.delay)
                    .Append(m_CanvasGroup.DOFade(outAnimation.end.alpha, outAnimation.duration))
                    .Insert(0, transform.DOLocalMove(outAnimation.end.position, outAnimation.duration))
                    .Insert(0, transform.DOScale(outAnimation.end.scale, outAnimation.duration))
                    .OnComplete(() =>
                    {
                        onExit?.Invoke();
                        gameObject.SetActive(false);
                    });
        }
    }
}