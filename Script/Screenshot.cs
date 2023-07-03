using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Screenshot : MonoBehaviour
{
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
                instance = FindObjectOfType<Screenshot>();
            return instance;
        }
    }

    private static Screenshot instance;

    public event Action<Texture2D> OnTaked;

    public event Action<bool, string> OnUploaded;

    public Camera cam;  // 如果有選擇Camera的話就截圖這個Camera的畫面
    public int targetWidth;
    public int targetHeight;

    public bool isSequence = false;  // 打開選項的話，截圖並不會馬上上傳或儲存

    [Title("Local")]
    public bool isLocal = false;

    [ShowIf("isLocal")]
    public LocalPathMod localPathMod;

    [ShowIf("isLocal"), ShowIf("localPathMod", LocalPathMod.CustomPath)]
    public string localPath;

    [Title("Online")]
    public bool isOnline = false;

    [ShowIf("isOnline")]
    public string url = "https://starlitetw.com/api/qr_get_photo_upload_photo";

    [ShowIf("isOnline")]
    public string actid;

    private List<ShortData> shortDatas = new List<ShortData>();
    private ShortData shortData;
    private string path;
    private Texture2D tex;

    private string logPath = Application.streamingAssetsPath + "/Debug.txt";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (instance != this)
            Destroy(gameObject);
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
    }

    public void Take()
    {
        StartCoroutine(Shot());
    }

    public void Take(string fileName, int photoNum = 1)
    {
        shortData = new ShortData();
        shortData.fileName = fileName;
        shortData.photoNum = photoNum;

        StartCoroutine(Shot());
    }

    /// <summary>
    /// 上傳或儲存截圖
    /// </summary>
    public void Save()
    {
        foreach (ShortData data in shortDatas)
        {
            if (String.IsNullOrEmpty(data.fileName))
            {
                Debug.LogError("檔名不能為Null或空白");
                continue;
            }

            if (isLocal)
            {
                LocalSave(data.fileName, data.bytes);
            }

            if (isOnline)
            {
                StartCoroutine(WebSave(data.fileName, data.bytes, data.photoNum));
            }
        }

        shortDatas.Clear();
    }

    /// <summary>
    /// 清除序列
    /// </summary>
    public void ClearSeqence()
    {
        shortDatas.Clear();
    }

    private IEnumerator Shot()
    {
        ClearTexture();

        if (cam == null)
            cam = Camera.main;

        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        tex = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        tex.Apply();

        shortData.bytes = tex.EncodeToJPG();
        shortDatas.Add(shortData);

        Debug.Log("Screenshot captured!");

        RenderTexture.active = null;
        cam.targetTexture = null;
        Destroy(rt);

        OnTaked?.Invoke(tex);

        if (!isSequence)
        {
            if (isLocal || isOnline)
                Save();
        }
    }

    private void LocalSave(string fileName, byte[] bytes)
    {
        if (localPathMod == LocalPathMod.StreamingAssetsPath)
        {
            path = Path.Combine(Application.streamingAssetsPath, fileName + ".jpg");
        }
        else if (localPathMod == LocalPathMod.CustomPath)
        {
            path = Path.Combine(localPath, fileName + ".jpg");
        }

        File.WriteAllBytes(path, bytes);
    }

    private IEnumerator WebSave(string fileName, byte[] bytes, int photoNum)
    {
        WWWForm form = new WWWForm();
        form.AddField("actid", actid);
        form.AddField("photoid", fileName);
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
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    sw.WriteLine(DateTime.Now.ToString("MM/dd HH:mm ") + fileName + ".jpg" + " Upload Complete.");
                }
                Debug.Log("Upload Complete.");

                OnUploaded?.Invoke(true, "Upload Complete.");
            }
        }
    }

    private void ClearTexture()
    {
        if (tex != null)
        {
            Destroy(tex);
            tex = null;
        }
    }
}