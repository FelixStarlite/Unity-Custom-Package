using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEvent : MonoBehaviour
{
    public EventInfo[] eventInfos;

    private Dictionary<string, UnityEvent> OnAnimeds = new Dictionary<string, UnityEvent>();

    private void Start()
    {
        for (int i = 0; i < eventInfos.Length; i++)
        {
            OnAnimeds.Add(eventInfos[i].id, eventInfos[i].OnAnimed);
        }
    }

    public void OnAnimed(string id)
    {
        OnAnimeds[id]?.Invoke();
    }
}

[Serializable]
public class EventInfo
{
    public string id;
    public UnityEvent OnAnimed;
}