using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resolution : MonoBehaviour
{
    public int width, height;

    private void Start()
    {
        SetResolution();
    }

    private void SetResolution()
    {
        Screen.SetResolution(width, height, true);
    }
}