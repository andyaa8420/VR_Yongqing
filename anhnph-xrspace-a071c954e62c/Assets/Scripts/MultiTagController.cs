using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MultiTagController : MonoBehaviour
{
    //UI
    public Text nameLabel;

    public Text descriptionLabel;

    public Image image;

    public Button button;

    public Button exitButton;

    public MultiTagListView multiTagListView;

    private Action<string> _actionLinkCallback;

    public void SetInfo(Maker maker)
    {
        List<MultiTagInfo> multiTagInfoList = new List<MultiTagInfo>();
        foreach (MultiTagInfo multiTagInfo in maker.multitagInfo)
        {
            if (multiTagInfo.hidden)
            {
                continue;
            }

            if (multiTagInfo.mainPhoto)
            {
                nameLabel.text = multiTagInfo.name;
                descriptionLabel.text = multiTagInfo.description;

                if (!string.IsNullOrWhiteSpace(multiTagInfo.action))
                {
                    string actionName = multiTagInfo.action;
                    if (actionName.StartsWith("cus_"))
                    {
                        actionName = actionName.Substring("cus_".Length);
                    }

                    button.gameObject.SetActive(true);
                    button.transform.Find("Text").GetComponent<Text>().text = Util.CovertCamelCaseToSentence(actionName);
                    button.onClick.AddListener(() => {
                        OnActionClick(multiTagInfo.actionLink);
                    });
                }

                if (!string.IsNullOrWhiteSpace(multiTagInfo.photo))
                {
                    Davinci data = Davinci.get();
                    data.load(multiTagInfo.photo)
                        .into(image)
                        .setFadeTime(1f)
                        .setCached(true)
                        .withLoadedAction(() =>
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
                            image.gameObject.SetActive(true);
                        })
                        .start();

                    Util.MoveY(descriptionLabel.gameObject, -275f, true);
                }
            } else {
                multiTagInfoList.Add(multiTagInfo);
            }
        }

        multiTagListView.SetData(multiTagInfoList);
        multiTagListView.AddActionLinkListener(OnActionClick);
    }

    private void OnActionClick(string actionLink) {
        if (_actionLinkCallback != null)
        {
            _actionLinkCallback(actionLink);
        }
    }

    public void AddActionLinkListener(Action<string> callback)
    {
        _actionLinkCallback = callback;
    }

    public void AddExitListener(Action callback)
    {
        exitButton.onClick.AddListener(() => {
            callback();
        });
    }

}
