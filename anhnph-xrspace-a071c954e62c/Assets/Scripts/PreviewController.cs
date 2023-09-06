using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PreviewController : MonoBehaviour
{
    //UI
    public Image image;

    //private Action<string> _actionLinkCallback;

    private Panorama _panorama;

    public void SetInfo(Panorama panorama)
    {
        _panorama = panorama;
        Davinci data = Davinci.get();
        data.load(_panorama.thumbnail)
            .into(image)
            .setCached(true)
            .withLoadedAction(() =>
            {
                try
                {
                    float width = data.rawTexture2D.width;
                    float height = data.rawTexture2D.height;

                    var rectTransform = image.gameObject.GetComponent<RectTransform>();
                    float scaleX = rectTransform.sizeDelta.x / width;
                    float scaleY = rectTransform.sizeDelta.y / height;
                    float scale = scaleX > scaleY ? scaleY : scaleX;

                    Vector2 size = new Vector2();
                    size.x = width * scale;
                    size.y = height * scale;
                    rectTransform.sizeDelta = size;
                    transform.parent.gameObject.SetActive(true);

                    //image.gameObject.GetComponent<Button>().onClick.AddListener(() => {
                    //    OnActionClick(_panorama.objectId);
                    //});
                } catch(Exception ex)
                {

                }
            })
            .start();
    }

    //private void OnActionClick(string actionLink)
    //{
    //    if (_actionLinkCallback != null)
    //    {
    //        _actionLinkCallback(actionLink);
    //    }
    //}

    //public void AddActionLinkListener(Action<string> callback)
    //{
    //    _actionLinkCallback = callback;
    //}

}
