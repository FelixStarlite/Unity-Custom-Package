using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/****************************************************
 這是一個計時器
 這個計時器可以設定時間，並在時間到時觸發事件

 使用方法：
 1. 將這個腳本掛在任意物件上
 2. 設定時間
 3. 訂閱 OnTimeUp 事件或 timeUp 事件
 4. 呼叫 TimerPlay() 開始計時

 例如：
 Timer timer = gameObject.AddComponent<Timer>();
 timer.OnTimeUp += () => Debug.Log("Time's up!");
 timer.TimerPlay();
 * ****************************************************/

public class Timer : MonoBehaviour
{
    public event Action OnTimeUp;

    [SerializeField] private float timer = 30f;
    [SerializeField] private UnityEvent timeUp;

    public void TimerPlay()
    {
        StartCoroutine(Timering());
    }

    private IEnumerator Timering()
    {
        float time = timer;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        OnTimeUp?.Invoke();
        timeUp?.Invoke();
    }
}