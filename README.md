# Unity Custom Package 工具庫說明

本專案包含一系列自製的 Unity 通用功能腳本與插件包，涵蓋 UI 管理、硬體串接、檔案處理及系統功能等。

## 📂 Script 功能列表

### 🖥️ UI 與 視覺效果 (UI & Visuals)

| 腳本名稱 | 功能說明 | 依賴插件 |
| :--- | :--- | :--- |
| **ViewManager.cs** | UI 頁面管理器核心，負責頁面的切換、堆疊與返回邏輯。 | - |
| **ViewController.cs** | 單一 UI 頁面的控制器，定義頁面的進場/退場動畫與事件 (OnEnter/OnExit)。 | DOTween, Odin |
| **ViewAnimation.cs** | 定義 UI 動畫的資料結構 (Fade, Scale, Slide 等) 與參數設定。 | DOTween |
| **ScrollPageController.cs** | 透過 ScrollRect 實現翻頁效果的控制器，支援上一頁/下一頁按鈕。 | DOTween |
| **CanvasScale.cs** | 自動適配 Canvas 比例，處理不同解析度下的黑邊填充 (Letterbox/Pillarbox)。 | - |
| **AnimationEvent.cs** | 用於接收 Animation Clip 中的 Event 並轉發為 UnityEvent，方便在 Inspector 設置回調。 | Odin |
| **VideoController.cs** | 簡單的 VideoPlayer 控制器，將影片渲染至 RawImage。 | - |
| **SignatureDrawer.cs** | 簽名板功能，支援在 UI 上繪製線條並儲存為圖片 (PNG)。 | Input System |

### ⚙️ 系統與檔案處理 (System & IO)

| 腳本名稱 | 功能說明 | 備註 |
| :--- | :--- | :--- |
| **LogManager.cs** | 本地日誌系統，將 Debug Log 自動寫入至 StreamingAssets 的文字檔中，並支援自動初始化。 | - |
| **Screenshot.cs** | 強大的截圖工具，支援全螢幕或指定 Camera 截圖，可存至本地或上傳至伺服器。 | UniTask, Odin |
| **CaptureController.cs** | 360 度全景截圖工具，可將場景存為全景圖。 | Unity360ScreenshotCapture |
| **CSVStudio.cs** | 簡易 CSV 讀寫工具，支援泛型物件與 CSV 格式互轉。 | - |
| **StringCryptog.cs** | 字串加解密工具，使用 AES-256 對稱式加密演算法。 | - |
| **ExifData.cs** | `CompactExifLib` 庫，用於讀取或修改圖片檔案的 EXIF 資訊 (如 GPS、拍攝時間)。 | - |
| **ProcessManager.cs** | 外部程序管理，用於在 Windows 環境下啟動、關閉或監控 .exe 執行檔。 | Windows Only |
| **WindowMod.cs** | Windows視窗管理，可強制設定視窗解析度、去除邊框、多螢幕定位。 | Windows Only |
| **Program.cs** | 簡單的工具，用於顯示字串的 Unicode 編碼。 | - |

### 🔌 硬體與輸入裝置 (Hardware & Input)

| 腳本名稱 | 功能說明 | 備註 |
| :--- | :--- | :--- |
| **WebCamDevices.cs** | 跨平台 WebCam 管理，自動處理前後鏡頭切換、畫面旋轉校正與鏡像顯示。 | - |
| **PrinterManager.cs** | 印表機列印管理，支援設定紙張格式與呼叫印表機列印圖片。 | PrintLib |
| **GpsManager.cs** | 行動裝置 GPS 定位服務，提供經緯度獲取與距離計算。 | - |
| **CandleBlow1.cs** | 麥克風吹氣偵測，透過麥克風音量控制物件縮放 (模擬吹蠟燭效果)。 | - |
| **KeyBoard.cs** | 呼叫系統鍵盤，支援 Windows 螢幕小鍵盤 (osk.exe) 或行動裝置鍵盤。 | - |
| **TouchKeyboardLauncher.cs** | 專門用於呼叫 Windows 10/11 觸控鍵盤 (TabTip.exe) 的啟動器。 | Windows Only |
| **MouseAutoHide.cs** | 滑鼠閒置自動隱藏，並包含 ESC 鍵退出程式功能。 | Input System |
| **MultiTouchRaycastTemplate.cs** | 多點觸控射線檢測模板，支援新版 Input System 的觸控與滑鼠點擊偵測。 | Input System |
| **BodyTracker.cs** | Kinect 人體骨架追蹤腳本，將 Kinect 關節數據映射到 UI 或物件上。 | Kinect SDK |
| **Transform3D.cs** | 3D 物件手勢操作，支援雙指縮放與旋轉。 | FingersScript |

