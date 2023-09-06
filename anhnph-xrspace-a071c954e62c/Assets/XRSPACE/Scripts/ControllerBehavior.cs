using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;

public class ControllerBehavior : MonoBehaviour
{
    public XRCTLRaycaster CTLRaycaster;
    public XRHandlerDeviceType Device;
    public XRControllerButton TriggerButton;

    public Transform BeamRender;
    // Line
    public LineRenderer DistanceRenderer;
    public LineRenderer InfinityRenderer;

    // Dot
    public Transform EndPoint;
    public Material DotMaterial;
    private Vector3 _currentHitPos;
    private bool _shouldUpdateDot = false;

    //teleport
    private XRRaycasterUtils.TeleportState _teleportState;
    private Vector3 _teleportPos;
    private Vector3 _tpRecenterDir;
    public static UnityAction<Quaternion> OnTeleportEnd;

    //touch pad
    public Transform TouchPadDot;
    private Vector2 _lastTouchPadPos = Vector2.zero;
    private bool _startTouch;

    private void Start()
    {
        if (EndPoint)
            EndPoint.GetComponent<MeshRenderer>().sortingOrder = 2;
    }

    private void OnEnable()
    {
        CTLRaycaster.TeleportEvent += UpdateTeleportStatus;
        CTLRaycaster.AfterRaycasterEvent += DrawBeam;
    }

    private void OnDisable()
    {
        CTLRaycaster.TeleportEvent -= UpdateTeleportStatus;
        CTLRaycaster.AfterRaycasterEvent -= DrawBeam;
        HideAllLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (CTLRaycaster)
        {
            CTLRaycaster.HeadHeight = XRManager.Instance.transform.localPosition.y + XRManager.Instance.head.localPosition.y;
            if (!Application.isEditor || XRInputManager.Instance.EditorMode == XREditorMode.Simulator)
            {
                CTLRaycaster.Origin = transform.position;
                CTLRaycaster.Direction = transform.forward;
                CTLRaycaster.IsPress = XRInputManager.Instance.Button((XRDeviceType)Device, TriggerButton);
                CTLRaycaster.CanDrag = XRInputManager.Instance.Button((XRDeviceType)Device, TriggerButton);
                if (XRInputManager.Instance.Button((XRDeviceType)Device, XRControllerButton.TrackPadTouch) && !XRInputManager.Instance.Button((XRDeviceType)Device, XRControllerButton.TrackPadPress))
                {
                    if(!_startTouch)
                    {
                        //Debug.Log("[XRSDK] first _lastTouchPadPos");
                        _lastTouchPadPos = XRInputManager.Instance.TouchPosition((XRDeviceType)Device);
                    }
                    var delta = (XRInputManager.Instance.TouchPosition((XRDeviceType)Device) - _lastTouchPadPos);
                    var signX = Mathf.Sign(delta.x);
                    var signY = Mathf.Sign(delta.y);
                    delta *= 10;
                    delta = new Vector3(signX * Mathf.Round(Mathf.Abs(delta.x)), signY * Mathf.Floor(Mathf.Abs(delta.y)));
                    delta /= 10;
                    CTLRaycaster.ScrollDelta = delta;
                    //_lastTouchPadPos = XRInputManager.Instance.TouchPosition((XRDeviceType)Device);
                    _startTouch = true;
                }
                else
                    _startTouch = false;
            }
        }
        if(XRInputManager.Instance.ButtonUp((XRDeviceType)Device, TriggerButton)
            && _teleportState == XRRaycasterUtils.TeleportState.CanTeleport)
        {
            Teleport();
        }
        ProcessTouchpadDot();

        //for editor mouse control
        if (Application.isEditor && XRInputManager.Instance.EditorMode != XREditorMode.Simulator)
        {
            if (XRInputManager.Instance.EditorMode == XREditorMode.Mouse)
            {
                if (Device == XRHandlerDeviceType.HANDLER_LEFT || Device == XRHandlerDeviceType.HANDLER_RIGHT)
                {
                    if (Input.GetMouseButtonDown(0)) //change to only mouseleft
                    {

                        if (CTLRaycaster != null)
                        {
                            CTLRaycaster.IsPress = true;
                            CTLRaycaster.CanDrag = true;
                            DotMaterial.SetFloat("_Blend", 1);
                        }

                        if (_teleportState == XRRaycasterUtils.TeleportState.CanTeleport)
                        {
                            Teleport();
                        }

                    }

                    if (Input.GetMouseButtonUp(0)) //change to only mouseleft
                    {

                        if (CTLRaycaster != null)
                        {
                            CTLRaycaster.IsPress = false;
                            CTLRaycaster.CanDrag = false;
                            DotMaterial.SetFloat("_Blend", 0);
                        }
                    }

                }
            }
        }
        BeamRender.position = transform.position;
        BeamRender.rotation = XRManager.Instance.head.rotation;
    }

