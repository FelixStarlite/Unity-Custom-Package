using System.Collections.Generic;
using UnityEngine;

// 擴充方法必須在靜態類別中定義
public static class ListExtensions
{
    // 關鍵點：
    // 1. 方法必須是 static
    // 2. 第一個參數前面加上 "this" 關鍵字
    // 3. 使用泛型 <T> 讓它適用於任何類型的 List
    // 額外提供：返回新列表的版本（不修改原列表）
    public static List<T> Shuffle<T>(this List<T> list)
    {
        List<T> newList = new List<T>(list);
        int n = newList.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            // 交換元素
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return newList;
    }
}