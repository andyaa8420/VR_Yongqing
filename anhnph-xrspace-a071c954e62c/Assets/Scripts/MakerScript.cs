using System;
using System.Collections;
using System.Collections.Generic;
using TWV;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MakerScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Action<PointerEventData> _pointerEnterCallback;

    private Action<PointerEventData> _pointerExitCallback;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_pointerEnterCallback != null)
        {
            _pointerEnterCallback(eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_pointerExitCallback != null)
        {
            _pointerExitCallback(eventData);
        }
    }

    public void AddPointerEnterListener(Action<PointerEventData> callback)
    {
        _pointerEnterCallback = callback;
    }

    public void AddPointerExitListener(Action<PointerEventData> callback)
    {
        _pointerExitCallback = callback;
    }
}
