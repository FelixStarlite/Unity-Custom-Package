using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBoard : MonoBehaviour
{

    //private TouchScreenKeyboard keyboard = null;

    public void OpenThing()
    {
        System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\osk.exe");
        //���}�ù���L

        //keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        //���O���}��L


    }
}
