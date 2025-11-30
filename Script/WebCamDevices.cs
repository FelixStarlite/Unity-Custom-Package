using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RawImage displayImage;
    [SerializeField] private AspectRatioFitter ratioFitter;

    [Header("Settings")]
    [SerializeField] private bool autoStart = true;

    // 核心變數
    public WebCamTexture CurrentWebCam { get; private set; }
    private WebCamDevice[] devices;
    private int currentDeviceIndex = 0; // 追蹤當前使用的鏡頭索引

    // 用來判斷是否為前鏡頭 (結合了硬體屬性與字串推斷)
    private bool isCurrentFrontFacing;

    private int lastVideoRotationAngle = -1;
    public System.Action<Texture2D> OnPhotoCaptured;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (displayImage == null) displayImage = GetComponent<RawImage>();
        if (ratioFitter == null) ratioFitter = GetComponent<AspectRatioFitter>();
    }

    private IEnumerator Start()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                if (autoStart)
                {
                    // 預設啟動索引 0 的相機 (通常是後鏡頭，但視裝置而定)
                    StartCameraByIndex(0);
                }
            }
            else
            {
                Debug.LogError("未偵測到相機裝置");
            }
        }
    }

    private void Update()
    {
        if (CurrentWebCam != null && CurrentWebCam.isPlaying)
        {
            if (lastVideoRotationAngle != CurrentWebCam.videoRotationAngle)
            {
                UpdatePreviewOrientation();
            }
        }
    }

    /// <summary>
    /// 透過索引啟動相機 (最穩定的做法)
    /// </summary>
    private void StartCameraByIndex(int index)
    {
        StopCamera();

        if (devices == null || devices.Length == 0) return;

        // 防止索引越界
        currentDeviceIndex = Mathf.Clamp(index, 0, devices.Length - 1);
        WebCamDevice device = devices[currentDeviceIndex];

        // 優先信任 isFrontFacing，如果硬體回傳 false，則嘗試用名稱推斷
        isCurrentFrontFacing = device.isFrontFacing;

        // 如果硬體說它不是前鏡頭 (或全都是 false)，我們進行二次確認
        if (!isCurrentFrontFacing)
        {
            isCurrentFrontFacing = InferIsFrontFacingByName(device.name);
        }

        Debug.Log($"啟動相機: {device.name} | IsFront: {isCurrentFrontFacing}");

        // 初始化
        CurrentWebCam = new WebCamTexture(device.name, 1920, 1080);

        if (displayImage != null)
        {
            displayImage.texture = CurrentWebCam;
            displayImage.material.mainTexture = CurrentWebCam;
        }

        CurrentWebCam.Play();
        StartCoroutine(WaitForCameraReady());
    }

    /// <summary>
    /// 名稱推斷邏輯 (Heuristic)
    /// 當 isFrontFacing 失效時，透過字串猜測
    /// </summary>
    private bool InferIsFrontFacingByName(string deviceName)
    {
        string lowerName = deviceName.ToLower();

        // 正面關鍵字
        if (lowerName.Contains("front") || lowerName.Contains("selfie") || lowerName.Contains("secondary"))
            return true;

        // 如果包含 "back", "rear" 則肯定不是前鏡頭
        if (lowerName.Contains("back") || lowerName.Contains("rear") || lowerName.Contains("main"))
            return false;

        return false;
    }

    public void SwitchCamera()
    {
        if (devices == null || devices.Length < 1) return;

        // 算出下一個索引，並取餘數 (Loop)
        int nextIndex = (currentDeviceIndex + 1) % devices.Length;

        StartCameraByIndex(nextIndex);
    }

    public void StopCamera()
    {
        if (CurrentWebCam != null)
        {
            CurrentWebCam.Stop();
            Destroy(CurrentWebCam);
            CurrentWebCam = null;
        }
    }

    private IEnumerator WaitForCameraReady()
    {
        while (CurrentWebCam != null && CurrentWebCam.width <= 16)
        {
            yield return null;
        }
        if (CurrentWebCam != null) UpdatePreviewOrientation();
    }

    private void UpdatePreviewOrientation()
    {
        if (CurrentWebCam == null) return;

        lastVideoRotationAngle = CurrentWebCam.videoRotationAngle;
        int rotation = -CurrentWebCam.videoRotationAngle;

        if (displayImage != null)
            displayImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);

        if (ratioFitter != null)
        {
            float videoRatio = (float)CurrentWebCam.width / (float)CurrentWebCam.height;
            if (Mathf.Abs(rotation) == 90 || Mathf.Abs(rotation) == 270)
            {
                videoRatio = (float)CurrentWebCam.height / (float)CurrentWebCam.width;
            }
            ratioFitter.aspectRatio = videoRatio;
        }

        if (displayImage != null)
        {
            // 使用我們推斷出來的 isCurrentFrontFacing 來決定鏡像
            float scaleX = isCurrentFrontFacing ? -1f : 1f;
            float scaleY = CurrentWebCam.videoVerticallyMirrored ? -1f : 1f;
            displayImage.rectTransform.localScale = new Vector3(scaleX, scaleY, 1);
        }
    }

    // CapturePhoto 邏輯同上一版，但需傳入 isCurrentFrontFacing
    [ContextMenu("Capture Photo")]
    public void CapturePhoto()
    {
        if (CurrentWebCam == null || !CurrentWebCam.isPlaying) return;
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        int rotationAngle = CurrentWebCam.videoRotationAngle;
        bool isRotated = Mathf.Abs(rotationAngle) == 90 || Mathf.Abs(rotationAngle) == 270;

        int finalWidth = isRotated ? CurrentWebCam.height : CurrentWebCam.width;
        int finalHeight = isRotated ? CurrentWebCam.width : CurrentWebCam.height;

        RenderTexture rt = RenderTexture.GetTemporary(finalWidth, finalHeight, 0, RenderTextureFormat.ARGB32);

        // 傳入 isCurrentFrontFacing
        RotateAndMirrorTexture(CurrentWebCam, rt, rotationAngle, isCurrentFrontFacing, CurrentWebCam.videoVerticallyMirrored);

        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D photo = new Texture2D(finalWidth, finalHeight, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, finalWidth, finalHeight), 0, 0);
        photo.Apply();

        RenderTexture.active = currentActiveRT;
        RenderTexture.ReleaseTemporary(rt);

        Debug.Log($"拍照完成: {photo.width}x{photo.height}");
        OnPhotoCaptured?.Invoke(photo);
    }

    private void RotateAndMirrorTexture(Texture source, RenderTexture destination, int angle, bool mirrorHorizontal, bool mirrorVertical)
    {
        Graphics.SetRenderTarget(destination);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, destination.width, destination.height, 0);
        float x = destination.width / 2f;
        float y = destination.height / 2f;
        GL.Translate(new Vector3(x, y, 0));
        GL.Rotate(-angle);
        float sx = mirrorHorizontal ? -1f : 1f;
        float sy = mirrorVertical ? -1f : 1f;
        GL.Scale(new Vector3(sx, sy, 1));
        float drawW = source.width;
        float drawH = source.height;
        Rect drawRect = new Rect(-drawW / 2f, -drawH / 2f, drawW, drawH);
        Graphics.DrawTexture(drawRect, source);
        GL.PopMatrix();
    }
}