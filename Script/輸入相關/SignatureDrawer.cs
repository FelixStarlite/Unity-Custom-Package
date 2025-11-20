using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Starlite
{
    /// <summary>
    /// SignatureDrawer 類別允許使用者在指定的畫布上繪製簽名，
    /// 並提供清除與保存簽名的功能。
    /// 使用方式:
    /// 將專案預設的InputActionAsset "InputSystem_Actions"拖入 inputActionAsset 欄位，
    /// 並確保其中包含 "UI/Click" 動作。
    /// 在Canvas底下新增一個空物件，設定好範圍後拖入 drawingArea。
    /// </summary>
    public class SignatureDrawer : MonoBehaviour
    {
        [Header("輸入設定")]
        [SerializeField] private InputActionAsset inputActionAsset;
        [Header("繪圖設定")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform drawingArea;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private float lineWidth = 5f;
        [SerializeField] private Color lineColor = Color.black;

        [Header("按鈕")]
        [SerializeField] private Button clearButton;
        [SerializeField] private Button saveButton;
        [Space]
        [SerializeField] private bool isLocalSave;
        public UnityEvent<Texture2D> OnCaptured;

        private Texture2D texture;
        private InputAction signatureAction;
        private LineRenderer currentLine;
        private List<Vector2> currentLinePositions = new List<Vector2>();
        private List<GameObject> allLines = new List<GameObject>();
        private bool isDrawing = false;

        private void Awake()
        {
            signatureAction = inputActionAsset.FindAction("UI/Click");
        }

        private void OnEnable()
        {
            signatureAction.Enable();
        }

        private void OnDisable()
        {
            signatureAction.Disable();
        }

        void Start()
        {
            // 設定按鈕事件
            if (clearButton != null)
                clearButton.onClick.AddListener(ClearSignature);

            if (saveButton != null)
                saveButton.onClick.AddListener(SaveSignature);

            // 創建Line預製物件（如果沒有指定）
            if (linePrefab == null)
            {
                linePrefab = new GameObject("LinePrefab");
                linePrefab.AddComponent<LineRenderer>();
                linePrefab.SetActive(false);
            }
        }

        void Update()
        {
            // 檢測 Input Action 的輸入
            if (signatureAction == null) return;

            if (signatureAction.WasPressedThisFrame())
            {
                if (IsPointerOverDrawingArea())
                {
                    StartDrawing();
                }
            }
            else if (signatureAction.IsPressed() && isDrawing)
            {
                ContinueDrawing();
            }
            else if (signatureAction.WasReleasedThisFrame() && isDrawing)
            {
                EndDrawing();
            }
        }

        bool IsPointerOverDrawingArea()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea,
                Pointer.current.position.ReadValue(),
                canvas.worldCamera,
                out localPoint
            );

            return drawingArea.rect.Contains(localPoint);
        }

        void StartDrawing()
        {
            isDrawing = true;
            currentLinePositions.Clear();

            // 創建新的線條物件
            GameObject lineObj = Instantiate(linePrefab, drawingArea);
            lineObj.SetActive(true);
            allLines.Add(lineObj);

            currentLine = lineObj.GetComponent<LineRenderer>();
            if (currentLine == null)
            {
                currentLine = lineObj.AddComponent<LineRenderer>();
            }

            // 設定LineRenderer屬性
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = 3;
            currentLine.material = new Material(Shader.Find("Sprites/Default"));
            currentLine.startColor = lineColor;
            currentLine.endColor = lineColor;
            currentLine.positionCount = 0;
            currentLine.useWorldSpace = false;
            currentLine.sortingOrder = 10;

            // 添加第一個點
            AddPoint();
        }

        void ContinueDrawing()
        {
            if (currentLine != null)
            {
                AddPoint();
            }
        }

        void AddPoint()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea,
                Pointer.current.position.ReadValue(),
                canvas.worldCamera,
                out localPoint
            );

            // 避免添加重複的點
            if (currentLinePositions.Count > 0)
            {
                Vector2 lastPoint = currentLinePositions[currentLinePositions.Count - 1];
                if (Vector2.Distance(lastPoint, localPoint) < 1f)
                    return;
            }

            // 防止簽名超出範圍
            if (CheckLinePosOutOfBounds())
            {
                EndDrawing();
                return;
            }

            currentLinePositions.Add(localPoint);
            currentLine.positionCount = currentLinePositions.Count;
            currentLine.SetPosition(currentLinePositions.Count - 1, localPoint);
        }

        private bool CheckLinePosOutOfBounds()
        {
            // 獲取繪圖區域的螢幕座標
            Vector3[] corners = new Vector3[4];
            drawingArea.GetWorldCorners(corners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);

            Vector2 currentPoint = Pointer.current.position.ReadValue();

            if ((currentPoint.x < min.x) || (currentPoint.x > max.x) || (currentPoint.y < min.y) || (currentPoint.y > max.y))
            {
                return true;
            }

            return false;
        }

        private void EndDrawing()
        {
            isDrawing = false;
            currentLine = null;
        }

        public void ClearSignature()
        {
            // 刪除所有線條
            foreach (GameObject line in allLines)
            {
                Destroy(line);
            }

            allLines.Clear();
            currentLinePositions.Clear();
            currentLine = null;
            isDrawing = false;
        }

        public void SaveSignature()
        {
            StartCoroutine(CaptureSignature());
        }

        IEnumerator CaptureSignature()
        {
            // 等到畫面渲染結束
            yield return new WaitForEndOfFrame();

            // 獲取繪圖區域的螢幕座標
            Vector3[] corners = new Vector3[4];
            drawingArea.GetWorldCorners(corners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);

            int width = (int)(max.x - min.x);
            int height = (int)(max.y - min.y);

            // 清理
            if (texture != null)
                Destroy(texture);

            // 創建紋理並讀取螢幕像素
            texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(min.x, min.y, width, height), 0, 0);
            texture.Apply();

            OnCaptured?.Invoke(texture);

            if (isLocalSave)
            {
                // 保存為PNG
                byte[] bytes = texture.EncodeToPNG();
                string filename = "Signature_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                string path = Path.Combine(Application.persistentDataPath, filename);

                // 檢查目錄是否存在
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(path, bytes);

                Debug.Log("簽名已保存至: " + path);

                ShowSaveMessage(path);
            }
        }

        void ShowSaveMessage(string path)
        {
            // 這裡可以顯示一個提示訊息
            Debug.Log("簽名保存成功！路徑：" + path);
        }

        private void OnDestroy()
        {
            if (texture != null)
            {
                Destroy(texture);
                texture = null;
            }
        }
    }
}