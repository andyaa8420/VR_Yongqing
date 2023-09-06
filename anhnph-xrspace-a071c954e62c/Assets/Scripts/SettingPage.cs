using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XRSpace.Platform.InputDevice;

public class SettingPage: MonoBehaviour
{
    public Toggle m_Toggle;
    public GameObject hint;
    public GameObject listView;
    public GameObject tutorial;
    public GameObject setting;
    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public GameObject page4;
    public GameObject page5;
    // Use this for initialization
    void Start()
    {
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(m_Toggle);
        });
        hint.SetActive(m_Toggle.isOn);
        gameObject.SetActive(false);
    }

    private float timer = 0.0f;

    private float waitTime = 1.5f;

    void Update()
    {
       if (XRInputManager.Instance.Button(XRDeviceType.HANDLER_LEFT, XRControllerButton.Trigger)
            || XRInputManager.Instance.Button(XRDeviceType.HANDLER_RIGHT, XRControllerButton.Trigger)
            && page1.activeSelf && tutorial.activeSelf)
        {
            page5.SetActive(false);
            page4.SetActive(false);
            page3.SetActive(false);
            page2.SetActive(true);
            page1.SetActive(false);
        }

        if (XRInputManager.Instance.Button(XRDeviceType.HANDLER_LEFT, XRControllerButton.Grab)
             || XRInputManager.Instance.Button(XRDeviceType.HANDLER_RIGHT, XRControllerButton.Grab) && page2.activeSelf)
        {
            page5.SetActive(false);
            page4.SetActive(false);
            page3.SetActive(true);
            page2.SetActive(false);
            page1.SetActive(false);
        }

        if (XRInputManager.Instance.Button(XRDeviceType.HANDLER_LEFT, XRControllerButton.Action)
             || XRInputManager.Instance.Button(XRDeviceType.HANDLER_RIGHT, XRControllerButton.Action) && page3.activeSelf)
        {
            page5.SetActive(false);
            page4.SetActive(true);
            page3.SetActive(false);
            page2.SetActive(false);
            page1.SetActive(false);
        }

    }

    public void OnNextPage5()
    {
        page5.SetActive(true);
        page4.SetActive(false);
        page3.SetActive(false);
        page2.SetActive(false);
        page1.SetActive(false);
    }

    public void ShowTutorial()
    {
        tutorial.SetActive(true);
        setting.SetActive(false);
        page5.SetActive(false);
        page4.SetActive(false);
        page3.SetActive(false);
        page2.SetActive(false);
        page1.SetActive(true);
    }

    public void HideTutorial()
    {
        tutorial.SetActive(false);
        setting.SetActive(true);
        page5.SetActive(false);
        page4.SetActive(false);
        page3.SetActive(false);
        page2.SetActive(false);
        page1.SetActive(true);
        gameObject.SetActive(false);
        listView.SetActive(true);
    }

    void ToggleValueChanged(Toggle change)
    {
        hint.SetActive(m_Toggle.isOn);
    }

    public void ShowSettingPage()
    {
        gameObject.SetActive(true);
        listView.SetActive(false);
    }

    public void HideSettingPage()
    {
        tutorial.SetActive(false);
        setting.SetActive(true);
        page5.SetActive(false);
        page1.SetActive(true);
        gameObject.SetActive(false);
        listView.SetActive(true);
    }
}
