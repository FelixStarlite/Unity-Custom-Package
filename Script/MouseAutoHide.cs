using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MouseAutoHide 类用于实现鼠标自动隐藏功能。
/// 用户可以设置一个延迟时间，当鼠标长时间未移动时，自动隐藏鼠标光标。
///
/// 另外設定一個Esc用來關閉遊戲。
/// </summary>
public class MouseAutoHide : MonoBehaviour
{
    public float delay = 3;

    private float time;

    private void Start()
    {
        time = delay;
    }

    private void Update()
    {
        var hasMouse = Mouse.current != null;
        Vector2 delta = hasMouse ? Mouse.current.delta.ReadValue() : Vector2.zero;
        if (hasMouse && (delta.x != 0f || delta.y != 0f))
            time = delay;

        if (time > 0)
        {
            time -= Time.deltaTime;
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();
    }
}