    private void UpdateTeleportStatus(Vector3 Origin, Vector3 Target, XRRaycasterUtils.TeleportState teleportState)
    {
        _teleportState = teleportState;
        if (_teleportState == XRRaycasterUtils.TeleportState.NotProcess)
            return;
        //var heigh = XRManager.Instance.transform.position.y;
        _teleportPos = Target;
        Vector3 direction = Target - XRManager.Instance.head.position;
        _tpRecenterDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
    }

    private void Teleport()
    {
        if (_teleportState != XRRaycasterUtils.TeleportState.CanTeleport || !CTLRaycaster.UseRaycast)
            return;

        var playerRotate = Quaternion.FromToRotation(Vector3.ProjectOnPlane(XRManager.Instance.head.forward, Vector3.up), _tpRecenterDir);
        XRManager.Instance.transform.rotation *= playerRotate;
        var playerShift = _teleportPos - XRManager.Instance.head.position;
        //y shift is from player floor pos to teleportPos 
        playerShift.y = _teleportPos.y - XRManager.Instance.transform.parent.position.y;
        XRManager.Instance.transform.parent.position += playerShift;
        if (!Application.isEditor || XRInputManager.Instance.EditorMode == XREditorMode.Simulator)
            OnTeleportEnd(playerRotate);
    }


    private void LateUpdate()
    {
        //LineRender render line at lateUpdate.
        //If EndPoint update position at DrawBeam, will see the point and line not sync.
        if (_shouldUpdateDot && EndPoint)
            BeamRenderUtils.UpdateHitDot(EndPoint, _currentHitPos, XRManager.Instance.head.position);
    }

    private void DrawBeam(Vector3 origin, Vector3 direction, XRCTLRaycaster.RaycasterType type, RaycastResult result)
    {
        if (type == XRCTLRaycaster.RaycasterType.UIRaycaster || type == XRCTLRaycaster.RaycasterType.ObjectRaycaster)
        {
            _currentHitPos = result.worldPosition;
            if (DistanceRenderer)
            {
                BeamRenderUtils.UpdateLine(DistanceRenderer, origin, result.worldPosition);
                DistanceRenderer.enabled = true;
            }
            if (EndPoint)
            {
                _shouldUpdateDot = true;
                EndPoint.gameObject.SetActive(true);
            }
            if (InfinityRenderer)
                InfinityRenderer.enabled = false;
        }
        else if (type == XRCTLRaycaster.RaycasterType.None)
        {
            if (DistanceRenderer)
                DistanceRenderer.enabled = false;
            if (EndPoint)
            {
                _shouldUpdateDot = false;
                EndPoint.gameObject.SetActive(false);
            }
            if (InfinityRenderer)
            {
                InfinityRenderer.enabled = true;
                BeamRenderUtils.UpdateLine(InfinityRenderer, origin, origin + direction * 3f);
            }
        }
        else
        {
            if (DistanceRenderer)
                DistanceRenderer.enabled = false;
            if (EndPoint)
            {
                _shouldUpdateDot = false;
                EndPoint.gameObject.SetActive(false);
            }
            if (InfinityRenderer)
            {
                InfinityRenderer.enabled = false;
            }
        }
    }

    private void ProcessTouchpadDot()
    {
        if (TouchPadDot)
        {
            TouchPadDot.gameObject.SetActive(XRInputManager.Instance.Button((XRDeviceType)Device, XRControllerButton.TrackPadTouch));
            TouchPadDot.localPosition = 15 * XRInputManager.Instance.TouchPosition((XRDeviceType)Device);
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
}
