using DigitalRubyShared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Transform3D : MonoBehaviour
{
    public Vector2 MinMaxScale = new Vector2(0.75f, 2f);
    public PanGestureRecognizer PanGesture { get; private set; }
    public ScaleGestureRecognizer ScaleGesture { get; private set; }
    public bool isRotate = true;
    public bool isScale = true;
    public bool isHorizontal { get; set; }

    [Tooltip("The threshold in units touches must move apart or together to begin scaling.")]
    [Range(0.0f, 1.0f)]
    public float ScaleThresholdUnits = 0.15f;

    private bool isAutoRotate = true;
    private Vector3? startScale;
    private Quaternion originRotation;

    // Start is called before the first frame update
    private void Awake()
    {
        startScale = transform.localScale;
        originRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        PanGesture = new PanGestureRecognizer();
        PanGesture.StateUpdated += PanGestureUpdated;
        ScaleGesture = new ScaleGestureRecognizer();
        ScaleGesture.ThresholdUnits = ScaleThresholdUnits;
        ScaleGesture.StateUpdated += ScaleGestureUpdated;

        FingersScript.Instance.AddGesture(PanGesture);
        FingersScript.Instance.AddGesture(ScaleGesture);
    }

    private void OnDisable()
    {
        if (FingersScript.Instance)
        {
            FingersScript.Instance.RemoveGesture(PanGesture);
            FingersScript.Instance.RemoveGesture(ScaleGesture);
        }

        transform.localRotation = originRotation;
        transform.localScale = startScale.Value;
        isAutoRotate = true;
    }

    private void Start()
    {
        FingersScript.Instance.ShowTouches = false;
    }

    private void ScaleGestureUpdated(GestureRecognizer gesture)
    {
        if (!isScale)
        {
            gesture.Reset();
            return;
        }

        if (gesture.State == GestureRecognizerState.Executing)
        {
            // assume uniform scale
            Vector3 scale = new Vector3
            (
                (transform.localScale.x * ScaleGesture.ScaleMultiplier),
                (transform.localScale.y * ScaleGesture.ScaleMultiplier),
                (transform.localScale.z * ScaleGesture.ScaleMultiplier)
            );
            if (MinMaxScale.x > 0.0f || MinMaxScale.y > 0.0f)
            {
                float minValue = Mathf.Min(MinMaxScale.x, MinMaxScale.y);
                float maxValue = Mathf.Max(MinMaxScale.x, MinMaxScale.y);
                scale.x = Mathf.Clamp(scale.x, startScale.Value.x * minValue, startScale.Value.x * maxValue);
                scale.y = Mathf.Clamp(scale.y, startScale.Value.y * minValue, startScale.Value.y * maxValue);
                scale.z = Mathf.Clamp(scale.z, startScale.Value.z * minValue, startScale.Value.z * maxValue);
            }
            transform.localScale = scale;
        }
    }

    private void PanGestureUpdated(GestureRecognizer panGesture)
    {
        if (!isRotate)
        {
            panGesture.Reset();
            return;
        }

        if (panGesture.State == GestureRecognizerState.Executing)
        {
            if (isHorizontal)
            {
                transform.Rotate(0, -panGesture.DeltaX * 0.3f, 0);
            }
            else
            {
                transform.Rotate(0, -panGesture.DeltaX * 0.3f, 0);
                transform.Rotate(panGesture.DeltaY * 0.3f, 0, 0, Space.World);
            }

            isAutoRotate = false;
        }
    }

    public void ToggleHorizontal()
    {
        isHorizontal = !isHorizontal;
    }

    public void ResetRotate()
    {
        transform.localRotation = originRotation;
        transform.localScale = startScale.Value;
    }

    // Update is called once per frame
    private void Update()
    {
        //if (isAutoRotate)
        //    transform.Rotate(0, Time.deltaTime * 20, 0);
    }
}