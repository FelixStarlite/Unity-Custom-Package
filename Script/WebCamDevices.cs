using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理鏡頭裝置的顯示和控制，支援PC和行動裝置
/// 用法:將該腳本拉到RawImage組件下
/// </summary>
public class WebCamDevices : MonoBehaviour
{
    /// <summary>
    /// 获取或设置网络摄像头纹理
    /// </summary>
    public WebCamTexture WebCamTexture
    {
        get => _webCamTexture;
        private set => _webCamTexture = value;
    }

    [SerializeField] private bool isFront = false;
    private const int UNINITIALIZED_TEXTURE_WIDTH = 16; // 鏡頭紋理未載入時像素為16
    private RawImage _displayImage;
    private WebCamTexture _webCamTexture;
    private bool _isAspectRatioInitialized = false;
    private WebCamDevice? _currentDevice;

    private void OnEnable()
    {
        _webCamTexture.Play();
    }

    private void OnDisable()
    {
        _webCamTexture.Stop();
    }

    private void Awake()
    {
        InitializeWebCam();
    }

    /// <summary>
    /// 初始化网络摄像头和显示组件
    /// </summary>
    private void InitializeWebCam()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            _currentDevice = devices.Select(d => (WebCamDevice?)d).FirstOrDefault(d => d.Value.isFrontFacing == isFront) ?? devices[0];
            _webCamTexture = new WebCamTexture(_currentDevice.Value.name);
        }
        else
        {
            _webCamTexture = new WebCamTexture();
        }

        _displayImage = GetComponent<RawImage>();
        _displayImage.texture = _webCamTexture;
        _displayImage.rectTransform.localEulerAngles = Vector3.zero;

        Vector3 scale = _displayImage.rectTransform.localScale;
        scale.y = isFront ? 1f : -1f;
        _displayImage.rectTransform.localScale = scale;
    }

    private void Update()
    {
        if (!_isAspectRatioInitialized)
        {
            TryInitializeAspectRatio();
        }

        UpdateCameraOrientation();
    }

    /// <summary>
    /// 尝试初始化显示比例
    /// </summary>
    private void TryInitializeAspectRatio()
    {
        if (_webCamTexture.width <= UNINITIALIZED_TEXTURE_WIDTH) return;

        Debug.Log($"{_webCamTexture.width}x{_webCamTexture.height}");

        var ratioFitter = GetComponent<AspectRatioFitter>();
        var aspectRatio = _webCamTexture.texelSize.y / _webCamTexture.texelSize.x;
        ratioFitter.aspectRatio = aspectRatio;

        _isAspectRatioInitialized = true;
    }

    /// <summary>
    /// 校正畫面的方向與裝置的方向一致
    /// </summary>
    private void UpdateCameraOrientation()
    {
        if (_webCamTexture == null || !_webCamTexture.isPlaying) return;

        int rotation = -_webCamTexture.videoRotationAngle;
        _displayImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);
    }

    /// <summary>
    /// 切換前後鏡頭
    /// </summary>
    public void SwitchCamera()
    {
        if (!HasMultipleCameras()) return;

        _webCamTexture.Stop();

        isFront = !isFront;
        WebCamDevice[] devices = WebCamTexture.devices;
        _currentDevice = _currentDevice.HasValue ? devices.FirstOrDefault(d => d.isFrontFacing == isFront) : devices[0];

        Vector3 scale = _displayImage.rectTransform.localScale;
        scale.y = isFront ? 1f : -1f;
        _displayImage.rectTransform.localScale = scale;

        if (_currentDevice.HasValue)
        {
            _webCamTexture = new WebCamTexture(_currentDevice.Value.name);
            _displayImage.texture = _webCamTexture;
            _isAspectRatioInitialized = false;
            _webCamTexture.Play();
        }
    }

    /// <summary>
    /// 檢查裝置是否有多個鏡頭
    /// </summary>
    public bool HasMultipleCameras()
    {
        return WebCamTexture.devices.Length > 1;
    }

    /// <summary>
    /// 檢查當前鏡頭是否為前鏡頭
    /// </summary>
    public bool IsFrontFacing()
    {
        return _currentDevice?.isFrontFacing ?? false;
    }
}