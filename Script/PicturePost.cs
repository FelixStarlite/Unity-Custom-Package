using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class PicturePost : MonoBehaviour
{
    // 上傳圖片
    public static async UniTask<string> PostPicture(byte[] bytes)
    {
        var timestamp = DateTime.Now.Ticks;
        string photoid = timestamp.ToString("X");

        WWWForm wf = new WWWForm();
        wf.AddField("actid", "test_2023");
        wf.AddField("photoid", photoid);
        wf.AddField("photonum", 1);
        wf.AddBinaryData("upload_photo", bytes, "image.jpg", "Image/jpg");

        using (UnityWebRequest www = UnityWebRequest.Post("https://starlitetw.com/api/qr_get_photo_upload_photo", wf))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Post Picture Error : " + www.error);
                return null;
            }
            else
            {
                string downloadURL = "https://starlitetw.com/qr_get_photo/" + photoid;
                Debug.Log("Post Picture Success : " + downloadURL);
                return downloadURL;
            }
        }
    }
}

[Serializable]
public class PostData
{
    public string status;
    public string msg;
}