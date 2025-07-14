using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using Sirenix.OdinInspector;
using System.Collections.Generic;

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

        public event Action<Texture2D> OnTaked;

        public event Action<bool, string> OnUploaded;

        public Camera cam; // 如果有選擇Camera的話就截圖這個Camera的畫面,會尋找有ShotCamera的物件
        public int targetWidth;
        public int targetHeight;
        public int number = 1;
        public bool isAutoPrint = true;

        [Title("Local")] public bool isLocal = false;

        [ShowIf("isLocal")] public LocalPathMod localPathMod;

        [ShowIf("isLocal"), ShowIf("localPathMod", LocalPathMod.CustomPath)]
        public string localPath;

        [Title("Online")] public bool isOnline = false;

        [ShowIf("isOnline")] public string url = "https://starlitetw.com/api/qr_get_photo_upload_photo";

        [ShowIf("isOnline")] public string actid;

        private List<ShortData> shortDatas = new List<ShortData>();
        private ShortData shortData;
        private string path;
        private bool isRenderTexture = true;
        private Texture2D tex;
        private IEnumerator sequenceing;

        private string logPath = Application.streamingAssetsPath + "/Debug.txt";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            if (!File.Exists(logPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(logPath))
                {
                    sw.WriteLine("=====RecodeLog=====");
                }
            }

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

            shortData.bytes = tex.EncodeToJPG();
            shortData.photoNum = shortDatas.Count + 1;
            shortDatas.Add(shortData);

            OnTaked?.Invoke(tex);

            if (isAutoPrint)
                Print();
        }

        /// <summary>
        /// 對目標Camera進行截圖
        /// </summary>
        private void CameraCapture()
        {
            ClearTexture();

            //if (targetWidth == 0)
            //{
            //    targetWidth = cam.targetTexture == null ? Screen.width : cam.targetTexture.width;
            //}

            //if (targetHeight == 0)
            //{
            //    targetHeight = cam.targetTexture == null ? Screen.height : cam.targetTexture.height;
            //}

            RenderTexture rt = cam.targetTexture;
            // 如果沒有RenderTexture就新增一個
            if (rt == null)
            {
                rt = new RenderTexture(targetWidth, targetHeight, 24);
                cam.targetTexture = rt;
                cam.Render();
                isRenderTexture = false;
            }

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = rt;
            cam.Render();

            tex = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            tex.Apply();

            shortData.bytes = tex.EncodeToJPG();
            shortData.photoNum = shortDatas.Count + 1;
            shortDatas.Add(shortData);

            Debug.Log("Screenshot captured!");

            RenderTexture.active = currentRT;
            // 沒有默認的RenderTexture才會執行
            if (!isRenderTexture)
            {
                cam.targetTexture = null;
                Destroy(rt);
            }

            OnTaked?.Invoke(tex);

            if (isAutoPrint)
                Print();
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
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.WriteLine(DateTime.Now.ToString("MM/dd HH:mm ") + fileName + ".jpg" + " " + www.error);
                    }

                    Debug.Log(www.error);

                    OnUploaded?.Invoke(false, www.error);

                    // 連線有問題，就將陣列清空並停止序列
                    shortDatas.Clear();

                    StopCoroutine(sequenceing);
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.WriteLine(DateTime.Now.ToString("MM/dd HH:mm ") + fileName + ".jpg" + " Upload Complete.");
                    }

                    Debug.Log("Upload Complete.");

                    OnUploaded?.Invoke(true, actid + "_" + fileName);
                }
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