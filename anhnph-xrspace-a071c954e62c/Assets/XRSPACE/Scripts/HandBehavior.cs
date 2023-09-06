using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;

public class HandBehavior : MonoBehaviour
{
    public XRHandlerDeviceType DeviceType;
    public XRHandRaycaster HandRaycaster;
    public Transform Index;
    public Transform Knuckle;
    public Transform HoldingAnchor;
    private XRHandlerData _handlerData;
    private XRGestureAnimation _xRGestureAnimation;
    private int _touchPriority;
    private int _uiPriority;
    private int _objectPriority;
    private int _teleportPriority;

    //Grab
    private GameObject _grabGo;
    private Vector3 _originPos;
    private Vector3 _originRot;
    private Vector3 _currentGrabbedLocation;
    private Vector3 _handVector;
    private float _startTime;
    private bool _isGrabGo = false;
    private bool _onTransition = false;

    public Transform BeamRender;
    // Reticle Line
    public LineRenderer DistanceRenderer;
    public LineRenderer InfinityRenderer;

    // Reticle Point
    public Transform EndPoint;
    public Material DotMaterial;
    private Vector3 _currentHitPos;
    private bool _shouldUpdateDot = false;

    //teleport
    private XRRaycasterUtils.TeleportState _teleportState;
    private Vector3 _teleportPos;
    private Vector3 _tpRecenterDir;

    // Start is called before the first frame update
    void Start()
    {
        if (HandRaycaster)
            HandRaycaster.Head = XRManager.Instance.head;
        _xRGestureAnimation = transform.GetComponentInParent<XRGestureAnimation>();

        _touchPriority = HandRaycaster.UITouch;
        _uiPriority = HandRaycaster.UIRaycaster;
        _objectPriority = HandRaycaster.ObjectRaycaster;
        _teleportPriority = HandRaycaster.Teleport;

        _handlerData = XRInputManager.Instance.GetInputData<XRHandlerData>((XRDeviceType)DeviceType);

        if (EndPoint)
            EndPoint.GetComponent<MeshRenderer>().sortingOrder = 2;
    }

    private void OnEnable()
    {
        XRActionGestureManager.ActionDetectedEvent += ProcessActionGesture;
        HandRaycaster.TeleportEvent += UpdateTeleportStatus;
        HandRaycaster.AfterRaycasterEvent += DrawBeam;
    }

    private void OnDisable()
    {
        XRActionGestureManager.ActionDetectedEvent -= ProcessActionGesture;
        HandRaycaster.TeleportEvent -= UpdateTeleportStatus;
        HandRaycaster.AfterRaycasterEvent -= DrawBeam;
        HideAllLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (HandRaycaster)
        {
            HandRaycaster.HeadHeight = XRManager.Instance.transform.localPosition.y + XRManager.Instance.head.localPosition.y;
            HandRaycaster.Origin = XRManager.Instance.transform.TransformPoint(_handlerData.RaycastOrigin);
            HandRaycaster.Direction = XRManager.Instance.transform.TransformDirection(_handlerData.RaycastDirection);
            if (!HandRaycaster.UseRaycast)
                HideAllLine();
        }
        else
            HideAllLine();

        ProcessGesture(XRInputManager.Instance.Gesture((XRDeviceType)DeviceType));
        //move Go to hand
        if (_isGrabGo)
        {
            var HandPos = XRManager.Instance.transform.TransformPoint(_handlerData.Position);
            MoveObject();
            _handVector = HandPos - _currentGrabbedLocation;
            _currentGrabbedLocation = HandPos;
        }
        BeamRender.position = transform.position;
        BeamRender.rotation = XRManager.Instance.head.rotation;
    }

    private void LateUpdate()
    {
        //LineRender render line at lateUpdate.
        //If EndPoint update position at DrawBeam, will see the point and line not sync.
        if(_shouldUpdateDot && EndPoint)
            BeamRenderUtils.UpdateHitDot(EndPoint, _currentHitPos, XRManager.Instance.head.position);
    }

