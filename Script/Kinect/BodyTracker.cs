using com.rfilkov.components;
using com.rfilkov.kinect;
using UnityEngine;

public class BodyTracker : MonoBehaviour
{
    public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;
    public int sensorIndex; // 感測器索引
    public int playerIndex; // 玩家索引
    public float smoothFactor = 10; // 平滑因子
    public float maxScaleMagnif = 1; // 縮放因子
    public bool isScale;

    private Vector2 cursorPos = Vector2.zero;
    private GameObject obj;
    private RectTransform parentRT;
    private RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        parentRT = transform.parent.GetComponent<RectTransform>();

        if (transform.childCount > 0)
            obj = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        var kinectManager = KinectManager.Instance;
        if (kinectManager && kinectManager.IsInitialized())
        {
            if (kinectManager.IsUserDetected(playerIndex))
            {
                if (obj) obj.SetActive(true);

                var userId = kinectManager.GetUserIdByIndex(playerIndex);

                var bodyId = kinectManager.GetSensorBodyId(sensorIndex, userId);
                var bodyIndex = kinectManager.GetSensorBodyIndex(sensorIndex, bodyId);

                if (kinectManager.IsSensorJointTracked(sensorIndex, bodyIndex, (int)trackedJoint))
                {
                    var posJointRaw = kinectManager.GetSensorJointKinectPosition(sensorIndex, bodyIndex, (int)trackedJoint, false);

                    if (posJointRaw != Vector3.zero)
                    {
                        // 隨距離縮放物件
                        if (isScale)
                            rt.localScale = Vector3.one * (maxScaleMagnif / posJointRaw.z);

                        var posDepth = kinectManager.MapSpacePointToDepthCoords(sensorIndex, posJointRaw);
                        var depthValue = kinectManager.GetDepthForPixel(sensorIndex, (int)posDepth.x, (int)posDepth.y);

                        var posColor = Vector2.zero;
                        if (posDepth != Vector2.zero && depthValue > 0)
                        {
                            // depth pos to color pos
                            posColor = kinectManager.MapDepthPointToColorCoords(sensorIndex, posDepth, depthValue);
                        }
                        else
                        {
                            // workaround - try to use the color camera space, if depth is not available
                            var sensorData = kinectManager.GetSensorData(sensorIndex);
                            if (sensorData != null && sensorData.depthCamIntr == null &&
                                sensorData.colorCamIntr != null)
                                posColor = kinectManager.MapSpacePointToColorCoords(sensorIndex, posJointRaw);
                        }

                        if (posColor.x != 0f && !float.IsInfinity(posColor.x))
                        {
                            var sensorData = kinectManager.GetSensorData(sensorIndex);

                            // get the color image x-offset and width (use the portrait background, if available)
                            float colorWidth = sensorData.colorImageWidth;
                            var colorOfsX = 0f;

                            var portraitBack = PortraitBackground.Instance;
                            if (portraitBack && portraitBack.enabled)
                            {
                                colorWidth = portraitBack.GetColorScaledScreenWidth();
                                colorOfsX = (sensorData.colorImageWidth - colorWidth) / 2f;
                            }

                            var xScaled = (posColor.x - colorOfsX) / colorWidth;
                            var yScaled = posColor.y / sensorData.colorImageHeight;

                            var xScreen = sensorData.colorImageScale.x > 0f ? xScaled : 1f - xScaled;
                            var yScreen = sensorData.colorImageScale.y > 0f ? yScaled : 1f - yScaled;

                            cursorPos = Vector2.Lerp(cursorPos, new Vector2(xScreen, yScreen),
                                smoothFactor * Time.deltaTime);
                            //Debug.Log("CursorPos: " + new Vector2(xScreen, yScreen));

                            rt.anchoredPosition = new Vector2(cursorPos.x * parentRT.rect.width,
                                cursorPos.y * parentRT.rect.height);
                        }
                    }
                }
            }
            else
            {
                if (obj) obj.SetActive(false);
            }
        }
    }
}