using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 管理鏡頭裝置的顯示和控制，支援PC和行動裝置
/// 用法:
/// 1. 在RawImage物件中增加AspectRatioFitter元件
/// 2. 設定RawImage和AspectRatioFitter組件
/// </summary>
public class WebCamDevices : MonoBehaviour
{
    public static WebCamDevices Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<WebCamDevices>();
            return instance;
        }
    }
    private static WebCamDevices instance;

    public event Action<Texture2D> OnPictureTaken;

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
    [SerializeField] private bool isFront; // 預設打開後鏡頭
    [SerializeField] private bool isInfo; // 是否顯示鏡頭資訊
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
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 如果沒有設定displayImage, 就試著從當前物件取得
        if (displayImage == null)
            displayImage = GetComponent<RawImage>();

        // 同理, 檢查 RatioFitter
        if (ratioFitter == null)
            ratioFitter = GetComponent<AspectRatioFitter>();

        // 在 Awake 中建立一次 WebCamTexture 物件
        if (webCamTexture == null)
        {
            webCamTexture = new WebCamTexture();
        }

        // 並且在這裡就將它指派給 RawImage
        // 這樣 SwitchCamera 時就不需重新指派
        if (displayImage != null)
        {
            displayImage.texture = webCamTexture;
        }
    }

    private void Start()
    {
        if (isAutoPlay)
        {
            OpenCamera(isFront);
        }
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
    /// 開啟前鏡頭
    /// </summary>
    public void OpenFrontCamera()
    {
        OpenCamera(true);
    }

    /// <summary>
    /// 開啟後鏡頭
    /// </summary>
    public void OpenBackCamera()
    {
        OpenCamera(false);
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
    /// 切換前後鏡頭
    /// </summary>
    public void SwitchCamera()
    {
        if (WebCamTexture.devices.Length <= 1)
        {
            Debug.LogWarning("裝置只有一個鏡頭，無法切換。");
            return;
        }

        OpenCamera(!isFront);
    }

    /// <summary>
    /// 指定開啟某個鏡頭 (前/後)
    /// </summary>
    /// <param name="openFront">true 為前鏡頭, false 為後鏡頭</param>
    /// <summary>
    /// 指定開啟某個鏡頭 (前/後)
    /// </summary>
    /// <param name="openFront">true 為前鏡頭, false 為後鏡頭</param>
    private void OpenCamera(bool openFront)
    {
        // 1. 檢查裝置
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("找不到任何鏡頭裝置。");
            return;
        }

        // 2. 檢查是否需要動作
        // 如果 "相機正在播放" 且 "要開的鏡頭" 跟 "現在的鏡頭" 相同
        // *** 修正: 增加 webCamTexture != null 檢查 ***
        if (webCamTexture != null && webCamTexture.isPlaying && isFront == openFront)
        {
            // Debug.Log($"相機已在播放中 ({(openFront ? "Front" : "Back")})，無需動作。");
            return;
        }

        // 3. 尋找並設定新狀態
        // (注意: 我們先設定狀態，就算相機啟動失敗，狀態也是正確的)
        isFront = openFront;
        currentDevice = FindDevice(devices, isFront);
        if (!currentDevice.HasValue)
        {
            Debug.LogError($"找不到指定的 {(openFront ? "Front" : "Back")} 鏡頭，將使用第一個裝置。");
            currentDevice = devices[0];
        }

        // --- 這是解決問題的關鍵 ---

        // 4.停止並 *銷毀* 舊的 WebCamTexture
        // 這是為了強制釋放硬體資源，避免跨場景後「卡住」
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null; // 確保參照被清除
        }

        // 5.建立一個 *新的* WebCamTexture 實例
        // 並且直接在建構子 (constructor) 中指定裝置名稱
        webCamTexture = new WebCamTexture(currentDevice.Value.name);

        // 6.將 *新的* 紋理指派給 RawImage
        if (displayImage != null)
        {
            displayImage.texture = webCamTexture;
        }
        else
        {
            // 這種情況可能發生在 RegisterDisplayComponents 還沒被呼叫時
            // 但沒關係，等它被呼叫時，就會抓到這個新的 webCamTexture
            Debug.LogWarning("OpenCamera: displayImage 為 null。將在 RegisterDisplayComponents 時指派紋理。");
        }

        // 7. 啟動相機 (此方法不變)
        StartWebCam();
    }

    /// <summary>
    /// 檢查當前鏡頭是否為前鏡頭
    /// </summary>
    public bool IsFrontFacing()
    {
        return currentDevice?.isFrontFacing ?? false;
    }

    /// <summary>
    /// 供新場景呼叫，以註冊新的 UI 顯示元件
    /// </summary>
    /// <param name="newDisplayImage">新場景中的 RawImage</param>
    /// <param name="newRatioFitter">新場景中的 AspectRatioFitter</param>
    public void RegisterDisplayComponents(RawImage newDisplayImage, AspectRatioFitter newRatioFitter)
    {
        // 1. 儲存新的 UI 參照
        displayImage = newDisplayImage;
        ratioFitter = newRatioFitter;

        if (displayImage == null)
        {
            Debug.LogWarning("RegisterDisplayComponents: 傳入的 RawImage 是 null。");
            return;
        }

        if (webCamTexture == null)
        {
            // 這種情況幾乎不會發生，因為 Awake() 已經處理
            webCamTexture = new WebCamTexture();
        }

        // 2. 將 "持久的" 紋理，指派給 "新的" RawImage
        displayImage.texture = webCamTexture;

        // 3. 重設長寬比，以便 Update() 重新計算
        isAspectRatioInitialized = false;

        // 4. 如果相機已經在播放 (例如從前一個場景就開著)
        //    Update() 會自動處理畫面旋轉和鏡像
        if (webCamTexture.isPlaying)
        {
            Debug.Log("相機已在播放，重新綁定 UI。");
        }
    }

    /// <summary>
    /// 拍照並觸發 OnPictureTaken 事件
    /// </summary>
    [Sirenix.OdinInspector.Button]
    public void TakePicture()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying || webCamTexture.width <= UninitializedTextureWidth)
        {
            Debug.LogWarning("相機未準備好，無法拍照。");
            return;
        }

        // 創建一個快照，並應用與預覽畫面相同的鏡像
        Texture2D snapshot = GetMirroredSnapshot(webCamTexture);

        // 觸發事件
        OnPictureTaken?.Invoke(snapshot);
    }

    /// <summary>
    /// 獲取 WebCamTexture 的快照，並應用當前設定的鏡像
    /// 注意：此快照將與 WebCamTexture 具有相同的原始方向 (例如，手機直立時可能是 1080x1920)
    /// 它 *不會* 應用 UI 上的旋轉 (videoRotationAngle)。
    /// </summary>
    private Texture2D GetMirroredSnapshot(WebCamTexture source)
    {
        // 1. 獲取原始像素
        Color32[] originalPixels = source.GetPixels32();
        int width = source.width;
        int height = source.height;

        // 2. 準備一個新的陣列來存放處理過的像素
        Color32[] processedPixels = new Color32[originalPixels.Length];

        // 3. 檢查是否需要鏡像 (邏輯同 UpdateCameraOrientationAndMirroring)
        bool applyHorizontalMirror = isFront; // 前鏡頭水平鏡像
        bool applyVerticalMirror = source.videoVerticallyMirrored; // 裝置垂直顛倒

        // 4. 遍歷像素並應用鏡像
        //    (這段邏輯複製了 RawImage UV Rect 的行為)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 計算原始索引 (y * width + x)
                int originalIndex = (y * width) + x;

                // 根據鏡像計算目標座標
                int targetX = applyHorizontalMirror ? (width - 1 - x) : x;
                int targetY = applyVerticalMirror ? (height - 1 - y) : y;

                // 計算目標索引
                int targetIndex = (targetY * width) + targetX;

                // 將原始像素陣列中的 [originalIndex] 放到 處理後像素陣列的 [targetIndex]
                processedPixels[targetIndex] = originalPixels[originalIndex];
            }
        }

        Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        snapshot.SetPixels32(processedPixels);
        snapshot.Apply();

        return snapshot;
    }

    private void OnDestroy()
    {
        // 停止鏡頭
        StopWebCam();

        // 銷毀 WebCamTexture 物件
        if (webCamTexture != null)
        {
            // Destroy() 會釋放與其關聯的本機 (Native) 資源
            Destroy(webCamTexture);
            webCamTexture = null;
        }
    }
}