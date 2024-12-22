using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using UnityEngine;

//丟在隨便一個物件上,在inspector可以看到設定,設定分別是(起始點X,起始點Y,寬,高)
//如果遇到第一次執行時有邊框,關掉後第二次再執行就會沒有了

public class WindowMod: MonoBehaviour
{
    [Header("設定單一螢幕畫面解析度大小")]
    public Vector2 screenResolution = new Vector2(1920,1080);
    [Header("設定單一螢幕是否為全螢幕")]
    public bool isFullScreen = true;
    [Header("設定是否多螢幕或投影拼接")]
    public bool isMultiScreen = false;
    [Header("設定全部最大畫面解析度位置與長寬")]
	public Rect screenPosition;
	[DllImport("user32.dll")]
	static extern IntPtr SetWindowLong (IntPtr hwnd,int _nIndex ,int dwNewLong);
	[DllImport("user32.dll")]
	static extern bool SetWindowPos (IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	[DllImport("user32.dll")]
	static extern IntPtr GetForegroundWindow ();
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
        if(!isMultiScreen)
        {
            print("Defalut Screen Setting for Single PC");
            Screen.SetResolution((int)screenResolution.x, (int)screenResolution.y, isFullScreen);
        }
    }

    void Start ()
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