using TWV;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureWebView))]
[CanEditMultipleObjects]
[Serializable]
public class TWVEditor : Editor
{
    private const string TWV_VERSION = "1.0.0";
    public const string LINK_USER_MANUAL = "https://drive.google.com/file/d/1tfFlV1BLSd6rbM6yobXcV7cNGs7_HHns/view?usp=sharing";
    public const string LINK_ASSET_STORE_PAGE = "https://assetstore.unity.com/packages/slug/155318";
    public const string LINK_FORUM_PAGE = "https://forum.unity.com/threads/v-1-0-0-texture-web-view-android.755612/";

    SerializedProperty _outputObjectProp;
    SerializedProperty _inputCameraProp;
    SerializedProperty _eventSystemProp;

    SerializedProperty _useHtmlContentProp;
    SerializedProperty _dataProp;

    SerializedProperty _showMainOptionsProp;

    SerializedProperty _viewSizeProp;
    SerializedProperty _useScreenSizeProp;
    SerializedProperty _useObjectSizeProp;

    SerializedProperty _autoLoadProp;
    SerializedProperty _deviceKeyboardProp;

    SerializedProperty _inputSystemProp;
    SerializedProperty _debugRayProp;
    SerializedProperty _touchControllerProp;
    SerializedProperty _exclusionLayersProp;
    SerializedProperty _useGazeProp;
    SerializedProperty _gazeProp;
    SerializedProperty _gazeSubmitTimeProp;
    SerializedProperty _gazeSensitivityProp;

    SerializedProperty _showEventsOptionsProp;
    SerializedProperty _pageStartedEventProp;
    SerializedProperty _pageLoadingEventProp;
    SerializedProperty _pageFinishedEventProp;
    SerializedProperty _pageImageReadyEventProp;
    SerializedProperty _pageConsoleMessageEventProp;
    SerializedProperty _pageErrorEventProp;
    SerializedProperty _pageHttpErrorEventProp;
    SerializedProperty _motionEventsProp;
    SerializedProperty _errorEventProp;

    SerializedProperty _showAboutOptionProp;

    private bool _initCompleted;
    private Vector2 _dataAreaScroll;
    private Vector2 _eventsAreaScroll;
    private static GUIStyle _dataTextArea;
    private static GUIStyle _toggleOnButton;
    private static GUIStyle _toggleOffButton;
    private static GUIStyle _zeroMarginBox;
    private static GUIStyle _zeroPaddingBox;

    void OnEnable()
    {
        _outputObjectProp = serializedObject.FindProperty("_outputObject");
        _inputCameraProp = serializedObject.FindProperty("_inputCamera");
        _eventSystemProp = serializedObject.FindProperty("_eventSystem");

        _useHtmlContentProp = serializedObject.FindProperty("_useHtmlContent");
        _dataProp = serializedObject.FindProperty("_data");

        _showMainOptionsProp = serializedObject.FindProperty("_showMainOptions");

        _viewSizeProp = serializedObject.FindProperty("_viewSize");
        _useScreenSizeProp = serializedObject.FindProperty("_useScreenSize");
        _useObjectSizeProp = serializedObject.FindProperty("_useObjectSize");

        _autoLoadProp = serializedObject.FindProperty("_autoLoad");
        _deviceKeyboardProp = serializedObject.FindProperty("_deviceKeyboard");
        _inputSystemProp = serializedObject.FindProperty("_inputSystem");
        _debugRayProp = serializedObject.FindProperty("_debugRay");
        _touchControllerProp = serializedObject.FindProperty("_touchController");
        _exclusionLayersProp = serializedObject.FindProperty("_exclusionLayers");
        _useGazeProp = serializedObject.FindProperty("_useGaze");
        _gazeProp = serializedObject.FindProperty("_gaze");
        _gazeSubmitTimeProp = serializedObject.FindProperty("_gazeSubmitTime");
        _gazeSensitivityProp = serializedObject.FindProperty("_gazeSensitivity");

        _showEventsOptionsProp = serializedObject.FindProperty("_showEventsOptions");
        _pageStartedEventProp = serializedObject.FindProperty("_pageStartedEvent");
        _pageLoadingEventProp = serializedObject.FindProperty("_pageLoadingEvent");
        _pageFinishedEventProp = serializedObject.FindProperty("_pageFinishedEvent");
        _pageImageReadyEventProp = serializedObject.FindProperty("_pageImageReadyEvent");
        _pageConsoleMessageEventProp = serializedObject.FindProperty("_pageConsoleMessageEvent");
        _pageErrorEventProp = serializedObject.FindProperty("_pageErrorEvent");
        _pageHttpErrorEventProp = serializedObject.FindProperty("_pageHttpErrorEvent");
        _motionEventsProp = serializedObject.FindProperty("_motionEvents");
        _errorEventProp = serializedObject.FindProperty("_errorEvent");

        _showAboutOptionProp = serializedObject.FindProperty("_showAboutOption");

        _initCompleted = false;
    }

