using System;
using UnityEngine;
using XRSpace.Platform.InputDevice;

public class VibrateTracker : MonoBehaviour
{
    public void Vibrate(int deviceType)
    {
        if (Enum.IsDefined(typeof(XRDeviceType), deviceType))
            XRInputManager.Instance.Vibrate((XRDeviceType)deviceType);
    }
}
