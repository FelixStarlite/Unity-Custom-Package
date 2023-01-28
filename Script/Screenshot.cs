using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class Screenshot : MonoBehaviour
{
    public event Action<byte[]> OnTaked;

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

    private IEnumerator Shot()
    {
        Rect rect = new Rect(0, 0, 1920, 1080);
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, true);
        yield return new WaitForEndOfFrame();

        tex.ReadPixels(rect, 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToJPG();

        OnTaked?.Invoke(bytes);

        if (isLocal || isOnline)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                Debug.LogError("檔名不能為Null或空白");
                yield break;
            }
        }

        if (isLocal)
        {
            string path = Path.Combine(localPath, fileName + ".jpg");
            File.WriteAllBytes(path, bytes);
        }

        if (isOnline)
        {
            StartCoroutine(WebSave(fileName, bytes));
        }
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
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    sw.WriteLine(DateTime.Now.ToString("MM/dd HH:mm ") + fileName + ".jpg" + " Upload Complete.");
                }
                Debug.Log("Upload Complete.");
            }
        }
    }
}