    private void OnDestroy()
    {
        XRActionGestureManager.ActionDetectedEvent -= ProcessActionGesture;
    }

    private void ProcessActionGesture(XRDeviceType deviceType, XRActionGesture actionGesture)
    {
        if (deviceType != (XRDeviceType)DeviceType)
            return;

        switch (actionGesture)
        {
            case XRActionGesture.IndexDown_Outward:
                HandRaycaster.IsPress = true;
                HandRaycaster.CanDrag = true;
                DotMaterial.SetFloat("_Blend", 1);
                Teleport();
                break;
            case XRActionGesture.PointerUp:
                HandRaycaster.IsPress = false;
                HandRaycaster.CanDrag = false;
                DotMaterial.SetFloat("_Blend", 0);
                break;
            case XRActionGesture.Grab_Outward:
                Grab();
                break;
            case XRActionGesture.Release_Inward:
            case XRActionGesture.Release_Outward:
                HandRaycaster.IsPress = false;
                HandRaycaster.CanDrag = false;
                Release();
                DotMaterial.SetFloat("_Blend", 0);
                break;
            case XRActionGesture.IndexDown_Inward:
            case XRActionGesture.IndexUp_Outward:
            case XRActionGesture.IndexUp_Inward:
                HandRaycaster.IsPress = false;
                HandRaycaster.CanDrag = false;
                DotMaterial.SetFloat("_Blend", 0);
                break;
        }
    }

    private void ProcessGesture(XRHandlerGesture gesture)
    {
        switch (gesture)
        {
            case XRHandlerGesture.Index_Outward:
                HandRaycaster.ObjectRaycaster = -1;
                HandRaycaster.UITouch = _touchPriority;
                HandRaycaster.UIRaycaster = _uiPriority;
                HandRaycaster.Teleport = _teleportPriority;
                break;
            case XRHandlerGesture.Fist_Outward:
                if (_isGrabGo)
                    HandRaycaster.ObjectRaycaster = -1;
                break;
            case XRHandlerGesture.Fist_Inward:
                HandRaycaster.UIRaycaster = -1;
                HandRaycaster.ObjectRaycaster = -1;
                HandRaycaster.UITouch = -1;
                break;
            case XRHandlerGesture.Open_Outward:
                HandRaycaster.ObjectRaycaster = _objectPriority;
                HandRaycaster.UITouch = -1;
                HandRaycaster.Teleport = -1;
                HandRaycaster.UIRaycaster = -1;
                break;
            default:
                break;
        }
    }

    #region Interaction
    private void Grab()
    {
        if (HandRaycaster.ResultType == XRHandRaycaster.RaycasterType.ObjectRaycaster)
        {
            var Go = HandRaycaster.HitResult.gameObject;
            if (Go != null && !_isGrabGo && !_onTransition)
            {
                if (Go.GetComponent<Rigidbody>() != null)
                {
                    var _rigidbody = Go.GetComponent<Rigidbody>();
                    _rigidbody.useGravity = false;
                    _rigidbody.isKinematic = true;
                }
                _startTime = Time.time;
                _grabGo = HandRaycaster.HitResult.gameObject;
                _isGrabGo = true;
                _xRGestureAnimation.GrabCube = true;
                //avoid user grab>>release>>grab in short time
                _onTransition = true;
            }
        }
    }

    private void Release()
    {
        if (_grabGo != null)
        {
            if (_grabGo.GetComponent<Rigidbody>() != null)
            {
                var _rigidbody = _grabGo.GetComponent<Rigidbody>();
                _rigidbody.useGravity = true;
                _rigidbody.isKinematic = false;
                if(_grabGo.transform.parent == HoldingAnchor)
                    _grabGo.transform.parent = null;
                _rigidbody.AddForce(_handVector * 300, ForceMode.Impulse);
            }
            _grabGo = null;
            _isGrabGo = false;
            _xRGestureAnimation.GrabCube = false;
            _onTransition = false;
        }
    }

