using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public Vector2 resolution;
    public bool isAwakePlay = false;

    private VideoPlayer videoPlayer;
    private RawImage rawImage;

    // Start is called before the first frame update
    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        rawImage = GetComponent<RawImage>();

        var renderTexture = new CustomRenderTexture((int)resolution.x, (int)resolution.y);
        renderTexture.initializationColor = new Color(0f, 0f, 0f, 0f);
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        if (isAwakePlay)
        {
            videoPlayer.Play();
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}