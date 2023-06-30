using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using Sirenix.OdinInspector;

public class Screenshot : MonoBehaviour
{
    public event Action<Texture2D> OnTaked;

    public event Action<bool, string> OnUploaded;

    public bool isPreview = false;  // 打開選項的話，截圖並不會馬上上傳或儲存

    [Title("Local")]
    public bool isLocal = false;

    [ShowIf("isLocal")]
    public string localPath;

    [Title("Online")]
    public bool isOnline = false;

    [ShowIf("isOnline")]
    public string url;

    [ShowIf("isOnline")]
    public string actid;

    private string fileName;
    private Texture2D tex;

    private string logPath = Application.streamingAssetsPath + "/Debug.txt";

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

    public void Take(string fileName)
    {
        this.fileName = fileName;
        StartCoroutine(Shot());
    }

    /// <summary>
    /// 上傳或儲存截圖
    /// </summary>
    public void Save()
    {
        if (String.IsNullOrEmpty(fileName))
        {
            Debug.LogError("檔名不能為Null或空白");
            return;
        }

        byte[] bytes = tex.EncodeToJPG();

        if (isLocal)
        {
            LocalSave(fileName, bytes);
        }

        if (isOnline)
        {
            StartCoroutine(WebSave(fileName, bytes));
        }
    }

    private IEnumerator Shot()
    {
        Rect rect = new Rect(0, 0, 1920, 1080);
        tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, true);
        yield return new WaitForEndOfFrame();

        tex.ReadPixels(rect, 0, 0);
        tex.Apply();

        OnTaked?.Invoke(tex);

        if (isPreview) yield break;

        if (isLocal || isOnline)
            Save();
    }

    private void LocalSave(string fileName, byte[] bytes)
    {
        string path = Path.Combine(localPath, fileName + ".jpg");
        File.WriteAllBytes(path, bytes);
    }

    private IEnumerator WebSave(string fileName, byte[] bytes)
    {
        WWWForm form = new WWWForm();
        form.AddField("actid", actid);
        form.AddField("photoid", fileName);
        form.AddField("photonum", 1);
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
}