    private void InitStyles()
    {
        _dataTextArea = new GUIStyle(EditorStyles.textArea);
        _dataTextArea.wordWrap = true;
        _dataTextArea.margin = new RectOffset(0, 0, 0, 0);

        _toggleOnButton = new GUIStyle(EditorStyles.miniButtonMid);
        _toggleOnButton.normal.background = EditorStyles.miniButtonMid.active.background;
        _toggleOnButton.margin = new RectOffset(0, 0, 0, 0);

        _toggleOffButton = new GUIStyle(EditorStyles.miniButtonMid);
        _toggleOffButton.margin = new RectOffset(0, 0, 0, 0);

        _zeroMarginBox = new GUIStyle(GUI.skin.box);
        _zeroMarginBox.margin = new RectOffset(0, 0, 0, 0);

        _zeroPaddingBox = new GUIStyle(GUI.skin.box);
        _zeroPaddingBox.padding = new RectOffset(0, 0, 0, 0);
    }

    /// <summary>
    /// Create a EditorGUILayout.LabelField with auto-size width
    /// </summary>
    public void AutoSizeLabelField(string label, params GUILayoutOption[] options)
    {
        var labelSize = GUI.skin.label.CalcSize(new GUIContent(label));
        EditorGUILayout.LabelField(label, GUILayout.Width(labelSize.x));
    }

    public override void OnInspectorGUI()
    {
        if (!_initCompleted)
        {
            InitStyles();
            _initCompleted = true;
        }

        serializedObject.Update();

        var umpEditor = (TextureWebView)target;

        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;

        #region Performance Warnings
        int minSdkVersion = -1;

        minSdkVersion = (int)PlayerSettings.Android.minSdkVersion;

        if (minSdkVersion > 0 && minSdkVersion < 23)
        {
            EditorGUILayout.Space();
            ShowMessageBox(MessageType.Warning, "Don't have possibility to use hardware-accelerated canvas on some devices, becaue it's available only from API level 23, so will be used default one that can affect on CPU usage and smooth frame rate. Also, will be disable possibility to render components that depended of hardware decoding.", "Performance Warning: ");
        }
        #endregion

        EditorGUI.BeginChangeCheck();

        #region Output Field
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_outputObjectProp, new GUIContent("Output Object:", "Gameobject that have 'MeshCollider' or 'RectTransform' component, that will be used to render web view content"), true);
        #endregion

        #region Camera Field
        EditorGUILayout.PropertyField(_inputCameraProp, new GUIContent("Input Camera:", "Main camera for current scene that will be used for user input handler"), true);
        #endregion

        #region EventSystem Field
        if (umpEditor.OutputObject != null && umpEditor.OutputObject.GetComponent<RectTransform>() != null || 
            _inputSystemProp.intValue == (int)InputSystem.VR)
        {
            EditorGUILayout.PropertyField(_eventSystemProp, new GUIContent("Event System:", "The EventSystem that responsible for processing and handling events in a Unity Scene"), true);
        }
        else
        {
            _eventSystemProp.objectReferenceValue = null;
        }
        #endregion

        #region Data Field
        EditorGUILayout.Space();
        GUILayout.BeginVertical(_zeroMarginBox);
        if (GUILayout.Button(new GUIContent("Use HTML Content", "Loads the given data into this WebView component"), _useHtmlContentProp.boolValue ? _toggleOnButton : _toggleOffButton))
            _useHtmlContentProp.boolValue = !_useHtmlContentProp.boolValue;
        GUILayout.Space(-2);

        var dataHeight = _useHtmlContentProp.boolValue ? 100 : 30;

