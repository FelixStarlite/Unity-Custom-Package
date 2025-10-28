using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理鏡頭裝置的顯示和控制，支援PC和行動裝置
/// 用法:
/// 1. 在RawImage物件中增加AspectRatioFitter元件
/// 2. 設定RawImage和AspectRatioFitter組件
/// </summary>
public class WebCamDevices : MonoBehaviour
{
    /// <summary>
    /// 獲取或設置網路攝影機紋理
    /// </summary>
    public WebCamTexture WebCamTexture
    {
        get => webCamTexture;
        private set => webCamTexture = value;
    }

    [SerializeField] private RawImage displayImage; // 顯示鏡頭畫面
    [SerializeField] private AspectRatioFitter ratioFitter; // 調整顯示比例
    [SerializeField] private bool isAutoPlay = true; // 是否自動打開鏡頭
    [SerializeField] private bool isInfo; // 是否顯示鏡頭資訊
    [SerializeField] private bool isFront; // 預設打開後鏡頭
    [Header("使用指定的前後鏡頭"), Tooltip("無法取得前後鏡頭時可用")]
    [SerializeField] private bool useCustomFrontBack;
    [SerializeField] private string frontDeviceName;
    [SerializeField] private string backDeviceName;

    private const int UninitializedTextureWidth = 16; // 鏡頭紋理未載入時像素為16
    private WebCamTexture webCamTexture;
    private bool isAspectRatioInitialized = false;
    private WebCamDevice? currentDevice;

    private void Awake()
    {
        InitializeWebCam();
    }

    /// <summary>
    /// 初始化網路攝影機和顯示組件
    /// </summary>
    private void InitializeWebCam()
    {
        // 如果沒有設定displayImage, 就試著從當前物件取得
        if (displayImage == null)
            displayImage = GetComponent<RawImage>();

        // 同理, 檢查 RatioFitter
        if (ratioFitter == null)
            ratioFitter = GetComponent<AspectRatioFitter>();

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("找不到任何鏡頭裝置。");
            return;
        }

        // 顯示鏡頭列表
        if (isInfo)
        {
            Debug.Log("顯示設備名單");
            int item = 1;
            foreach (var webCamDevice in devices)
            {
                Debug.Log($"{item}. {webCamDevice.name}");
            }
        }

        // 選擇裝置
        currentDevice = FindDevice(devices, isFront);
        if (!currentDevice.HasValue)
        {
            Debug.LogError("找不到指定的鏡頭裝置，將使用第一個裝置。");
            currentDevice = devices[0];
        }

        webCamTexture = new WebCamTexture(currentDevice.Value.name);
        displayImage.texture = webCamTexture;

        // 注意：uvRect 的設定移至 Update 中，因為需要等待 webCamTexture.videoVerticallyMirrored 屬性生效
        // 舊的 uvRect 設定已移除

