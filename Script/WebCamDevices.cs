using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamDevices : MonoBehaviour
{
    private RawImage rawImage;
    private WebCamTexture webcamTexture;

    private void Awake()
    {
        webcamTexture = new WebCamTexture();
        rawImage = GetComponent<RawImage>();
        rawImage.material.mainTexture = webcamTexture;

        AspectRatioFitter ratioFitter = GetComponent<AspectRatioFitter>();
        float ratio = webcamTexture.texelSize.y / webcamTexture.texelSize.x;
        ratioFitter.aspectRatio = ratio;
    }

    private void OnEnable()
    {
        webcamTexture.Play();
    }

    private void OnDisable()
    {
        webcamTexture.Stop();
    }
}