### 🌐 網路與 API (Network)

| 腳本名稱 | 功能說明 | 依賴插件 |
| :--- | :--- | :--- |
| **ApiService.cs** | API 請求管理器，封裝 UnityWebRequest，支援 GET/POST 與 Token 驗證 (使用 UniTask)。 | UniTask, Newtonsoft.Json |
| **PicturePost.cs** | 圖片上傳範例，將 Byte 陣列上傳至特定 API 接口。 | UniTask |

### 🛠️ 輔助工具 (Utils)

| 腳本名稱 | 功能說明 |
| :--- | :--- |
| **Timer.cs** | 通用倒數計時器，時間到觸發 UnityEvent。 |
| **ListExtensions.cs** | List 擴充方法，目前包含 Shuffle (洗牌/亂序) 功能。 |
| **TextureManipulator.cs** | Texture2D 處理工具，目前包含圖片旋轉 90 度功能。 |

---

## 📦 Unity Packages 資源包

專案中包含以下預製的 UnityPackage，可直接匯入使用：

* **COMPort.unitypackage**: 串口通訊相關功能。
* **GameSetting.unitypackage**: 遊戲設定模組。
* **RFID讀卡機(限耘碩科技).unitypackage**: 特定型號 RFID 讀卡機串接。
* **北陽雷達_v1.1.unitypackage**: 北陽 (Hokuyo) 雷達互動模組。
* **字串加密和CSV編輯.unitypackage**: 包含上述加密與 CSV 工具的整合包。
* **導覽頁.unitypackage**: 通用導覽頁面 UI 模組。
* **深度感測插件_v1.0.unitypackage**: 深度相機相關功能。
* **版本號顯示.unitypackage**: 自動顯示應用程式版本號的 UI 小工具。

---

## ⚠️ 依賴插件 (Dependencies)

本專案部分腳本依賴以下第三方插件，使用前請確保專案中已安裝：

* **Odin Inspector**: 用於增強 Inspector 顯示 (如 `[Button]`, `[TabGroup]`)。
* **DOTween**: 用於 UI 動畫與數值漸變 (`ViewAnimation`, `ScrollPageController`)。
* **UniTask**: 用於非同步處理 (`ApiService`, `Screenshot`)。
* **Newtonsoft.Json**: 用於 API JSON 解析。
* **Input System (New)**: 用於觸控與輸入偵測 (`MultiTouchRaycastTemplate`, `SignatureDrawer`)。
* **PrintLib**: 用於印表機功能 (`PrinterManager`)。
* **Kinect SDK / Unity360ScreenshotCapture**: 特定功能依賴。

---

## 📝 使用注意事項

1.  **Windows 平台**: `ProcessManager`, `WindowMod`, `KeyBoard` (osk 部分), `TouchKeyboardLauncher` 主要針對 Windows 平台設計，包含對 `user32.dll` 的調用。
2.  **初始化**: `LogManager` 會自動初始化；`ViewManager` 需要場景中配置好 `ViewController` 子物件。
3.  **API 設定**: `ApiService` 與 `PicturePost` 中的 URL 為範例或特定專案使用，請記得修改為實際網址。
