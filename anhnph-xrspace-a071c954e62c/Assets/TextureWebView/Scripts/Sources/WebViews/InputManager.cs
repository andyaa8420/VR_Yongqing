using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TWV
{
    public enum InputSystem
    {
        Empty,
        Touch,
        VR
    }

    public enum MotionActions
    {
        Empty,
        /**
         * A pressed gesture has started, the
         * motion contains the initial starting location.
        **/
        Began,
        /**
         * A pressed gesture has finished, the
         * motion contains the final release location as well as any intermediate
         * points since the last down or move event.
        **/
        Ended,
        /** A change has happened during a
         * press gesture (between Began and Ended).
        **/
        Moved
    }

    public class InputManager
    {
        private const float DEBUG_RAY_DURATION = 0.1f;
        private const float DEFAULT_SUBMIT_TIME = 1f;
        private const float DEFAULT_GAZE_SENSITIVITY = 0.05f;

        private static float START_TIME_OFFSET = 0.000001f;
        private const float MOVE_SENSITIVITY = 15f;

        private MonoBehaviour _monoObject;
        private Camera _inputCamera;
        private EventSystem _eventSystem;
        private Transform _inputCameraTransform;

        /// <summary>
        /// Optionally show the debug ray
        /// </summary>
        private bool _debugRay;

        /// <summary>
        /// Use additional touch controller (useful for Oculus headset)
        /// </summary>
        private bool _touchController;

        /// <summary>
        /// Layers to exclude from the raycast.
        /// </summary>
        private LayerMask _exclusionLayers;

        /// <summary>
        /// Special gaze object
        /// </summary>
        private AGaze _gazeObject;

        /// <summary>
        /// Time to wait submit action
        /// </summary>
        private float _gazeSubmitTime = DEFAULT_SUBMIT_TIME;

        /// <summary>
        /// Sensitivity to disable current action when change gaze position.
        /// </summary>
        private float _gazeSensitivity = DEFAULT_GAZE_SENSITIVITY;

        /// <summary>
        /// Enable/Disable wait time before apply motion actions in VR mode.
        /// </summary>
        private bool _prepareVrMode = true;

        /// <summary>
        /// Set wait time that will be used before apply motion actions in VR mode.
        /// </summary>s
        private float _prepareVrModeTime = 2f;

        /// <summary>
        /// Add possibility to make multiple submits
        /// </summary>
        private bool _multipleSubmits;

        private float _actionWaitTime;
        private float _swipePower;
        private TargetInfo _lastInfoPosition;
        private TargetInfo _lastTargetInfo;
        private MotionData _lastInputData;
        private Vector3 _startPointVr = Vector3.one * -1f;

        private InputSystem _inputSystem;
        private IEnumerator _inputHandlerEnum;

        private class MotionData
        {
            private Vector2 _position = Vector2.one * -1;
            private MotionActions _action = MotionActions.Empty;

            public MotionData(InputSystem inputSystem, bool useTouchMotion)
            {
                if (!useTouchMotion && Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(0);
                    _position = touch.position;

                    if (touch.phase == TouchPhase.Began)
                        _action = MotionActions.Began;
                    else if (touch.phase == TouchPhase.Moved)
                        _action = MotionActions.Moved;
                    else if (touch.phase == TouchPhase.Ended ||
                        touch.phase == TouchPhase.Canceled)
                        _action = MotionActions.Ended;

                    if (inputSystem == InputSystem.VR)
                        _action = MotionActions.Empty;
                }
                else
                {
                    var isMove = (Math.Abs(Input.GetAxis("Mouse X")) > 0 ||
                        Math.Abs(Input.GetAxis("Mouse Y")) > 0);

                    if (Input.GetMouseButtonDown(0))
                        _action = MotionActions.Began;
                    else if (Input.GetMouseButton(0) && isMove)
                        _action = MotionActions.Moved;
                    else if (Input.GetMouseButtonUp(0))
                        _action = MotionActions.Ended;

                    if (_action != MotionActions.Empty)
                        _position = Input.mousePosition;
                }
            }

            public Vector2 Position
            {
                get
                {
                    return _position;
                }
                set
                {
                    _position = value;
                }
            }

            public MotionActions Action
            {
                get
                {
                    return _action;
                }
                set
                {
                    _action = value;
                }
            }

            public bool IsEmpty
            {
                get { return _action == MotionActions.Empty; }
            }

            public override string ToString()
            {
                return "Motion action: " + _action + " - (X=" + _position.x + ") : (Y=" + _position.y + ")";
            }
        }

        private class TargetInfo
        {
            private GameObject _object;
            private IInputManagerListener _inputManager;

            PointerEventData _pointerData;
            Vector2 _coords;
            Vector3 _normal;
            float _distance;

            public TargetInfo(MotionData data, Camera camera, Transform cameraTransform, EventSystem eventSystem)
            {
                object raycastResult = null;

                if (eventSystem != null)
                {
                    _pointerData = new PointerEventData(eventSystem);

                    if (!data.IsEmpty)
                        _pointerData.position = data.Position;
                    else
                        _pointerData.position = camera.WorldToScreenPoint(cameraTransform.position + cameraTransform.forward * camera.farClipPlane);

                    var raycastResults = new List<RaycastResult>();
                    eventSystem.RaycastAll(_pointerData, raycastResults);

                    if (raycastResults.Count > 0)
                        raycastResult = raycastResults[0];
                }

                Ray ray;

                if (!data.IsEmpty)
                    ray = camera.ScreenPointToRay(data.Position);
                else
                    ray = new Ray(cameraTransform.position, cameraTransform.forward);

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, camera.farClipPlane))
                {
                    if (raycastResult != null)
                    {
                        if (((RaycastResult)raycastResult).distance > hitInfo.distance)
                            raycastResult = hitInfo;
                    }
                    else
                    {
                        raycastResult = hitInfo;
                    }
                }

                if (raycastResult is RaycastResult)
                {
                    var hit = (RaycastResult)raycastResult;

                    _object = hit.gameObject;
                    _inputManager = _object.GetComponent<IInputManagerListener>();

                    var transform = (RectTransform)_object.transform;
                    var screenPoint = data.Position;
                    var coords = Vector2.zero;

                    _normal = transform.rotation * Vector3.forward;
                    _distance = hit.distance;

                    if (data.IsEmpty)
                    {
                        var worldCoords = cameraTransform.position + (cameraTransform.forward * _distance);
                        screenPoint = camera.WorldToScreenPoint(worldCoords);
                    }

                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, screenPoint, camera, out coords))
                    {
                        _coords = new Vector2((transform.rect.width * 0.5f + coords.x) / transform.rect.width,
                            (transform.rect.height * 0.5f - coords.y) / transform.rect.height);
                    }
                }
                else if(raycastResult is RaycastHit)
                {
                    var hit = (RaycastHit)raycastResult;

                    _object = hit.transform.gameObject;
                    _inputManager = _object.GetComponent<IInputManagerListener>();

                    _normal = hit.normal;
                    _distance = hit.distance;
                    _coords = new Vector2(1 - hit.textureCoord.x, hit.textureCoord.y);
                }
                else
                {
                    _object = null;
                    _inputManager = null;

                    _coords = Vector2.one * -1;
                    _normal = Vector3.one * -1;
                    _distance = -1;
                }
            }

            public GameObject Object
            {
                get { return _object; }
            }

            public Vector3 Coords
            {
                get { return _coords; }
            }

            public Vector3 Normal
            {
                get { return _normal; }
            }

            public float Distance
            {
                get { return _distance; }
            }

            public IInputManagerListener InputManager
            {
                get { return _inputManager; }
            }

            public bool IsEmpty
            {
                get
                {
                    return _object == null;
                }
            }

            public override string ToString()
            {
                return "Target info: " + (_object != null ? _object.name : "null") + " - (X=" + _coords.x + ") : (Y=" + _coords.y + ")";
            }
        }

        private bool SendUnityEventTrigger(GameObject target, EventTriggerType trigger, EventSystem eventSystem)
        {
            IEventSystemHandler eventSystemHandler = null;

            if (target == null || eventSystem == null)
                return false;

            while (target.transform.parent != null)
            {
                eventSystemHandler = target.GetComponent<IEventSystemHandler>();

                if (eventSystemHandler != null)
                    break;

                target = target.transform.parent.gameObject;
            }

            var pointerData = new PointerEventData(eventSystem);

            switch (trigger)
            {
                case EventTriggerType.PointerEnter:
                    if (eventSystemHandler is IPointerEnterHandler)
                        (eventSystemHandler as IPointerEnterHandler).OnPointerEnter(pointerData);
                    return true;

                case EventTriggerType.PointerExit:
                    if (eventSystemHandler is IPointerExitHandler)
                        (eventSystemHandler as IPointerExitHandler).OnPointerExit(pointerData);
                    return true;

                case EventTriggerType.Submit:
                    if (eventSystemHandler is ISubmitHandler)
                        (eventSystemHandler as ISubmitHandler).OnSubmit(pointerData);
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Create new instance of input manager
        /// </summary>
        /// <param name="monoObject">MonoBehaviour instanse</param>
        /// <param name="inputSystem">User input system</param>
        /// <param name="inputCamera">Camera that will be used for input manager</param>
        /// <param name="eventSystem">EventSystem that will be used for input manager</param>
        public InputManager(MonoBehaviour monoObject, InputSystem inputSystem, Camera inputCamera, EventSystem eventSystem = null)
        {
            _monoObject = monoObject;
            _inputCamera = inputCamera;
            _inputCameraTransform = _inputCamera != null ? _inputCamera.transform : null;
            _eventSystem = eventSystem;
            _inputSystem = inputSystem;
        }

        private IEnumerator InputHandler()
        {
            while (true)
            {
                if (Application.isEditor && _debugRay && _inputCameraTransform != null)
                    Debug.DrawRay(_inputCameraTransform.position, _inputCameraTransform.forward * _inputCamera.farClipPlane, Color.blue, DEBUG_RAY_DURATION);

                if (_inputSystem != InputSystem.Empty)
                {
                    var motionData = new MotionData(_inputSystem, Application.isEditor || _touchController);
                    var targetInfo = new TargetInfo(motionData, _inputCamera, _inputCameraTransform, _eventSystem);

                    if (targetInfo.IsEmpty)
                    {
                        yield return null;
                        continue;
                    }

                    if (_lastTargetInfo == null)
                        _lastTargetInfo = targetInfo;

                    // Send unity event trigger to UI component
                    if (_lastTargetInfo.Object != targetInfo.Object)
                    {
                        SendUnityEventTrigger(_lastTargetInfo.Object, EventTriggerType.PointerExit, _eventSystem);
                        SendUnityEventTrigger(targetInfo.Object, EventTriggerType.PointerEnter, _eventSystem);
                    }

                    var submitTime = _gazeSubmitTime;

                    // Show VR Gaze object
                    if (_gazeObject != null)
                    {
                        var gazePosition = _inputCameraTransform.position + (_inputCameraTransform.forward * targetInfo.Distance);
                        _gazeObject.Show(gazePosition, targetInfo.Normal);
                        submitTime = _gazeObject.SubmitTime;
                    }

                    if (_inputSystem == InputSystem.VR && !_touchController)
                    {
                        if (_startPointVr == Vector3.one * -1f)
                            _startPointVr = targetInfo.Coords;

                        var isMoved = Vector3.Distance(_startPointVr, targetInfo.Coords) > _gazeSensitivity;

                        if (isMoved)
                        {
                            _startPointVr = targetInfo.Coords;
                            _actionWaitTime = Time.time;

                            if (!_prepareVrMode)
                                _actionWaitTime += (START_TIME_OFFSET + submitTime);
                        }
                        // Check if VR mode is prepared
                        if (_prepareVrMode && _actionWaitTime > 0 && (Time.time - _actionWaitTime) > _prepareVrModeTime)
                        {
                            _prepareVrMode = false;

                            motionData.Action = MotionActions.Began;
                            _actionWaitTime = Time.time + START_TIME_OFFSET + submitTime;
                        }

                        if (!_prepareVrMode)
                        {
                            var progress = submitTime > 0 ? Mathf.Clamp01(1 - (_actionWaitTime - Time.time) / submitTime) : 0;

                            if (_gazeObject != null)
                                _gazeObject.SubmitAnimation(progress);

                            if (progress >= 1)
                            {
                                SendUnityEventTrigger(targetInfo.Object, EventTriggerType.Submit, _eventSystem);
                                motionData.Action = MotionActions.Ended;
                                _prepareVrMode = true;

                                if (_multipleSubmits)
                                    _actionWaitTime = Time.time + START_TIME_OFFSET + submitTime;
                                else
                                    _actionWaitTime = 0;
                            }
                            else if (isMoved)
                            {
                                motionData.Action = MotionActions.Moved;
                            }
                        }
                    }

                    if (!motionData.IsEmpty)
                    {
                        if (_motionEventsListener != null)
                            _motionEventsListener(targetInfo.Object, motionData.Action, targetInfo.Coords);

                        if (targetInfo.InputManager != null)
                            targetInfo.InputManager.OnMotionEvents(targetInfo.Object, motionData.Action, targetInfo.Coords);
                    }

                    _lastTargetInfo = targetInfo;
                }

                yield return null;
            }
        }

        private Vector2? GetAreaCoord(Vector2 touchPos, object inputArea)
        {
            if (inputArea is Rect)
            {
                var rectArea = (Rect)inputArea;

                if (rectArea.Contains(touchPos))
                    return new Vector2(touchPos.x / rectArea.width, 1 - (touchPos.y / rectArea.height));
            }
            else if (inputArea is MeshCollider)
            {
                var colliderArea = (MeshCollider)inputArea;

                RaycastHit hitInfo;
                if (Physics.Raycast(_inputCamera.ScreenPointToRay(touchPos), out hitInfo))
                {
                    if (hitInfo.collider == colliderArea)
                        return new Vector2(hitInfo.textureCoord.x, 1 - hitInfo.textureCoord.y);
                }
            }
            else if (inputArea is RectTransform)
            {
                var areaCoord = Vector2.zero;
                var transformArea = (RectTransform)inputArea;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transformArea, touchPos, _inputCamera, out areaCoord))
                {
                    var result = new Vector2((transformArea.rect.width * 0.5f + areaCoord.x) / transformArea.rect.width,
                        (transformArea.rect.height * 0.5f - areaCoord.y) / transformArea.rect.height);
                    if (result.x >= 0 && result.x <= 1 && result.y >= 0 && result.y <= 1)
                        return result;
                }
            }

            return null;
        }

        public InputSystem InputSystem
        {
            get { return _inputSystem; }
        }

        public bool DebugRay
        {
            get { return _debugRay; }
            set { _debugRay = value; }
        }

        public bool TouchController
        {
            get { return _touchController; }
            set { _touchController = value; }
        }

        public LayerMask ExclusionLayers
        {
            get { return _exclusionLayers; }
            set { _exclusionLayers = value; }
        }

        public float GazeSensitivity
        {
            get { return _gazeSensitivity; }
            set { _gazeSensitivity = value; }
        }

        public AGaze GazeObject
        {
            get { return _gazeObject; }
            set { _gazeObject = value; }
        }

        public float GazeSubmitTime
        {
            get { return _gazeSubmitTime; }
            set { _gazeSubmitTime = value; }
        }

        public float PrepareVrModeTime
        {
            get { return _prepareVrModeTime; }
            set { _prepareVrModeTime = value; }
        }

        public bool UseVrModeMultipleSubmits
        {
            get { return _multipleSubmits; }
            set { _multipleSubmits = value; }
        }

        public void StartListener()
        {
            if (_inputHandlerEnum != null)
                _monoObject.StopCoroutine(_inputHandlerEnum);

            _inputHandlerEnum = InputHandler();
            _monoObject.StartCoroutine(_inputHandlerEnum);
        }

        public void StopListener()
        {
            if (_inputHandlerEnum != null)
                _monoObject.StopCoroutine(_inputHandlerEnum);
        }

        public void AddInputListener(IInputManagerListener listener)
        {
            MotionEventsListener += listener.OnMotionEvents;
        }

        public void RemoveInputListener(IInputManagerListener listener)
        {
            MotionEventsListener -= listener.OnMotionEvents;
        }

        public void RemoveAllEvents()
        {
            if (_motionEventsListener != null)
            {
                foreach (Action<GameObject, MotionActions, Vector2> eh in _motionEventsListener.GetInvocationList())
                    _motionEventsListener -= eh;
            }
        }

        #region Actions

        private event Action<GameObject, MotionActions, Vector2> _motionEventsListener;

        public event Action<GameObject, MotionActions, Vector2> MotionEventsListener
        {
            add
            {
                _motionEventsListener = (Action<GameObject, MotionActions, Vector2>)Delegate.Combine(_motionEventsListener, value);
            }
            remove
            {
                if (_motionEventsListener != null)
                    _motionEventsListener = (Action<GameObject, MotionActions, Vector2>)Delegate.Remove(_motionEventsListener, value);
            }
        }

        #endregion
    }
}
