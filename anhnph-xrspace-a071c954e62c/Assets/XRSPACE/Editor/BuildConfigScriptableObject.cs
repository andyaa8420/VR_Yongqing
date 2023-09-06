using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct AvaliableDevice
{
    public bool Hand;
    public bool Controller;
    public bool Tracker;
}

[CreateAssetMenu(fileName ="XR-BuildConfig.asset", menuName ="XRSDK/Create build config", order = 1)]
public class BuildConfigScriptableObject : ScriptableObject
{
    [Tooltip("Set to false if you want to submit this apk for review. This will make app only available in Manova.")]
    public bool Exported;

    [Tooltip("Check options if your app support each of input methods. This will be reviewed when submit")]
    public AvaliableDevice InputMethod;

    public BuildConfigScriptableObject()
    {
        Exported = true;
        InputMethod.Hand = true;
        InputMethod.Controller = false;
        InputMethod.Tracker = false;
    }
}
