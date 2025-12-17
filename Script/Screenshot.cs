using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Newtonsoft.Json;

namespace Starlite
{
    /// <summary>
    /// 截圖工具類 - 用於在 Unity 中進行螢幕截圖並支援本地儲存或上傳至遠端伺服器
    /// 
    /// 使用方法：
    /// 1. 將此腳本掛載到場景中的 GameObject 上，或通過 Screenshot.Instance 自動建立單例
    /// 2. 設定截圖參數：
    ///    - cam: 指定要截圖的相機（留空則截取整個畫面）
    ///    - rect: 設定截圖範圍（留空則使用全螢幕）
    ///    - isLocal: 啟用本地儲存，並設定 localPathMod 和 localPath
    ///    - isOnline: 啟用線上上傳，並設定 url 和 token
    /// 3. 呼叫 Take() 或 Take(fileName) 進行截圖
    /// 4. 截圖完成後會觸發 OnTaked 事件，上傳完成後會觸發 OnUploaded 事件
    /// 5. 若 isAutoSave 為 false，需手動呼叫 UploadAndSave() 來儲存或上傳截圖
    /// </summary>
    public class Screenshot : MonoBehaviour
    {
        public enum LocalPathMod
        {
            StreamingAssetsPath,
            CustomPath
        }

        /// <summary>
        /// 序列容器
        /// </summary>
        [Serializable]
        public class TexSequenced
        {
            public string fileName;
            public Texture2D tex;
        }

        // 單例模式實作 - 確保場景中只有一個 Screenshot 實例，並在場景切換時保持存在
        public static Screenshot Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<Screenshot>();

                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(Screenshot).Name);
                    instance = go.AddComponent<Screenshot>();
                    DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        private static Screenshot instance;

        // 事件回調機制 - 訂閱這些事件以在截圖完成或上傳完成時執行自訂邏輯
        public event Action<Texture2D> OnTaked; // 完成截圖事件 - 當截圖完成時觸發，傳回截圖的 Texture2D
        public delegate void UploadedAction<T>(ApiResponse<T> result);
        public event UploadedAction<UploadResult> OnUploaded; // 完成上傳事件 - 當上傳完成時觸發，傳回 API 回應結果

        [Tooltip("指定Camera(可留空)")] public Camera cam; // 如果有選擇Camera的話就截圖這個Camera的畫面,會尋找有ShotCamera的物件
        [Tooltip("截圖範圍")] public Rect rect;
        [SerializeField, Tooltip("是否自動儲存截圖")] private bool isAutoSave = true; // 是否自動儲存，若為 false 則需手動呼叫 UploadAndSave()
        [Title("Local")]
        public bool isLocal;
        [ShowIf("isLocal"), Tooltip("本地儲存方式")] public LocalPathMod localPathMod;
        [ShowIf("isLocal"), InlineButton("GetLocatPath"), Tooltip("儲存路徑")]
        public string localPath;
        [Title("Online")]
        public bool isOnline;
        [ShowIf("isOnline"), Tooltip("API網址")] public string url = "https://system-qr.starlitetw-project.com/api/upload";
        [ShowIf("isOnline"), Tooltip("API金鑰")] public string token;

        // 工作流程：Take() -> Shot() -> (ViewCapture/CameraCapture) -> OnTaked 事件 -> UploadAndSave() -> (LocalSave/WebSave) -> OnUploaded 事件
        private bool createdTemporaryRT; // 標記是否使用新建RenderTexture
        private TexSequenced texSequenced;
        private List<TexSequenced> TexSequenceds = new(); // 序列模式用來儲存截圖 - 支援批次處理多張截圖

#if UNITY_EDITOR
        /// <summary>
        /// 取得儲存檔案路徑
        /// </summary>
        private void GetLocatPath()
        {
            string path = EditorUtility.SaveFolderPanel("Save Screenshot", Application.streamingAssetsPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                localPath = path;
            }
        }
#else
        private void GetLocatPath() { }
#endif

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 進行截圖
        /// </summary>
        [Button]
        public void Take()
        {
            // 使用日期標籤做為檔名
            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            texSequenced = new();
            texSequenced.fileName = fileName;
            StartCoroutine(Shot());
        }

        /// <summary>
        /// 進行截圖並使用自訂的檔名
        /// </summary>
        /// <param name="fileName"></param>
        public void Take(string fileName)
        {
            texSequenced = new();
            texSequenced.fileName = fileName;
            StartCoroutine(Shot());
        }

        /// <summary>
        /// 選擇截圖的方式
        /// ViewCapture:直接截取整個畫面
        /// CameraCapture:對目標Camera進行截圖
        /// </summary>
        /// <returns></returns>
        private IEnumerator Shot()
        {
            yield return new WaitForEndOfFrame();

            if (cam == null)
            {
                texSequenced.tex = ViewCapture();
            }
            else
            {
                texSequenced.tex = CameraCapture();
            }

            TexSequenceds.Add(texSequenced);

            Debug.Log("截圖完成！");
            OnTaked?.Invoke(texSequenced.tex);

            if (isAutoSave)
            {
                UploadAndSave();
            }
        }

        /// <summary>
        /// 對整個畫面進行截圖
        /// </summary>
        private Texture2D ViewCapture()
        {
            return ScreenCapture.CaptureScreenshotAsTexture();
        }

