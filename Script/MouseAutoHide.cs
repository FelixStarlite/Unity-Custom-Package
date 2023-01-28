using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAutoHide : MonoBehaviour
{
    public float delay = 3;

    private float time;

    private void Start()
    {
        time = delay;
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse X") > 0 || Input.GetAxis("Mouse Y") > 0)
            time = delay;

        if (time > 0)
        {
            time -= Time.deltaTime;
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}