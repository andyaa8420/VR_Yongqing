using UnityEngine;
using UnityEngine.UI;
using PolyAndCode.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class MultiTagDataCell : MonoBehaviour, ICell
{
    //UI
    public Text nameLabel;

    public Text descriptionLabel;

    public Text priceLabel;

    public Image image;

    public Button button;

    private Action<string> _actionLinkCallback;

    //Model
    private MultiTagInfo _dataInfo;

    private void Start()
    {
        //Can also be done in the inspector
        //GetComponent<Button>().onClick.AddListener(ButtonListener);
    }

    //This is called from the SetCell method in DataSource
    public void ConfigureCell(MultiTagInfo dataInfo)
    {
        _dataInfo = dataInfo;

        nameLabel.text = dataInfo.name;
        descriptionLabel.text = dataInfo.description;


        if (!string.IsNullOrWhiteSpace(dataInfo.action))
        {
            string actionName = dataInfo.action;
            if (actionName.StartsWith("cus_"))
            {
                actionName = actionName.Substring("cus_".Length);
            }

            button.gameObject.SetActive(true);
            button.transform.Find("Text").GetComponent<Text>().text = Util.CovertCamelCaseToSentence(actionName);
            button.onClick.AddListener(() => {
                OnActionClick(dataInfo.actionLink);
            });

            priceLabel.text = dataInfo.price;
        }

        if (!string.IsNullOrWhiteSpace(dataInfo.photo))
        {
            Davinci data = Davinci.get();
            data.load(dataInfo.photo)
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

            Util.MoveX(nameLabel.gameObject, 300f, true);
            Util.MoveX(descriptionLabel.gameObject, 300f, true);
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


}
