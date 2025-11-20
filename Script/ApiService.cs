using System;
using System.Collections.Generic;
using System.Threading;
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
        /// GET 請求
        /// </summary>
        public static async UniTask<ApiResponse<T>> GetAsync<T>(
            string url,
            string token = null,
            CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                SetCommonHeaders(request, token);
                return await SendRequestAsync<T>(request, cancellationToken);
            }
        }

        /// <summary>
        /// POST 請求
        /// </summary>
        public static async UniTask<ApiResponse<T>> PostAsync<T>(
            string url,
            List<IMultipartFormSection> form,
            string token = null,
            CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                SetCommonHeaders(request, token);
                return await SendRequestAsync<T>(request, cancellationToken);
            }
        }

        /// <summary>
        /// 核心請求發送方法
        /// </summary>
        private static async UniTask<ApiResponse<T>> SendRequestAsync<T>(
            UnityWebRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 發送請求並等待完成
                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                // 解析回應
                string responseText = request.downloadHandler.text;
                Debug.Log($"API 回應: {responseText}");

                ApiResponse<T> response = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);
                return response;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("請求已取消");
                return new ApiResponse<T>
                {
                    status = false,
                    msg = "請求已取消",
                    data = default(T)
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"請求異常: {ex.Message}\n{ex.StackTrace}");
                string responseText = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    ApiResponse<T> response = JsonConvert.DeserializeObject<ApiResponse<T>>(responseText);
                    return response;
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        status = false,
                        msg = ex.Message,
                        data = default(T)
                    };
                }
            }
        }

        /// <summary>
        /// 設定通用請求標頭 (例如: Token)
        /// </summary>
        private static void SetCommonHeaders(UnityWebRequest request, string token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("X-Tablet-Token", token);
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
    public bool status;
    public string msg;
    public T data;
}