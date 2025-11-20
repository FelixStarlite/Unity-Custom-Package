using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using UnityEngine;

//丟在隨便一個物件上,在inspector可以看到設定,設定分別是(起始點X,起始點Y,寬,高)
//如果遇到第一次執行時有邊框,關掉後第二次再執行就會沒有了

public class WindowMod : MonoBehaviour
{
    [Header("設定單一螢幕畫面解析度大小")]
    public Vector2 screenResolution = new Vector2(1920, 1080);
    [Header("設定單一螢幕是否為全螢幕")]
    public bool isFullScreen = true;
    [Header("啟用後，將強制以16:9等比縮放解析度")]
    public bool scaleTo16_9 = false;
    [Header("設定是否多螢幕或投影拼接")]
    public bool isMultiScreen = false;
    [Header("設定全部最大畫面解析度位置與長寬")]
    public Rect screenPosition;

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    // not used rigth now
    //const uint SWP_NOMOVE = 0x2;
    //const uint SWP_NOSIZE = 1;
    //const uint SWP_NOZORDER = 0x4;
    //const uint SWP_HIDEWINDOW = 0x0080;
    const uint SWP_SHOWWINDOW = 0x0040;
    const int GWL_STYLE = -16;
    const int WS_BORDER = 1;

    private void Awake()
    {
        //如果不是多畫面拼接，則使用Unity內建自動設定單一螢幕解析度
        if (!isMultiScreen)
        {
            // 檢查是否啟用等比縮放
            if (scaleTo16_9)
            {
                print("Screen Setting: Scale target resolution to fit screen (Letterbox/Pillarbox)");

                // 1. 取得你想要的目標解析度 (來自 Inspector)
                float targetWidth = screenResolution.x;
                float targetHeight = screenResolution.y;

                // 2. 取得螢幕的實際原生解析度
                float screenWidth = Screen.currentResolution.width;
                float screenHeight = Screen.currentResolution.height;

                // 3. 計算目標與螢幕的長寬比
                // (例如 1920 / 1080 = 1.777)
                float targetAspect = targetWidth / targetHeight;
                // (例如 1920 / 1200 = 1.6)
                float screenAspect = screenWidth / screenHeight;

                int newWidth;
                int newHeight;

                // 4. 比較長寬比並計算

                // Case 1: 螢幕比目標更 "窄" 或 "高" (例如 螢幕 16:10, 目標 16:9)
                // screenAspect (1.6) < targetAspect (1.777)
                // 這表示我們的 "寬度" 是限制，畫面會被上下加黑邊 (Letterbox)
                if (screenAspect < targetAspect)
                {
                    // 以 "螢幕寬度" 為基準
                    newWidth = (int)screenWidth;
                    // 根據目標長寬比，計算應有的高度
                    newHeight = Mathf.RoundToInt(newWidth / targetAspect);
                }

                // Case 2: 螢幕比目標更 "寬" (例如 螢幕 21:9, 目標 16:9)
                // 或是長寬比剛好相同
                // screenAspect (2.33) >= targetAspect (1.777)
                // 這表示我們的 "高度" 是限制，畫面會被左右加黑邊 (Pillarbox)
                else // (screenAspect >= targetAspect)
                {
                    // 以 "螢幕高度" 為基準
                    newHeight = (int)screenHeight;
                    // 根據目標長寬比，計算應有的寬度
                    newWidth = Mathf.RoundToInt(newHeight * targetAspect);
                }

                print($"Target Aspect: {targetAspect}, Screen Aspect: {screenAspect}");
                print($"Target Res: {targetWidth}x{targetHeight}, Screen Res: {screenWidth}x{screenHeight}, Final Res: {newWidth}x{newHeight}");
                Screen.SetResolution(newWidth, newHeight, isFullScreen);
            }
            else
            {
                // 如果沒有啟用等比縮放，就使用 Inspector 上的固定解析度
                print("Screen Setting: Use default fixed resolution");
                Screen.SetResolution((int)screenResolution.x, (int)screenResolution.y, isFullScreen);
            }
        }
    }

    void Start()
    {
#if UNITY_EDITOR

#else
        if(isMultiScreen)//如果是多畫面拼接，則使用WindowMod模式設定(需PlayerSetting->DisplayResolutionDialog選擇Enable發布後，開啟執行檔後打勾windowed，則自動會跳轉windowMod設定的解析度,再DisplayResolutionDialog選擇Disable重新發佈，即可正常顯示windowMod模式)
        {
            print("Multi Screen Setting for WindowMod");
            SetWindowLong(GetForegroundWindow (), GWL_STYLE, WS_BORDER);
		    bool result = SetWindowPos (GetForegroundWindow (), 0,(int)screenPosition.x,(int)screenPosition.y, (int)screenPosition.width,(int) screenPosition.height, SWP_SHOWWINDOW);
        }
#endif
    }
}