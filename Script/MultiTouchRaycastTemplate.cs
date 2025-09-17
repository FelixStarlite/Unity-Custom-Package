using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

/// <summary>
/// 多點觸控射線觸發器模板 - 移除專案特定依賴
/// </summary>
public class MultiTouchRaycastTemplate : MonoBehaviour
{
    [Header("射線設定")]
    [SerializeField] private float rayDistance = 1000f;
    [SerializeField] private LayerMask targetLayers = -1;

    [Header("除錯")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private Color rayColor = Color.red;
    [SerializeField] private float debugRayDuration = 1f;

    [Header("事件")]
    [SerializeField] private UnityEvent<Vector2> onTouch;
    [SerializeField] private UnityEvent<GameObject> onObjectHit;

    private Camera cam;
    private InputAction mouseClickAction;
    private bool wasMousePressed = false;

    private void Awake()
    {
        cam = Camera.main ?? FindObjectOfType<Camera>();

        if (cam == null)
        {
            Debug.LogError("找不到相機！");
        }

        // 建立滑鼠點擊動作
        mouseClickAction = new InputAction("MouseClick", InputActionType.Button);
        mouseClickAction.AddBinding("<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        // 啟用Enhanced Touch支援
        EnhancedTouchSupport.Enable();

        // 訂閱觸控事件
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += OnFingerMove;

        // 啟用滑鼠動作
        mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        // 取消訂閱事件
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= OnFingerMove;

        // 停用滑鼠動作
        mouseClickAction?.Disable();

        // 停用Enhanced Touch支援
        EnhancedTouchSupport.Disable();
    }

    private void OnDestroy()
    {
        // 釋放滑鼠動作
        mouseClickAction?.Dispose();
    }

    private void Update()
    {
        // 檢查滑鼠點擊
        CheckMouseInput();
    }

    /// <summary>
    /// 檢查滑鼠輸入
    /// </summary>
    private void CheckMouseInput()
    {
        if (mouseClickAction != null)
        {
            bool isPressed = mouseClickAction.IsPressed();

            // 檢測滑鼠按下的瞬間
            if (isPressed && !wasMousePressed)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Debug.Log($"滑鼠點擊：位置 {mousePosition}");
                HandleTouch(mousePosition);
            }

            wasMousePressed = isPressed;
        }
    }

    /// <summary>
    /// 手指按下事件
    /// </summary>
    private void OnFingerDown(Finger finger)
    {
        Vector2 touchPosition = finger.screenPosition;
        int touchId = finger.index;

        Debug.Log($"觸控點 {touchId} 開始：位置 {touchPosition}");
        HandleTouch(touchPosition);
    }

    /// <summary>
    /// 手指移動事件
    /// </summary>
    private void OnFingerMove(Finger finger)
    {
        Vector2 touchPosition = finger.screenPosition;
        
        // 可以在這裡添加拖拽射線檢測邏輯
        HandleTouch(touchPosition);
    }

    /// <summary>
    /// 處理觸碰事件
    /// </summary>
    /// <param name="screenPosition">螢幕座標位置</param>
    private void HandleTouch(Vector2 screenPosition)
    {
        if (cam == null) return;

        // 從螢幕座標轉換為射線
        Ray ray = cam.ScreenPointToRay(screenPosition);

        // 除錯顯示射線
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, rayColor, debugRayDuration);
        }

        // 觸發觸控事件
        onTouch?.Invoke(screenPosition);

        // 射線檢測
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayers))
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log($"觸控點觸發射線檢測：{hitObject.name}");

            // 觸發物件命中事件
            onObjectHit?.Invoke(hitObject);

            // 呼叫自訂處理方法，可在子類別中覆寫
            OnObjectHitCustom(hitObject, hit, screenPosition);
        }
    }

    /// <summary>
    /// 自訂物件命中處理 - 可在子類別中覆寫
    /// </summary>
    /// <param name="hitObject">命中的物件</param>
    /// <param name="hit">命中資訊</param>
    /// <param name="screenPosition">螢幕座標位置</param>
    protected virtual void OnObjectHitCustom(GameObject hitObject, RaycastHit hit, Vector2 screenPosition)
    {
        // 預設實作為空，子類別可以覆寫此方法來添加自訂邏輯
        
        // 範例：尋找特定介面或組件
        // var touchable = hitObject.GetComponent<ITouchable>();
        // if (touchable != null)
        // {
        //     touchable.OnTouch();
        // }
    }

    /// <summary>
    /// 手動觸發特定位置的射線檢測
    /// </summary>
    /// <param name="screenPosition">螢幕座標位置</param>
    public void TriggerRaycastAtPosition(Vector2 screenPosition)
    {
        HandleTouch(screenPosition);
    }

    /// <summary>
    /// 設定射線距離
    /// </summary>
    /// <param name="distance">射線距離</param>
    public void SetRayDistance(float distance)
    {
        rayDistance = distance;
    }

    /// <summary>
    /// 設定目標圖層
    /// </summary>
    /// <param name="layers">目標圖層遮罩</param>
    public void SetTargetLayers(LayerMask layers)
    {
        targetLayers = layers;
    }
}
