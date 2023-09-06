using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;

public class PunchGame : MonoBehaviour
{
    public bool IsPlayingPunchGame;
    public GameObject PunchingRoot;
    public Rigidbody PunchingBag;
    public GameObject PunchingVFX;
    public XRHandRaycaster[] HandRaycasters;

    private XRTrackerData _rightWrist;
    private readonly float _ignoreFactor = 5f;
    private bool _readyToPunch;
    private float _maxAccMaganitude;
    private Quaternion _rightRecenterRot = Quaternion.identity;

    private List<Vector3> _filterWindow = new List<Vector3>();
    private List<Vector3> _noiseClearWindow = new List<Vector3>();

    private float _lastClickTime;
    private float _longPressCount;

    private Vector3 _originPosition;
    private Quaternion _originRotation;

    // Start is called before the first frame update
    void Start()
    {
        _readyToPunch = false;
        _maxAccMaganitude = 0;
        _filterWindow.Clear();
        _noiseClearWindow.Clear();
        StartCoroutine(WaitingTrackerDataReady());
        _longPressCount = 0;
        _lastClickTime = 0;
        _originPosition = PunchingRoot.transform.position;
        _originRotation = PunchingRoot.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        PorcessPunchingRight();
    }

    private void PorcessPunchingRight()
    {
        if (_rightWrist == null)
            return;

        var filterdAcc = FilterLinearAcc(_rightWrist.LinearAcc);
        var upperArmAngle = Vector3.Angle(Vector3.ProjectOnPlane(XRIKSolver.Instance.RightLowerArm.position - XRIKSolver.Instance.RightUpperArm.position, -XRIKSolver.Instance.RightClavicle.right), -Vector3.up);

        if (upperArmAngle < 70)
        {
            _readyToPunch = true;
            _maxAccMaganitude = 0;
        }
        else
        {
            if (filterdAcc.magnitude > _maxAccMaganitude)
                _maxAccMaganitude = filterdAcc.magnitude;

            if (_readyToPunch && _maxAccMaganitude > _ignoreFactor)
            {
                var _hitPos = PunchingBag.GetComponent<CapsuleCollider>().ClosestPointOnBounds(XRManager.Instance.head.TransformPoint(Vector3.right * 0.1f));
                var punchfarward = _rightRecenterRot * _rightWrist.Rotation * Vector3.forward;
                var finalForce = 8f * _maxAccMaganitude * punchfarward;
                PunchingBag.AddForceAtPosition(finalForce, _hitPos);
                StartCoroutine(PopPunchingVFX(_hitPos));
                _maxAccMaganitude = 0;
                _readyToPunch = false;
            }
        }

        ProcessButtonEvent();
    }

    IEnumerator WaitingTrackerDataReady()
    {
        //Waiting tracker connect
        while (!XRInputManager.Instance.IsConnect(XRDeviceType.RIGHT_WRIST))
            yield return null;

        _rightWrist = XRInputManager.Instance.GetInputData<XRTrackerData>(XRDeviceType.RIGHT_WRIST);
        if (_rightWrist == null)
            XRLogger.Log("Get right wrist data failed.");
    }

    IEnumerator IKCalibration()
    {
        yield return new WaitForSeconds(3f);
        XRInputManager.Instance.RecenterIK();
        XRInputManager.Instance.Vibrate(XRDeviceType.RIGHT_WRIST);
        XRInputManager.Instance.Vibrate(XRDeviceType.RIGHT_ARM);
    }

