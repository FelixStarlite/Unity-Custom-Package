using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 使用滑動塊進行頁面選擇，需要DoTweenPro插件
/// </summary>
public class ScrollPageController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect; // 核心組件
    [SerializeField] private RectTransform content; // 內容區塊
    [SerializeField] private GameObject backBtn, nextBtn; // 上下頁切換按鈕

    private int pageCount = 0; // 總頁數
    private int currentPage = 1; //當前頁數
    private Tween tween; // 控制動畫的類別

    private void Start()
    {
        // 取得頁數
        pageCount = content.childCount;

        // 將conten寬度設定
        content.sizeDelta = new Vector2(Screen.width * pageCount, content.sizeDelta.y);
    }

    private void Update()
    {
        nextBtn.SetActive(currentPage < pageCount);
        backBtn.SetActive(currentPage > 1);
    }

    /// <summary>
    /// 下一頁
    /// </summary>
    public void Next()
    {
        currentPage = currentPage + 1 > pageCount ? currentPage : currentPage + 1;
        MovePage(currentPage);
    }

    /// <summary>
    /// 上一頁
    /// </summary>
    public void Back()
    {
        currentPage = currentPage - 1 < 1 ? currentPage : currentPage - 1;
        MovePage(currentPage);
    }

    /// <summary>
    /// 移動頁面
    /// </summary>
    /// <param name="page"></param>
    /// <param name="isfast"></param>
    private void MovePage(int page, bool isfast = false)
    {
        float position = 0;
        if (currentPage > 1)
        {
            position = (float)page / (float)pageCount;
        }

        Debug.Log(position);

        if (tween != null)
            tween.Kill();

        if (isfast)
        {
            scrollRect.horizontalNormalizedPosition = position;
        }
        else
        {
            tween = DOTween.To(() => scrollRect.horizontalNormalizedPosition, x => scrollRect.horizontalNormalizedPosition = x, position, 0.5f);
        }
    }
}