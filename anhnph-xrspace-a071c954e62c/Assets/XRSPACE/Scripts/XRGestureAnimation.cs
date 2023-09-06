using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;

public class XRGestureAnimation : MonoBehaviour
{
    public XRHandlerDeviceType Device;
    public Animator HandAnimator;
    private XRHandlerGesture _currentGesture = XRHandlerGesture.None;
    public bool GrabCube { get; set; }

    private void Start()
    {
        GrabCube = false;
        HandAnimator.keepAnimatorControllerStateOnDisable = true;
    }

    private void Update()
    {
        ProcessGestureChange(XRInputManager.Instance.Gesture((XRDeviceType)Device));
    }

    public void ProcessGestureChange(XRHandlerGesture gesture)
    {
        if (_currentGesture != gesture)
        {
            HandAnimator.SetBool("IsIndex", (_currentGesture == XRHandlerGesture.Index_Inward || _currentGesture == XRHandlerGesture.Index_Outward));
            _currentGesture = gesture;
            switch (_currentGesture)
            {
                case XRHandlerGesture.Fist_Inward:
                    if (GrabCube)
                        HandAnimator.SetInteger("ChangeGesture", 8); //grabCube
                    else
                        HandAnimator.SetInteger("ChangeGesture", (int)XRHandlerGesture.Fist_Outward);
                    break;
                case XRHandlerGesture.Index_Inward:
                    HandAnimator.SetInteger("ChangeGesture", (int)XRHandlerGesture.Index_Outward);
                    break;
                case XRHandlerGesture.Open_Inward:
                    HandAnimator.SetInteger("ChangeGesture", (int)XRHandlerGesture.Open_Outward);
                    break;
                case XRHandlerGesture.Fist_Outward:
                    if(GrabCube)
                        HandAnimator.SetInteger("ChangeGesture", 8); //grabCube
                    else
                        HandAnimator.SetInteger("ChangeGesture", (int)_currentGesture);
                    break;
                default:
                    HandAnimator.SetInteger("ChangeGesture", (int)_currentGesture);
                    break;
            }
        }
    }
}
        
    

