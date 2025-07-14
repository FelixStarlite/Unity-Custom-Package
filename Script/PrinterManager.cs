using System;
using PrintLib;
using UnityEngine;

/// <summary>
/// 使用於Printer Plugin插件的腳本，需要Odin插件和TextureManipulator腳本輔助，
/// 
/// </summary>
public class PrinterManager : MonoBehaviour
{
    private enum PageFormat
    {
        Page5x3_5,
        Page5x7,
        Page4x6,
    }

    [SerializeField] private PageFormat pageFormat;

    private Printer printer;
    private float pageWidth;
    private float pageHeight;

    void Start()
    {
        printer = new Printer();
        printer.SelectPrinter("DP-DS620");
        Initialized();
        // printer.GetPageSize(ref pageWidth, ref pageHeight);
        // Debug.Log($"實際頁面尺寸: {pageWidth}mm x {pageHeight}mm");
        // printer.SetPrinterSettings(Orientation.Portrait, pageWidth, pageHeight);
    }

    private void Initialized()
    {
        switch (pageFormat)
        {
            // 紙張大小為130x92公釐
            case PageFormat.Page5x3_5:
                pageWidth = 130f;
                pageHeight = 92f;
                break;
            case PageFormat.Page5x7:
                break;
            case PageFormat.Page4x6:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 顯示所有印表機裝置名稱
    /// </summary>
    [Sirenix.OdinInspector.Button]
    private void ShowDevicesName()
    {
        printer = new Printer();
        int count = printer.GetPrinterCount();
        for (int i = 0; i < count; i++)
        {
            Debug.Log(printer.GetPrinterName(i));
        }
    }

    /// <summary>
    /// 印出照片
    /// </summary>
    /// <param name="tex">要列印的紋理</param>
    [Sirenix.OdinInspector.Button]
    private void Printing(Texture2D tex)
    {
        // 將圖片旋轉90度
        tex = TextureManipulator.RotateTexture90Degrees(tex);
        printer.StartDocument();
        printer.SetPrintPosition(0, 0);
        // 定義圖片大小,其中一個值為0就是等比縮放
        printer.PrintTexture(tex, pageWidth, 0);
        printer.EndDocument();
        Debug.Log("列印中…");
    }
}