        _dataAreaScroll = EditorGUILayout.BeginScrollView(_dataAreaScroll, GUILayout.Height(dataHeight));
        _dataProp.stringValue = EditorGUILayout.TextArea(_dataProp.stringValue, _dataTextArea, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
        #endregion

        #region Main Fields
        EditorGUILayout.Space();
        if (GUILayout.Button(new GUIContent("Main Options", "Show/Hide main options"), _showMainOptionsProp.boolValue ? _toggleOnButton : _toggleOffButton))
            _showMainOptionsProp.boolValue = !_showMainOptionsProp.boolValue;

        if (_showMainOptionsProp.boolValue)
        {
            GUILayout.Space(-4);
            GUILayout.BeginVertical(_zeroPaddingBox);

            GUILayout.BeginVertical(_zeroMarginBox);

            EditorGUI.BeginDisabledGroup(_useScreenSizeProp.boolValue || _useObjectSizeProp.boolValue);
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(new GUIContent("Size:", "View size in pixels"), GUILayout.Width(115));
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(_viewSizeProp, GUIContent.none, true);
            _viewSizeProp.vector2Value = new Vector2(Mathf.Clamp((int)_viewSizeProp.vector2Value.x, 1, 4096), Mathf.Clamp((int)_viewSizeProp.vector2Value.y, 1, 4096));

            GUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Calculate Object Size", "Try to get output object size in pixels"), EditorStyles.miniButton))
            {
                var objectSize = WebViewHelper.GetPixelSizeOfObject(umpEditor.OutputObject, umpEditor.InputCamera);
                _viewSizeProp.vector2Value = new Vector2((int)objectSize.x, (int)objectSize.y);
            }

            EditorGUI.EndDisabledGroup();

            if (_useScreenSizeProp.boolValue && _useObjectSizeProp.boolValue)
            {
                _useScreenSizeProp.boolValue = false;
                _useObjectSizeProp.boolValue = false;
            }

            EditorGUI.BeginDisabledGroup(_useObjectSizeProp.boolValue);
            _useScreenSizeProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Use screen size:", "Use device screen size"), _useScreenSizeProp.boolValue);
            EditorGUI.EndDisabledGroup();

            if (!PlayerSettings.virtualRealitySupported)
            {
                EditorGUI.BeginDisabledGroup(_useScreenSizeProp.boolValue);
                _useObjectSizeProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Use object size:", "Try to automatically calculate output object size"), _useObjectSizeProp.boolValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                _useObjectSizeProp.boolValue = false;
            }
            GUILayout.EndVertical();

            _autoLoadProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Auto Load:", "Automatically load web page when current scene is start playing"), _autoLoadProp.boolValue);

            if (_inputSystemProp.intValue != (int)InputSystem.VR)
                _deviceKeyboardProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Device Keyboard:", "Show default device keyboard that allows a user to enter data that is sent to a server for processing"), _deviceKeyboardProp.boolValue);

            GUILayout.BeginVertical(_zeroPaddingBox);
            EditorGUILayout.PropertyField(_inputSystemProp, new GUIContent("Input System:", "Input system that will be used to handle user actions"), true);

            switch (_inputSystemProp.intValue)
            {
                case (int)InputSystem.VR:
                    _debugRayProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Debug Ray:", "Show debug ray (only in 'Editor' mode)"), _debugRayProp.boolValue);
                    _touchControllerProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Touch Controller:", "Use additional touch controller (useful for Oculus headset)"), _touchControllerProp.boolValue);
                    EditorGUILayout.PropertyField(_exclusionLayersProp, new GUIContent("Exclusion Layers:", "Layers to exclude from the raycast"), true);

                    var zeroMarginLeftRightBox = new GUIStyle(GUI.skin.box);
                    zeroMarginLeftRightBox.margin = new RectOffset(0, 0, GUI.skin.box.margin.top, 0);

                    GUILayout.BeginVertical(zeroMarginLeftRightBox);
                    _useGazeProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Gaze Object:", "Use special gaze object in VR environment"), _useGazeProp.boolValue);

                    if (_useGazeProp.boolValue)
                        EditorGUILayout.PropertyField(_gazeProp, new GUIContent("Object:", "Special gaze object (use speical 'Gaze' script)"), true);
                    else
                    {
                        _gazeSubmitTimeProp.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Submit Time:", "Time in seconds, to wait for admit actions"), _gazeSubmitTimeProp.floatValue), 0, 3600);
                        if (_gazeSubmitTimeProp.floatValue <= 0)
                            ShowMessageBox(MessageType.Warning, "Submit action will be ignored (can be used with additional user input system, for example 'GearVR' touchpad instead gaze focus)");
                    }

                    _gazeSensitivityProp.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Focus Sensitivity:", "Sensitivity to disable current action when change gaze position"), _gazeSensitivityProp.floatValue), 0, 1);

                    GUILayout.EndVertical();
                    break;
            }

            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }
        #endregion

        #region Events & Logging Fields
        EditorGUILayout.Space();

        if (GUILayout.Button(new GUIContent("Events", "Add/Remove events listeners"), _showEventsOptionsProp.boolValue ? _toggleOnButton : _toggleOffButton))
            _showEventsOptionsProp.boolValue = !_showEventsOptionsProp.boolValue;

        if (_showEventsOptionsProp.boolValue)
        {
            _eventsAreaScroll = EditorGUILayout.BeginScrollView(_eventsAreaScroll, GUILayout.Height(165));
            GUILayout.BeginVertical(_zeroMarginBox);

            EditorGUILayout.PropertyField(_pageStartedEventProp, true);
            EditorGUILayout.PropertyField(_pageLoadingEventProp, true);
            EditorGUILayout.PropertyField(_pageFinishedEventProp, true);
            EditorGUILayout.PropertyField(_pageImageReadyEventProp, true);
            EditorGUILayout.PropertyField(_pageConsoleMessageEventProp, true);
            EditorGUILayout.PropertyField(_pageErrorEventProp, true);
            EditorGUILayout.PropertyField(_pageHttpErrorEventProp, true);
            EditorGUILayout.PropertyField(_motionEventsProp, true);
            EditorGUILayout.PropertyField(_errorEventProp, true);

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region About
        EditorGUILayout.Space();

        if (GUILayout.Button(new GUIContent("About", "Show/Hide help options"), _showAboutOptionProp.boolValue ? _toggleOnButton : _toggleOffButton))
            _showAboutOptionProp.boolValue = !_showAboutOptionProp.boolValue;

        if (_showAboutOptionProp.boolValue)
        {
            GUILayout.Space(-4);
            GUILayout.BeginVertical(_zeroPaddingBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var icon = Resources.Load<Texture2D>("TWVIcon");
            if (icon != null)
                GUILayout.Label(new GUIContent(icon));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("TextureWebView by EasyWayAsset", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("version " + TWV_VERSION);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20f);
            EditorGUILayout.LabelField("Links", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Documentation");
            if (GUILayout.Button("User Manual", GUILayout.ExpandWidth(false)))
                Application.OpenURL(LINK_USER_MANUAL);

            GUILayout.Space(10f);
            GUILayout.Label("Rate and Review (★★★★☆)", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Unity Asset Store Page", GUILayout.ExpandWidth(false)))
                Application.OpenURL(LINK_ASSET_STORE_PAGE);

            GUILayout.Space(10f);
            GUILayout.Label("Community");
            if (GUILayout.Button("Unity Forum Page", GUILayout.ExpandWidth(false)))
                Application.OpenURL(LINK_FORUM_PAGE);

            GUILayout.EndVertical();
        }
        #endregion

        Repaint();

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

    }

    private static void ShowMessageBox(MessageType messageType, string message, string title = null)
    {
        switch (messageType)
        {
            case MessageType.None:
                GUI.color = Color.white;
                break;

            case MessageType.Info:
                GUI.color = Color.green;
                message = title == null ? "Info: " + message : title + message;
                break;

            case MessageType.Warning:
                GUI.color = Color.yellow;
                message = title == null ? "Warning: " + message : title + message;
                break;

            case MessageType.Error:
                GUI.color = Color.red;
                message = title == null ? "Error: " + message : title + message;
                break;
        }

        var textAreaStyle = EditorStyles.textArea;
        var wrap = textAreaStyle.wordWrap;

        textAreaStyle.wordWrap = true;
        EditorGUILayout.TextArea(message, textAreaStyle);
        textAreaStyle.wordWrap = wrap;

        GUI.color = Color.white;
    }
}