    private Vector3 FilterLinearAcc(Vector3 acc)
    {
        //filter most noise
        if (_filterWindow.Count == 11)
        {
            var temp = Vector3.zero;
            //filter x
            if (Mathf.Abs(_filterWindow[5].x) > _ignoreFactor && Mathf.Abs(_filterWindow[0].x) > _ignoreFactor && Mathf.Abs(_filterWindow[10].x) > _ignoreFactor)
                temp.x = _filterWindow[5].x;
            if (Mathf.Abs(_filterWindow[5].x) < _ignoreFactor && Mathf.Abs(Mathf.Abs(_filterWindow[5].x)) < _ignoreFactor && Mathf.Abs(_filterWindow[10].x) < _ignoreFactor)
                temp.x = 0;
            if (Mathf.Abs(_filterWindow[5].x) < _ignoreFactor && ((Mathf.Abs(Mathf.Abs(_filterWindow[5].x)) - _ignoreFactor) * (Mathf.Abs(_filterWindow[10].x) - _ignoreFactor) < 0))
                temp.x = (Mathf.Abs(_filterWindow[5].x) / _ignoreFactor) * _filterWindow[5].x;
            //filter y
            if (Mathf.Abs(_filterWindow[5].y) > _ignoreFactor && Mathf.Abs(_filterWindow[0].y) > _ignoreFactor && Mathf.Abs(_filterWindow[10].y) > _ignoreFactor)
                temp.y = _filterWindow[5].y;
            if (Mathf.Abs(_filterWindow[5].y) < _ignoreFactor && Mathf.Abs(_filterWindow[0].y) < _ignoreFactor && Mathf.Abs(_filterWindow[10].y) < _ignoreFactor)
                temp.y = 0;
            if (Mathf.Abs(_filterWindow[5].y) < _ignoreFactor && ((Mathf.Abs(_filterWindow[0].y) - _ignoreFactor) * (Mathf.Abs(_filterWindow[10].y) - _ignoreFactor) < 0))
                temp.y = (Mathf.Abs(_filterWindow[5].y) / _ignoreFactor) * _filterWindow[5].y;
            //filter z
            if (Mathf.Abs(_filterWindow[5].z) > _ignoreFactor && Mathf.Abs(_filterWindow[0].z) > _ignoreFactor && Mathf.Abs(_filterWindow[10].z) > _ignoreFactor)
                temp.z = _filterWindow[5].z;
            if (Mathf.Abs(_filterWindow[5].z) < _ignoreFactor && Mathf.Abs(_filterWindow[0].z) < _ignoreFactor && Mathf.Abs(_filterWindow[10].z) < _ignoreFactor)
                temp.z = 0;
            if (Mathf.Abs(_filterWindow[5].z) < _ignoreFactor && ((Mathf.Abs(_filterWindow[0].z) - _ignoreFactor) * (Mathf.Abs(_filterWindow[10].z) - _ignoreFactor) < 0))
                temp.z = (Mathf.Abs(_filterWindow[5].z) / _ignoreFactor) * _filterWindow[5].z;

            //adding for next filter
            _filterWindow.Add(acc);
            //_noiseClearWindow.Add(_filterWindow[0]);
            _filterWindow.RemoveAt(0);
            return temp;
        }
        else
        {
            _filterWindow.Add(acc);
            return acc;
        }
    }

    private void ProcessButtonEvent()
    {
        if(_rightWrist.ButtonDown)
        {
            //Double click to over punch game
            if (Time.time - _lastClickTime < 0.5f)
            {
                IsPlayingPunchGame = false;
                ResetPunchingBag();
                //Close hand raycaster to avoid unexcept bahavior.
                foreach (var raycaster in HandRaycasters)
                    raycaster.UseRaycast = true;
            }
            _lastClickTime = Time.time;
        }

        if(_rightWrist.Button)
        {
            //Long press to start play punch.
            if (_longPressCount > 2f)
            {
                IsPlayingPunchGame = true;
                MovePunchingBagToPlayerFront();
                TrackerRecenter();
                StartCoroutine(IKCalibration());
                //Close hand raycaster to avoid unexcept bahavior.
                foreach (var raycaster in HandRaycasters)
                    raycaster.UseRaycast = false;

                _longPressCount = 0;
            }
            _longPressCount += Time.deltaTime;
        }

        if(_rightWrist.ButtonUp)
        {
            _longPressCount = 0;
        }
    }

    private void ProcessGesture(XRDeviceType type, XRActionGesture gesture)
    {
        if(type == XRDeviceType.HANDLER_RIGHT)
        {
            if(gesture == XRActionGesture.IndexDown_Inward)
            {
                IsPlayingPunchGame = true;
                MovePunchingBagToPlayerFront();
                //Close hand raycaster to avoid unexcept bahavior.
                foreach (var raycaster in HandRaycasters)
                    raycaster.UseRaycast = false;
            }

            if(gesture == XRActionGesture.Grab_Outward && IsPlayingPunchGame)
            {
                TrackerRecenter();
                StartCoroutine(IKCalibration());
            }

            if(gesture == XRActionGesture.Release_Inward)
            {
                IsPlayingPunchGame = false;
                //Close hand raycaster to avoid unexcept bahavior.
                foreach (var raycaster in HandRaycasters)
                    raycaster.UseRaycast = true;
            }
        }
    }
    //Move punching bag to player front
    private void MovePunchingBagToPlayerFront()
    {
        PunchingRoot.transform.position = XRManager.Instance.head.position;
        PunchingRoot.transform.rotation = Quaternion.Euler(new Vector3(0, XRManager.Instance.head.rotation.eulerAngles.y, 0));
    }

    private void ResetPunchingBag()
    {
        PunchingRoot.transform.position = _originPosition;
        PunchingRoot.transform.rotation = _originRotation;
    }

    private void TrackerRecenter()
    {
        var trackerForward = Vector3.ProjectOnPlane(_rightWrist.Rotation * Vector3.forward, Vector3.up);
        var headForward = Vector3.ProjectOnPlane(XRManager.Instance.head.forward, Vector3.up);
        _rightRecenterRot = Quaternion.FromToRotation(trackerForward, headForward);
    }

    IEnumerator PopPunchingVFX(Vector3 pos)
    {
        var vfx = Instantiate(PunchingVFX, pos, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        Destroy(vfx);
    }
}
