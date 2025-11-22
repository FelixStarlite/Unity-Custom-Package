using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MouseAutoHide 类用于实现鼠标自动隐藏功能。
/// 用户可以设置一个延迟时间，当鼠标长时间未移动时，自动隐藏鼠标光标，同時將鼠標移動到指定位置，例如螢幕外。
/// L鍵可以開關鼠標移動功能，避免進行其他行為時受到干擾。
/// 另外設定一個Esc用來關閉遊戲。
/// </summary>
public class MouseAutoHide : MonoBehaviour
{
    public float delay = 3; // 鼠標靜止不動多久自動隱藏鼠標
    public float movementThreshold = 0.2f;  // 至少要移動多少距離才會顯示鼠標
    public Vector2 hidePosition = new Vector2(-100, -100);  // 指定鼠標移動的座標(左下角為0,0)

    private float time;
    private bool isLockedVisible = true; // 是否鎖定鼠標

    private void Start()
    {
        time = delay;
        SetCursorVisibility(true);
    }

    private void Update()
    {
        if (Mouse.current != null)
        {
            // 從新的輸入系統取得滑鼠資料
            Vector2 delta = Mouse.current.delta.ReadValue();

            // 使用 sqrMagnitude 判斷移動幅度是否超過門檻，比單純檢查 != 0 更穩定
            if (delta.sqrMagnitude > movementThreshold * movementThreshold)
            {
                time = delay; // 重置計時器
                SetCursorVisibility(true); // 有移動就顯示
            }
        }

        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            SetCursorVisibility(false);

            if (isLockedVisible)
                MoveCursorTo(hidePosition);
        }

        // 開關強制移動鼠標功能
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
            isLockedVisible = !isLockedVisible;

        // 附加功能，離開程式
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();
    }

    /// <summary>
    /// 僅在狀態確實改變時才呼叫 API
    /// </summary>
    private void SetCursorVisibility(bool isVisible)
    {
        if (Cursor.visible != isVisible)
        {
            Cursor.visible = isVisible;
        }
    }

    /// <summary>
    /// 將鼠標移動到指定的螢幕座標 (例如：按鈕的位置)
    /// </summary>
    /// <param name="screenPosition">螢幕座標 (x, y)</param>
    private void MoveCursorTo(Vector2 screenPosition)
    {
        if (Mouse.current != null)
        {
            Mouse.current.WarpCursorPosition(screenPosition);
        }
    }
}