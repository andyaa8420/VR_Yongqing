using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TagController : MonoBehaviour
{
    //UI
    public Text nameLabel;

    public Text descriptionLabel;

    public Image image;

    public Button button;

    public Button exitButton;

    private Action<string> _actionLinkCallback;

    public void SetInfo(Maker maker)
    {
        nameLabel.text = maker.name;
        descriptionLabel.text = maker.description;

        if (!string.IsNullOrWhiteSpace(maker.action))
        {
            string actionName = maker.action;
            if (actionName.StartsWith("cus_"))
            {
                actionName = actionName.Substring("cus_".Length);
            }

            button.gameObject.SetActive(true);
            button.transform.Find("Text").GetComponent<Text>().text = Util.CovertCamelCaseToSentence(actionName);
            button.onClick.AddListener(() => {
                OnActionClick(maker.actionLink);
            });

            Util.MoveY(descriptionLabel.gameObject, -50f, true);
        }

        if (!string.IsNullOrWhiteSpace(maker.photo))
        {
            Davinci data = Davinci.get();
            data.load(maker.photo)
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

            Util.MoveX(nameLabel.gameObject, 375f, true);
            Util.MoveX(descriptionLabel.gameObject, 375f, true);
            Util.MoveX(button.gameObject, 375f, false);
        }

    }

    private void OnActionClick(string actionLink)
    {
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
