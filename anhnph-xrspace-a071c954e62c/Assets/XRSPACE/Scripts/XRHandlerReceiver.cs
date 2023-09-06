using UnityEngine;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;

public class XRHandlerReceiver : MonoBehaviour
{
    public XRHandlerDeviceType Device;
    public HandlerMode HandlerMode = HandlerMode.Auto;
    public bool EnableHandRotation = true;
    private XRHandlerData _currentData;
    private XRHandlerType _currentHandlerType = XRHandlerType.None;
    private Quaternion _recenterRotation = Quaternion.identity;
    private Quaternion _defaultRotateR = new Quaternion(-0.41f, 0.41f, 0.58f, 0.58f);
    private Quaternion _defaultRotateL = new Quaternion(-0.41f, -0.41f, -0.58f, 0.58f);

    private void Start()
    {
        _currentData = XRInputManager.Instance.GetInputData<XRHandlerData>((XRDeviceType)Device);
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentData == null)
            return;

        ProcessHandlerType();
        ProcessTransform();

        //recenter
        if (XRInputManager.Instance.ButtonDown((XRDeviceType)Device, XRControllerButton.Action))
            XRInputManager.Instance.Recenter((XRDeviceType)Device);
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
                if (_currentData.HandlerType == XRHandlerType.Hand || _currentData.HandlerType == XRHandlerType.Controller6Dof || Application.isEditor)
                    _currentHandlerType = XRHandlerType.Hand;
                else
                    _currentHandlerType = XRHandlerType.None;
                break;
        }
    }
    //Process handler position and rotation
    private void ProcessTransform()
    {
        //if hand is not tracked, maintain last position
        if(_currentHandlerType == XRHandlerType.Hand || _currentHandlerType == XRHandlerType.Controller6Dof)
        {
            transform.localPosition = _currentData.Position;
        }

        if (_currentHandlerType == XRHandlerType.Hand)
        {
            if(EnableHandRotation)
                transform.localRotation = _currentData.Rotation;
            else
            {
                if (Device == XRHandlerDeviceType.HANDLER_LEFT)
                    transform.localRotation = XRManager.Instance.head.localRotation * _defaultRotateL;
                if (Device == XRHandlerDeviceType.HANDLER_RIGHT)
                    transform.localRotation = XRManager.Instance.head.localRotation * _defaultRotateR;
            }
        }
        else if (!Application.isEditor || XRInputManager.Instance.EditorMode == XREditorMode.Simulator)
            transform.localRotation = _currentData.Rotation;
    }
}
