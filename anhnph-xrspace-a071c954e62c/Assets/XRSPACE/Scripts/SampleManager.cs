using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XRSpace.Platform;
using XRSpace.Platform.IME;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;
using UnityEngine.SceneManagement;
using XRSpace.Platform.HardwareRemote;
using KeyCode = UnityEngine.KeyCode;

public class SampleManager : MonoBehaviour
{
    public static SampleManager Instance { get; private set; }

    public GameObject Character = null;
    public GameObject XRHandlerL = null;
    public GameObject XRHandlerR = null;
    // The pivot offset relative to head z axis.
    public float HeadDownwardOffset { get { return _headDownwardOffset; } set { _headDownwardOffset = value; XRHandlerUtils.HeadDownwardOffset = value; } }
    public float HeadSideOffset { get { return _headSideOffset; } set { _headSideOffset = value; XRHandlerUtils.HeadSideOffset = value; } }
    [SerializeField]
    private float _headDownwardOffset = 0.2f;
    [SerializeField]
    private float _headSideOffset = 0.12f;
    public float FollowingDis
    {
        get { return _followingDis; }
        set { _followingDis = Mathf.Clamp01(value); XRHandlerUtils.FollowingDis = _followingDis; }
    }
    public float VerticleWeight
    {
        get { return _verticleWeight; }
        set { _verticleWeight = Mathf.Clamp01(value); XRHandlerUtils.VerticalWeight = _verticleWeight; }
    }
    public float HorizonWeight
    {
        get { return _horizonWeight; }
        set { _horizonWeight = Mathf.Clamp01(value); XRHandlerUtils.HorizonWeight = _horizonWeight; }
    }
    [SerializeField]
    [Range(0, 1)]
    public float _followingDis = 1f;
    [SerializeField]
    [Range(0, 1)]
    public float _verticleWeight = 0.6f;
    [SerializeField]
    [Range(0, 1)]
    public float _horizonWeight = 0.3f;
    [SerializeField]
    private Text FPS = null;

    public AudioSource BackgroundMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Character.SetActive(true);
            DontDestroyOnLoad(Character);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate SampleManager! destroying....");
            Destroy(Character);
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        Input.backButtonLeavesApp = true;

		XRActionGestureManager.ActionDetectedEvent += (type, action) =>
		{
			if (type == XRDeviceType.HANDLER_LEFT && action == XRActionGesture.Grab_Inward)
			{
				XRIMEEventSystem.ActiveIme(false);
			}
		};

        XRHardwareRemote.replyCastStateChangedResultEvent += ReplyCastStateChangedResultEvent;

        XRDeviceType[] list = new XRDeviceType[4] {XRDeviceType.HANDLER_LEFT,XRDeviceType.HANDLER_RIGHT,XRDeviceType.LEFT_ARM,XRDeviceType.HEADSET };
        XRInputManager.Instance.SetDeviceConnectionAllowList(list);
	}

    private void OnDestroy()
    {
        XRHardwareRemote.replyCastStateChangedResultEvent -= ReplyCastStateChangedResultEvent;
    }

    private void ReplyCastStateChangedResultEvent(JsonDataReplyCastStateChangedResult obj)
    {
        if(obj.Status == CastState.DISPLAY_STATE_CONNECTED || obj.Status == CastState.DISPLAY_STATE_NOT_CONNECTED)
        {
            StartCoroutine(PlayBackgroundMusic());
        }
    }

    private IEnumerator PlayBackgroundMusic()
    {
        yield return new WaitForSeconds(1);

        if (BackgroundMusic != null)
            BackgroundMusic.Play();
    }

    private void OnEnable()
    {
		//XRIMEManager.OnIMEBegin += EnableHandRotation;
		//XRIMEManager.OnIMEEnd += DisableHandRotation;
		XRIMEEventSystem.OnBeginIME += EnableHandRotation;
		XRIMEEventSystem.OnEndIME += DisableHandRotation;
	}

    private void OnDisable()
    {
		//XRIMEManager.OnIMEBegin -= EnableHandRotation;
		//XRIMEManager.OnIMEEnd -= DisableHandRotation;
		XRIMEEventSystem.OnBeginIME -= EnableHandRotation;
		XRIMEEventSystem.OnEndIME -= DisableHandRotation;
	}

    public void SetGestureSimulation(bool isOn)
    {
        XRHandlerR.GetComponentInChildren<XRHandlerSwitch>().HandlerMode = isOn ? HandlerMode.Hand : HandlerMode.Auto;
        XRHandlerL.GetComponentInChildren<XRHandlerSwitch>().HandlerMode = isOn ? HandlerMode.Hand : HandlerMode.Auto;
    }

    private void EnableHandRotation()
    {
        XRHandlerR.GetComponentInChildren<XRHandlerReceiver>().EnableHandRotation = false;
        XRHandlerL.GetComponentInChildren<XRHandlerReceiver>().EnableHandRotation = false;
    }

    private void DisableHandRotation()
	{
		XRHandlerR.GetComponentInChildren<XRHandlerReceiver>().EnableHandRotation = true;
        XRHandlerL.GetComponentInChildren<XRHandlerReceiver>().EnableHandRotation = true;
    }

	void Update()
	{
		ProcessHandlerActive();

		FPS.text = "FPS: " + XRManager.Instance.FPS.ToString();

		if (Input.GetKeyDown(KeyCode.K))
		{
			XRIMEEventSystem.ActiveIme(true);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			XRIMEEventSystem.ActiveIme(false);
		}
    }

    private void ProcessHandlerActive()
    {
        //In editor do not change controller state 
        if (Application.isEditor && (XRInputManager.Instance.EditorMode == XREditorMode.Mouse))
        {
            if (XRHandlerL.activeInHierarchy)
                XRHandlerL.SetActive(false);
            if (!XRHandlerR.activeInHierarchy)
                XRHandlerR.SetActive(true);
            return;
        }

        //check have 3dof or 6dof controller connect and set active(true) to what controller connnected. 
        if (XRInputManager.Instance.IsConnect(XRDeviceType.HANDLER_RIGHT))
        {
            if (!XRHandlerR.activeInHierarchy)
                XRHandlerR.SetActive(true);
        }
        else
        {
            if (XRHandlerR.activeInHierarchy)
                XRHandlerR.SetActive(false);
        }
        if (XRInputManager.Instance.IsConnect(XRDeviceType.HANDLER_LEFT))
        {
            if (!XRHandlerL.activeInHierarchy)
                XRHandlerL.SetActive(true);
        }
        else
        {
            if (XRHandlerL.activeInHierarchy)
                XRHandlerL.SetActive(false);
        }
    }

    public void ResetScene()
    {
        var Go = FindObjectsOfType<CubeSelectionHint>();
        foreach (var obj in Go )
        {
            obj.ResetTrans();
        }
    }
}