        /// <summary>
        /// 對目標Camera進行截圖
        /// </summary>
        private Texture2D CameraCapture()
        {
            // 取得實際截圖寬高（若未設定則回退到相機 RT 或螢幕大小）
            GetTargetSize(out int width, out int height);

            RenderTexture rt = cam.targetTexture;
            createdTemporaryRT = false;

            // 如果沒有RenderTexture就新增一個（臨時）
            if (rt == null)
            {
                rt = new RenderTexture(width, height, 24);
                cam.targetTexture = rt;
                createdTemporaryRT = true;
            }

            // 將目前啟用的RT暫存
            RenderTexture prevActive = RenderTexture.active;

            try
            {
                // 啟動新的RT內容進行截圖
                RenderTexture.active = rt;
                cam.Render();

                // 若未設定 rect（或為 0），預設抓整張 RT
                Rect readRect = rect;
                if (readRect.width <= 0f || readRect.height <= 0f)
                {
                    readRect = new Rect(0, 0, width, height);
                }

                // Clamp，避免超出 RT 範圍造成黑圖/錯位
                float x = Mathf.Clamp(readRect.x, 0, width - 1);
                float y = Mathf.Clamp(readRect.y, 0, height - 1);
                float w = Mathf.Clamp(readRect.width, 1, width - x);
                float h = Mathf.Clamp(readRect.height, 1, height - y);
                readRect = new Rect(x, y, w, h);

                Texture2D texture = new Texture2D((int)w, (int)h, TextureFormat.RGB24, false);
                texture.ReadPixels(readRect, 0, 0);
                texture.Apply();
                Debug.Log("Screenshot captured!");
                return texture;
            }
            finally
            {
                RenderTexture.active = prevActive;

                // 僅在建立了臨時 RT 時才清理
                if (createdTemporaryRT)
                {
                    cam.targetTexture = null;
                    Destroy(rt);
                    createdTemporaryRT = false;
                }
            }
        }

        /// <summary>
        /// 上傳或儲存截圖
        /// </summary>
        [Button]
        public void UploadAndSave()
        {
            if (TexSequenceds.Count == 0) return;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            foreach (var texSequenced in TexSequenceds)
            {
                if (isLocal)
                {
                    LocalSave(texSequenced.tex, texSequenced.fileName);
                }

                if (isOnline)
                {
                    formData.Add(new MultipartFormFileSection("photos[]", texSequenced.tex.EncodeToJPG(), texSequenced.fileName, "image/jpg"));
                }
            }

            if (isOnline)
            {
                WebSave(formData).Forget();
            }

            ClearTexture();
        }

        /// <summary>
        /// 將截圖儲存在本地
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="bytes">截圖原始檔</param>
        private void LocalSave(Texture2D tex, string fileName)
        {
            string path = "";
            if (localPathMod == LocalPathMod.StreamingAssetsPath)
            {
                localPath = Application.streamingAssetsPath;
                path = Path.Combine(localPath, fileName + ".jpg");
            }
            else if (localPathMod == LocalPathMod.CustomPath)
            {
                // 沒有指定路徑就使用默認路徑
                if (string.IsNullOrEmpty(localPath))
                {
                    Debug.LogWarning("沒有指定路徑，使用默認路徑。");
                    localPath = Application.streamingAssetsPath;
                }

                path = Path.Combine(localPath, fileName + ".jpg");
            }

            // 指定路徑沒有資料夾就新增一個
            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);

            File.WriteAllBytes(path, tex.EncodeToJPG());
        }

        /// <summary>
        /// 將截圖上傳到網路後台
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="bytes">截圖原始檔</param>
        /// <param name="photoNum">第幾張照片</param>
        /// <returns></returns>
        private async UniTaskVoid WebSave(List<IMultipartFormSection> formData)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
            {
                www.SetRequestHeader("Authorization", $"Bearer {token}");

                try
                {
                    await www.SendWebRequest();

                    string body = www.downloadHandler != null ? www.downloadHandler.text : null;
                    // 成功就嘗試反序列化；失敗也要回呼，避免外部流程卡住
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"截圖成功上傳: {body}");
                        var ok = JsonConvert.DeserializeObject<ApiResponse<UploadResult>>(body);
                        OnUploaded?.Invoke(ok);
                    }
                }
                catch (Exception e)
                {
                    string body = www.downloadHandler != null ? www.downloadHandler.text : null;

                    Debug.LogError($"截圖上傳失敗: {e.Message}");

                    // 先嘗試從回傳的資料進行解析
                    var err = JsonConvert.DeserializeObject<ApiResponse<UploadResult>>(body);
                    if (err != null)
                    {
                        err.error_code = www.responseCode;
                        OnUploaded?.Invoke(err);
                        return;
                    }

                    // 解析失敗就使用通用錯誤
                    OnUploaded?.Invoke(new ApiResponse<UploadResult>
                    {
                        status = false,
                        msg = www.error,
                        error_code = www.responseCode,
                        data = null
                    });
                }
            }
        }

        /// <summary>
        /// 取得RenderTexture大小：相機的 RenderTexture 或螢幕分辨率
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void GetTargetSize(out int width, out int height)
        {
            // 若相機已有 RT，使用其尺寸；否則使用螢幕尺寸
            if (cam != null && cam.targetTexture != null)
            {
                width = cam.targetTexture.width;
                height = cam.targetTexture.height;
            }
            else
            {
                width = Screen.width;
                height = Screen.height;
            }
        }

        /// <summary>
        /// 刪除紋理，防止記憶體泄露
        /// </summary>
        private void ClearTexture()
        {
            if (TexSequenceds.Count > 0)
            {
                foreach (var texSequenced in TexSequenceds)
                {
                    Destroy(texSequenced.tex);
                }

                TexSequenceds.Clear();
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
    public long error_code;
    public T data;
}

/// <summary>
/// 上傳截圖結果
/// </summary>
[Serializable]
public class UploadResult
{
    public string session_uuid;
    public string qr_url;
    public int photo_count;
}