    //not every user grab untill gameobject on hand
    private void MoveObject()
    {
        var handPos = HoldingAnchor.position;
        float journeyLength = Vector3.Distance(_grabGo.transform.position, handPos);
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - _startTime) * 2;//2 speed
        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;
        _grabGo.transform.position = Vector3.Lerp(_grabGo.transform.position, handPos, fractionOfJourney);
        if (Vector3.Distance(_grabGo.transform.position, handPos) == 0)
            _grabGo.transform.parent = HoldingAnchor;
    }
    #endregion

    #region Teleport
    private void UpdateTeleportStatus(Vector3 Origin, Vector3 Target, XRRaycasterUtils.TeleportState teleportState)
    {
        _teleportState = teleportState;
        _teleportPos = Target;
        if (_teleportState == XRRaycasterUtils.TeleportState.NotProcess)
            return;
        Vector3 direction = Target - XRManager.Instance.transform.position;
        _tpRecenterDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
    }

    private void Teleport()
    {
        if (_teleportState != XRRaycasterUtils.TeleportState.CanTeleport || !HandRaycaster.UseRaycast)
            return;

        var playerRotate = Quaternion.FromToRotation(Vector3.ProjectOnPlane(XRManager.Instance.head.forward, Vector3.up), _tpRecenterDir);
        XRManager.Instance.transform.rotation *= playerRotate;
        var playerShift = _teleportPos - XRManager.Instance.head.position;
        //y shift is from player floor pos to teleportPos 
        playerShift.y = _teleportPos.y - XRManager.Instance.transform.parent.position.y;
        XRManager.Instance.transform.parent.position += playerShift;
    }
    #endregion

    #region BeamRender
    private void DrawBeam(Vector3 origin, Vector3 direction, XRHandRaycaster.RaycasterType type, RaycastResult result)
    {
        origin = Knuckle.position;
        if (type == XRHandRaycaster.RaycasterType.ObjectRaycaster)
        {
            // When only collider and Open_Outward, draw ray. Length of ray is distance of palm to gameobject.
            if (XRInputManager.Instance.Gesture((XRDeviceType)DeviceType) == XRHandlerGesture.Open_Outward)
            {
                _currentHitPos = result.worldPosition;
                DrawLine(true, origin, _currentHitPos);
            }
            else
                HideAllLine();
        }
        else if (type == XRHandRaycaster.RaycasterType.UIRaycaster)
        {
            origin = Index.position;
            _currentHitPos = result.worldPosition;
            DrawLine(true, origin, _currentHitPos);
        }
        else if (XRInputManager.Instance.Gesture((XRDeviceType)DeviceType) != XRHandlerGesture.Open_Outward || type == XRHandRaycaster.RaycasterType.UITouch)
            HideAllLine();
        else
            DrawLine(false, origin, direction.normalized * 3 + origin);
    }

    private void DrawLine(bool isHit, Vector3 start, Vector3 end)
    {
        _shouldUpdateDot = isHit;
        if (isHit)
        {
            if (DistanceRenderer)
            {
                BeamRenderUtils.UpdateLine(DistanceRenderer, start, end);
                DistanceRenderer.enabled = true;
            }
            if (EndPoint)
                EndPoint.gameObject.SetActive(true);
            if (InfinityRenderer)
                InfinityRenderer.enabled = false;
        }
        else
        {
            if (DistanceRenderer)
                DistanceRenderer.enabled = false;
            if (EndPoint)
                EndPoint.gameObject.SetActive(false);
            if (InfinityRenderer)
            {
                BeamRenderUtils.UpdateLine(InfinityRenderer, start, end);
                InfinityRenderer.enabled = true;
            }
        }
    }

    private void HideAllLine()
    {
        _shouldUpdateDot = false;
        if (DistanceRenderer)
            DistanceRenderer.enabled = false;
        if (EndPoint)
            EndPoint.gameObject.SetActive(false);
        if (InfinityRenderer)
            InfinityRenderer.enabled = false;
    }
    #endregion
}
