using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/****************************************************
 這是一個計時器
 這個計時器可以設定時間，並在時間到時觸發事件

 使用方法：
 1. 將這個腳本掛在任意物件上
 2. 設定時間
 3. 訂閱 OnTimeUp 事件
 4. 呼叫 StartTimer() 開始計時
 5. 呼叫 StopTimer() 停止計時

 例如：
 Timer timer = gameObject.AddComponent<Timer>();
 timer.OnTimeUp += () => Debug.Log("Time's up!");
 timer.TimerPlay();
 * ****************************************************/

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timer = 30f;
    [SerializeField] private bool isAutoStart;
    [SerializeField] private UnityEvent onTimeUp;

    private Coroutine timerCoroutine;

    private void OnEnable()
    {
        if(isAutoStart)
            StartTimer();
    }

    private void Start()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timer).ToString();
    }

    /// <summary>
    /// 開始計時
    /// </summary>
    public void StartTimer()
    {
        if (timerCoroutine == null)
            StartCoroutine(Timering());
    }

    /// <summary>
    /// 停止計時
    /// </summary>
    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    /// <summary>
    /// 實現邏輯
    /// </summary>
    /// <returns></returns>
    private IEnumerator Timering()
    {
        float time = timer;
        while (time > 0)
        {
            time -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(time).ToString();

            yield return null;
        }

        if (timerText != null)
            timerText.text = "0";

        // 時間到，觸發事件
        onTimeUp?.Invoke();

        timerCoroutine = null;
    }
}