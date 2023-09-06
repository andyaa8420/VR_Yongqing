using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    private RectTransform rectComponent;
    private Image imageComp;
    private float _percent = 0;
    public float CurrentProcess;


    // Use this for initialization
    void Awake()
    {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;
    }

    void Update()
    {
        if (imageComp.fillAmount < _percent)
        {
            imageComp.fillAmount = imageComp.fillAmount + Time.deltaTime;
            CurrentProcess = imageComp.fillAmount + Time.deltaTime;
        } else
        {
            imageComp.fillAmount = _percent;
            CurrentProcess = _percent;
        }
    }

    public void SetProcess(float percent)
    {
        _percent = percent;
        if (_percent == 0)
        {
            imageComp.fillAmount = _percent;
            CurrentProcess = _percent;
        }
    }
}
