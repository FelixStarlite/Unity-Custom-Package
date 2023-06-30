using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScale : MonoBehaviour
{
    public Vector2 referenceResolution;
    public RectTransform topFill;
    public RectTransform bottomFill;
    public RectTransform leftFill;
    public RectTransform rightFill;

    private CanvasScaler canvasScaler;

    // Start is called before the first frame update
    private void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        CreateFill();

        float baseScaleWidth = Screen.width / referenceResolution.x;
        float baseScaleHeight = Screen.height / referenceResolution.y;

        if (baseScaleWidth <= baseScaleHeight)
        {
            canvasScaler.scaleFactor = 1 * baseScaleWidth;
            float canvasWidth = referenceResolution.x / Screen.width;
            float fillHeight = (Screen.height * canvasWidth - referenceResolution.y) / 2;
            topFill.sizeDelta = new Vector2(referenceResolution.x, fillHeight);
            topFill.anchorMin = new Vector2(0.5f, 1);
            topFill.anchorMax = new Vector2(0.5f, 1);
            topFill.pivot = new Vector2(0.5f, 1);
            topFill.anchoredPosition = new Vector2(0, 0);
            bottomFill.sizeDelta = new Vector2(referenceResolution.x, fillHeight);
            bottomFill.anchorMin = new Vector2(0.5f, 0);
            bottomFill.anchorMax = new Vector2(0.5f, 0);
            bottomFill.pivot = new Vector2(0.5f, 0);
            bottomFill.anchoredPosition = new Vector2(0, 0);
            leftFill.gameObject.SetActive(false);
            rightFill.gameObject.SetActive(false);
        }
        else if (baseScaleWidth >= baseScaleHeight)
        {
            canvasScaler.scaleFactor = 1 * baseScaleHeight;
            float canvasHeight = referenceResolution.y / Screen.height;
            float fillWidth = (Screen.width * canvasHeight - referenceResolution.x) / 2;
            leftFill.sizeDelta = new Vector2(fillWidth, referenceResolution.y);
            leftFill.anchorMin = new Vector2(0, 0.5f);
            leftFill.anchorMax = new Vector2(0, 0.5f);
            leftFill.pivot = new Vector2(0, 0.5f);
            leftFill.anchoredPosition = new Vector2(0, 0);
            rightFill.sizeDelta = new Vector2(fillWidth, referenceResolution.y);
            rightFill.anchorMin = new Vector2(1, 0.5f);
            rightFill.anchorMax = new Vector2(1, 0.5f);
            rightFill.pivot = new Vector2(1, 0.5f);
            rightFill.anchoredPosition = new Vector2(0, 0);
            topFill.gameObject.SetActive(false);
            bottomFill.gameObject.gameObject.SetActive(false);
        }
    }

    private void CreateFill()
    {
        topFill = new GameObject("topFill").AddComponent<RectTransform>();
        topFill.SetParent(transform, false);
        topFill.AddComponent<Image>().color = Color.black;
        bottomFill = new GameObject("bottomFill").AddComponent<RectTransform>();
        bottomFill.SetParent(transform, false);
        bottomFill.AddComponent<Image>().color = Color.black;
        leftFill = new GameObject("leftFill").AddComponent<RectTransform>();
        leftFill.SetParent(transform, false);
        leftFill.AddComponent<Image>().color = Color.black;
        rightFill = new GameObject("rightFill").AddComponent<RectTransform>();
        rightFill.SetParent(transform, false);
        rightFill.AddComponent<Image>().color = Color.black;
    }
}