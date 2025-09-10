using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;

/*
 截圖程式
    1. 可以截取畫面
    2. 可以截取Camera畫面
    3. 可以儲存截圖
    4. 可以上傳截圖
    5. 可以設定截圖大小
    6. 可以設定截圖數量
 */

namespace Starlite
{
    public class Screenshot : MonoBehaviour
    {
        [Serializable]
        public struct ShortData
        {
            public string fileName;
            public int photoNum;
            public byte[] bytes;
        }

        public enum LocalPathMod
        {
            StreamingAssetsPath,
            CustomPath
        }

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

        public delegate UniTask WebUpload(Texture2D tex);
        public event WebUpload OnTaked;
        public event Action<bool, string> OnUploaded;
        public Camera cam; // 如果有選擇Camera的話就截圖這個Camera的畫面,會尋找有ShotCamera的物件
        public int targetWidth;
        public int targetHeight;
        public int number = 1;
        public bool isAutoPrint = true;
        [Title("Local")] public bool isLocal = false;
        [ShowIf("isLocal")] public LocalPathMod localPathMod;
        [ShowIf("isLocal"),InlineButton("GetLocatPath")]
        public string localPath;
        [Title("Online")] public bool isOnline = false;
        [ShowIf("isOnline")] public string url = "https://starlitetw.com/api/qr_get_photo_upload_photo";
        [ShowIf("isOnline")] public string actid;


        private List<ShortData> shortDatas = new List<ShortData>();
        private ShortData shortData;
        private string path;
        private bool isRenderTexture = true;
        private bool createdTemporaryRT = false;
        private Texture2D tex;
        private IEnumerator sequenceing;

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

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            if (cam != null && targetWidth == 0)
            {
                targetWidth = cam.targetTexture == null ? Screen.width : cam.targetTexture.width;
            }

            if (cam != null && targetHeight == 0)
            {
                targetHeight = cam.targetTexture == null ? Screen.height : cam.targetTexture.height;
            }
        }

        public void Take()
        {
            StartCoroutine(Shot());
        }

        public void Take(string fileName)
        {
            shortData = new ShortData();
            shortData.fileName = fileName;

            StartCoroutine(Shot());
        }

        /// <summary>
        /// 上傳或儲存截圖
        /// </summary>
        public void Print()
        {
            if (shortDatas.Count >= number)
            {
                Debug.Log("Upload Texture : " + shortDatas.Count);
                sequenceing = Sequenceing();
                StartCoroutine(sequenceing);
            }
        }

        /// <summary>
        /// 清除佇列
        /// </summary>
        public void ClearShortDatas()
        {
            shortDatas.Clear();
        }

        /// <summary>
        /// 開始將佇列中的資料進行儲存
        /// </summary>
        /// <returns></returns>
        private IEnumerator Sequenceing()
        {
            if (!isLocal && !isOnline)
            {
                ClearShortDatas();
                yield break;
            }

            for (int i = 0; i < shortDatas.Count; i++)
            {
                if (String.IsNullOrEmpty(shortDatas[i].fileName))
                {
                    Debug.LogError("檔名不能為Null或空白");
                    yield break;
                }

                if (isLocal)
                {
                    LocalSave(shortDatas[i].fileName + "_" + i, shortDatas[i].bytes);
                }

                if (isOnline)
                {
                    yield return StartCoroutine(WebSave(shortDatas[i].fileName, shortDatas[i].bytes,
                        shortDatas[i].photoNum));
                }
            }

            ClearShortDatas();
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
                ViewCapture();
            }
            else
            {
                CameraCapture();
            }
        }

        /// <summary>
        /// 對整個畫面進行截圖
        /// </summary>
        private void ViewCapture()
        {
            ClearTexture();

            tex = ScreenCapture.CaptureScreenshotAsTexture();

            RecordAndNotify(tex);
        }

        /// <summary>
        /// 對目標Camera進行截圖
        /// </summary>
        private void CameraCapture()
        {
            ClearTexture();

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

            RenderTexture prevActive = RenderTexture.active;

            try
            {
                RenderTexture.active = rt;
                cam.Render();

                tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                Debug.Log("Screenshot captured!");

                RecordAndNotify(tex);
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
        /// 本地儲存
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="bytes">截圖原始檔</param>
        private void LocalSave(string fileName, byte[] bytes)
        {
            if (localPathMod == LocalPathMod.StreamingAssetsPath)
            {
                path = Path.Combine(Application.streamingAssetsPath,
                    DateTime.Now.ToString("yyyyMMdd") + fileName + ".jpg");
            }
            else if (localPathMod == LocalPathMod.CustomPath)
            {
                // 沒有指定路徑就使用默認路徑
                if (string.IsNullOrEmpty(localPath))
                {
                    Debug.LogWarning("沒有指定路徑，使用默認路徑。");
                    localPath = Application.streamingAssetsPath;
                }

                // 指定路徑沒有資料夾就新增一個
                if (!Directory.Exists(localPath))
                    Directory.CreateDirectory(localPath);

                path = Path.Combine(localPath, DateTime.Now.ToString("yyyyMMdd") + fileName + ".jpg");
            }

            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// 後台儲存
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="bytes">截圖原始檔</param>
        /// <param name="photoNum">第幾張照片</param>
        /// <returns></returns>
        private IEnumerator WebSave(string fileName, byte[] bytes, int photoNum)
        {
            Debug.Log("actid : " + actid + " photoid : " + fileName + " photonum : " + photoNum);
            WWWForm form = new WWWForm();
            form.AddField("actid", actid);
            form.AddField("photoid", actid + "_" + fileName);
            form.AddField("photonum", photoNum);
            form.AddBinaryData("upload_photo", bytes, "image/jpg");

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);

                    OnUploaded?.Invoke(false, www.error);

                    // 連線有問題，就將陣列清空並停止序列
                    shortDatas.Clear();

                    StopCoroutine(sequenceing);
                }
                else
                {
                    Debug.Log("Upload Complete.");

                    OnUploaded?.Invoke(true, actid + "_" + fileName);
                }
            }
        }

        // 將擷取到的貼圖進行編碼、編號、加入清單、觸發事件與自動列印
        private void RecordAndNotify(Texture2D captured)
        {
            shortData.bytes = captured.EncodeToJPG();
            shortData.photoNum = shortDatas.Count + 1;
            shortDatas.Add(shortData);

            OnTaked?.Invoke(captured);

            if (isAutoPrint)
                Print();
        }

        // 取得截圖尺寸：優先使用設定值，否則回退到相機的 RT 或螢幕分辨率
        private void GetTargetSize(out int width, out int height)
        {
            // 若已明確設定則直接使用
            if (targetWidth > 0 && targetHeight > 0)
            {
                width = targetWidth;
                height = targetHeight;
                return;
            }

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
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }
    }
}