using UnityEngine;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;

public class XRHandlerSwitch : MonoBehaviour
{
    public GameObject ControllerGroup;
    public GameObject HandGroup;
    public XRHandlerDeviceType Device;
    public HandlerMode HandlerMode = HandlerMode.Auto;
    private XRHandlerData _currentData;
    private XRHandlerType _currentHandlerType = XRHandlerType.None;
    private bool _systemMenuVisibilityChange = false;

    private void Start()
    {
        _currentData = XRInputManager.Instance.GetInputData<XRHandlerData>((XRDeviceType)Device);
        XREvent.OnSystemMenuVisibilityChange += XREvent_OnSystemMenuVisibilityChange;
    }

    private void OnDestroy()
    {
        XREvent.OnSystemMenuVisibilityChange -= XREvent_OnSystemMenuVisibilityChange;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentData == null)
            return;

        ProcessHandlerType();

        ProcessHandControllerVisible();
    }

    private void ProcessHandlerType()
    {
        switch (HandlerMode)
        {
            case HandlerMode.Auto:
                _currentHandlerType = _currentData.HandlerType;
                break;
            case HandlerMode.Controller_3Dof:
                if (_currentData.HandlerType == XRHandlerType.Controller3Dof || _currentData.HandlerType == XRHandlerType.Controller6Dof)
                    _currentHandlerType = XRHandlerType.Controller3Dof;
                else
                    _currentHandlerType = XRHandlerType.None;
                break;
            case HandlerMode.Controller_6Dof:
                if (_currentData.HandlerType == XRHandlerType.Controller6Dof)
                    _currentHandlerType = XRHandlerType.Controller6Dof;
                else
                    _currentHandlerType = XRHandlerType.None;
                break;
            case HandlerMode.Hand:
                if (_currentData.HandlerType == XRHandlerType.Hand || _currentData.HandlerType == XRHandlerType.Controller6Dof)
                    _currentHandlerType = XRHandlerType.Hand;
                else
                    _currentHandlerType = XRHandlerType.None;
                break;
        }
    }

    //Hand and controller visible switch.
    private void ProcessHandControllerVisible()
    {
        if(Application.isEditor && (XRInputManager.Instance.EditorMode == XREditorMode.Mouse))
        {
            HandGroup.SetActive(false);
            ControllerGroup.SetActive(true);
            return;
        }
        if (!_systemMenuVisibilityChange)
        {
            switch (_currentHandlerType)
            {
                case XRHandlerType.None:
                    if (HandGroup != null && HandGroup.activeSelf)
                        HandGroup.SetActive(false);
                    if (ControllerGroup != null && ControllerGroup.activeSelf)
                        ControllerGroup.SetActive(false);
                    break;
                case XRHandlerType.Controller3Dof:
                case XRHandlerType.Controller6Dof:
                    if (HandGroup != null && HandGroup.activeSelf)
                        HandGroup.SetActive(false);
                    if (ControllerGroup != null && !ControllerGroup.activeSelf)
                        ControllerGroup.SetActive(true);
                    break;
                case XRHandlerType.Hand:
                    if (HandGroup != null)
                    {
                        ////If hand is too close to camera, hide it.
                        if (Vector3.Distance(XRManager.Instance.head.localPosition, _currentData.Position) < 0.1f)
                            HandGroup.SetActive(false);
                        else
                            HandGroup.SetActive(true);
                    }
                    if (ControllerGroup != null && ControllerGroup.activeSelf)
                        ControllerGroup.SetActive(false);
                    break;
                default:
                    XRLogger.Warn(gameObject.name + " receiver have unprocess or undefine HandType: " + _currentHandlerType);
                    break;
            }
        }
    }

    private void XREvent_OnSystemMenuVisibilityChange(bool change)
    {
        _systemMenuVisibilityChange = change;
        if (change)
        {
            HandGroup.SetActive(false);
            ControllerGroup.SetActive(false);
        }
    }
}
