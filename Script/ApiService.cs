using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;


namespace Starlite
{
    /// <summary>
    /// API 服務類別，用於處理與伺服器進行的 HTTP 請求操作。
    /// </summary>
    public static class ApiService
    {
        /// <summary>
        /// 發送 HTTP POST 請求至指定的 URL，並附帶多部分表單資料進行上傳。
        /// </summary>
        /// <typeparam name="T">接收資料的目標類型，必須為參考型別。</typeparam>
        /// <param name="url">指定的目標 URL。</param>
        /// <param name="form">包含多部分表單資料的集合。</param>
        /// <returns>反序列化後的回應資料物件，包含狀態、訊息及數據。</returns>
        /// <exception cref="Exception">當請求失敗或伺服器回傳錯誤時拋出。</exception>
        public static async UniTask<ApiResponse<T>> Upload<T>(string url, List<IMultipartFormSection> form) where T : class
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                try
                {
                    await request.SendWebRequest().ToUniTask();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"資料上傳成功: {request.downloadHandler.text}");
                        var data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                        return new ApiResponse<T>
                        {
                            IsSuccess = true,
                            Message = $"資料上傳成功: {request.downloadHandler.text}",
                            Data = data
                        };

                        //如果資料上傳成功，但是狀態有可能是失敗，可以使用型別檢查來進行強轉型(需要實作介面)
                        // public interface IStatusToken
                        // {
                        //     bool status { get; }
                        // }
                        //============
                        // bool successFlag = data is IStatusToken statusToken ? statusToken.status : true;
                        // return new ApiResponse<T>
                        // {
                        //     isSuccess = data.status,
                        //     message = $"資料上傳成功: {request.downloadHandler.text}",
                        //     data = data
                        // };
                    }

                    Debug.LogError($"資料上傳失敗: 錯誤類型: {request.result}, 錯誤碼: {request.responseCode}, 錯誤訊息: {request.error}");
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = $"資料上傳失敗: 錯誤類型: {request.result}, 錯誤碼: {request.responseCode}, 錯誤訊息: {request.error}",
                        Data = null
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError($"上傳資料發生錯誤: {e.Message}\n{e.StackTrace}");
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = $"上傳資料發生錯誤: {e.Message}",
                        Data = null
                    };
                }
            }
        }
    }

    /// <summary>
    /// 通用回傳格式
    /// </summary>
    [Serializable]
    public class ApiResponse<T>
    {
        public bool IsSuccess;
        public string Message;
        public T Data;
    }
}