        if (isAutoPlay)
            StartWebCam();
    }

    /// <summary>
    /// 根據 isFront 和 useCustomFrontBack 尋找裝置
    /// </summary>
    private WebCamDevice? FindDevice(WebCamDevice[] devices, bool findFront)
    {
        if (useCustomFrontBack)
        {
            string deviceName = findFront ? frontDeviceName : backDeviceName;
            return devices.Select(d => (WebCamDevice?)d).FirstOrDefault(d => d.Value.name == deviceName);
        }
        else
        {
            // 優先找 isFrontFacing 匹配的，如果找不到，就用第一個
            return devices.Select(d => (WebCamDevice?)d).FirstOrDefault(d => d.Value.isFrontFacing == findFront) ?? devices.Select(d => (WebCamDevice?)d).FirstOrDefault();
        }
    }


    private void Update()
    {
        // 確保 WebCamTexture 已經啟動且已獲取到有效的畫面 (寬度 > 16)
        if (webCamTexture == null || !webCamTexture.isPlaying || webCamTexture.width <= UninitializedTextureWidth)
        {
            return;
        }

        // 步驟 1: 初始化長寬比 (只需要執行一次)
        if (!isAspectRatioInitialized)
        {
            TryInitializeAspectRatio();
        }

        // 步驟 2: 更新畫面方向與鏡像 (每幀執行)
        UpdateCameraOrientationAndMirroring();
    }

    /// <summary>
    /// 嘗試初始化顯示比例 (已修正旋轉問題)
    /// </summary>
    private void TryInitializeAspectRatio()
    {
        if (ratioFitter == null) return;

        Debug.Log($"WebCam 原始解析度: {webCamTexture.width}x{webCamTexture.height}");

        float rotation = webCamTexture.videoRotationAngle;
        float aspectRatio;

        // 檢查畫面是否旋轉了 90 或 270 度
        // 如果是，長寬比需要反轉
        if (Mathf.Abs(rotation) == 90 || Mathf.Abs(rotation) == 270)
        {
            aspectRatio = (float)webCamTexture.height / (float)webCamTexture.width;
        }
        else
        {
            // 0 或 180 度，使用原始長寬比
            aspectRatio = (float)webCamTexture.width / (float)webCamTexture.height;
        }

        ratioFitter.aspectRatio = aspectRatio;
        isAspectRatioInitialized = true;
        Debug.Log($"設定 AspectRatio: {aspectRatio} (旋轉角度: {rotation})");
    }

    /// <summary>
    /// 校正畫面的方向與裝置的方向一致，並處理鏡像
    /// </summary>
    private void UpdateCameraOrientationAndMirroring()
    {
        if (displayImage == null) return;

        // 校正因手機旋轉造成的畫面旋轉
        int rotation = -webCamTexture.videoRotationAngle;
        displayImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);

        // webCamTexture.videoVerticallyMirrored 來處理垂直顛倒
        bool isVerticallyMirrored = webCamTexture.videoVerticallyMirrored;

        // 修正: U 座標從 1 開始, 寬度 -1, 實現 1 -> 0 的鏡像
        float uv_X = isFront ? 1f : 0f;
        float uv_W = isFront ? -1f : 1f;

        // 同理，如果垂直顛倒，V 座標從 1 開始, 高度 -1
        float uv_Y = isVerticallyMirrored ? 1f : 0f;
        float uv_H = isVerticallyMirrored ? -1f : 1f;

        // 套用修正後的 UV Rect
        displayImage.uvRect = new Rect(uv_X, uv_Y, uv_W, uv_H);
    }

    /// <summary>
    /// 切換前後鏡頭
    /// </summary>
    public void SwitchCamera()
    {
        if (WebCamTexture.devices.Length <= 1)
        {
            Debug.LogWarning("裝置只有一個鏡頭，無法切換。");
            return;
        }

        // 停止目前的鏡頭
        StopWebCam();

        // 切換目標
        isFront = !isFront;

        // 尋找新裝置
        WebCamDevice[] devices = WebCamTexture.devices;
        currentDevice = FindDevice(devices, isFront);
        if (!currentDevice.HasValue)
        {
            Debug.LogError("找不到可切換的鏡頭，將使用第一個裝置。");
            currentDevice = devices[0];
        }

        // 建立並啟動新鏡頭
        webCamTexture = new WebCamTexture(currentDevice.Value.name);
        displayImage.texture = webCamTexture;

        // 重設長寬比標記，以便下次 Update 時重新計算
        isAspectRatioInitialized = false;

        StartWebCam();
    }

    /// <summary>
    /// 打開鏡頭
    /// </summary>
    public void StartWebCam()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Play();
            isAspectRatioInitialized = false; // 每次啟動都重設
        }
    }

    /// <summary>
    /// 關閉鏡頭
    /// </summary>
    public void StopWebCam()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }

    /// <summary>
    /// 檢查當前鏡頭是否為前鏡頭
    /// </summary>
    public bool IsFrontFacing()
    {
        return currentDevice?.isFrontFacing ?? false;
    }
}