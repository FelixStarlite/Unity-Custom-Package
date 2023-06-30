using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationMod
{
    None,
    InFade,
    OutFade,
    InScale,
    OutScale,
}

[Serializable]
public class ViewAnimation
{
    [Serializable]
    public class AnimationData
    {
        public float alpha = 0;

        public Vector3 position = Vector3.zero;

        public Vector3 scale = Vector3.one;
    }

    [OnValueChanged("ChangeValue")]
    public AnimationMod mod;

    public float delay = 0;

    [HideIf("mod", AnimationMod.None)]
    public float duration = 3;

    [HideIf("mod", AnimationMod.None), HideLabel, Title("Start")]
    public AnimationData start;

    [HideIf("mod", AnimationMod.None), HideLabel, Title("End")]
    public AnimationData end;

    private void ChangeValue()
    {
        switch (mod)
        {
            case AnimationMod.InFade:
                {
                    start.alpha = 0;
                    start.position = Vector3.zero;
                    start.scale = Vector3.one;
                    end.alpha = 1;
                    end.position = Vector3.zero;
                    end.scale = Vector3.one;
                }
                break;

            case AnimationMod.OutFade:
                {
                    start.alpha = 1f;
                    start.position = Vector3.zero;
                    start.scale = Vector3.one;
                    end.alpha = 0;
                    end.position = Vector3.zero;
                    end.scale = Vector3.one;
                }
                break;

            case AnimationMod.InScale:
                {
                    start.alpha = 0;
                    start.position = Vector3.zero;
                    start.scale = new Vector3(1.2f, 1.2f, 1.2f);
                    end.alpha = 1f;
                    end.position = Vector3.zero;
                    end.scale = Vector3.one;
                }
                break;

            case AnimationMod.OutScale:
                {
                    start.alpha = 1f;
                    start.position = Vector3.zero;
                    start.scale = Vector3.one;
                    end.alpha = 0;
                    end.position = Vector3.zero;
                    end.scale = new Vector3(1.2f, 1.2f, 1.2f);
                }
                break;
        }
    }
}