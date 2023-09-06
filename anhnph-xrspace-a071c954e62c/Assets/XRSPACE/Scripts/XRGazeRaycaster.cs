using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRSpace.Platform.InputDevice;

public class XRGazeRaycaster : XRBaseRaycaster
{
    public LayerMask RaycastMask
    {
        get
        {
            return _raycastMask;
        }
        set
        {
            _raycastMask = value;
            if (_camera != null)
                _camera.cullingMask = value;
        }
    }
    [SerializeField]
    private LayerMask _raycastMask = ~(1 << 13);
    private Camera _camera;
    private PhysicsRaycaster _physicsRaycaster;
    private List<Canvas> canvases = new List<Canvas>();

    //HeadSet
    public float GazeInitialTime = 1.0f;
    public float GazeTime = 2.0f;
    public float FocusDistance = 0.3f;
    private float _hitTime = 0;
    private GameObject _hitObject;

    protected override void Start()
    {
        base.Start();
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = gameObject.AddComponent<Camera>();
            _camera.enabled = false;
            _camera.fieldOfView = 3;
            _camera.nearClipPlane = 0.01f;
            _camera.cullingMask = RaycastMask;
        }
        if (GetComponent<PhysicsRaycaster>() == null)
            _physicsRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
    }

    public override RaycastResult Raycast(XRPointerEventData eventData)
    {
        HitResult.Clear();
        if (!UseRaycast)
            return HitResult;
        Origin = transform.position;
        Direction = transform.forward;
        List<RaycastResult> raycastResult = new List<RaycastResult>();
        var data = eventData;
        //Set ray position at screen center
        data.position = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);
        UpdateCanvas();
        //Call all Raycaster's Raycast method include GraphicRayacster, PhisicalRaycaster and all other Raycaster inherit BaseRaycaster
        EventSystem.current.RaycastAll(data, raycastResult);
        //Alraedy done it in EventSystem.current.RaycastAll, but has a bug under 2018.4, fixed in 2019.1
        raycastResult.Sort(RaycastComparer);
        //Get the first valid raycaster result
        for (var i = 0; i < raycastResult.Count; ++i)
        {
            if (raycastResult[i].gameObject == null)
                continue;
            //if physicalRaycaster result is not this XRRaycaster's physicalRaycaster, ignore it.
            if (raycastResult[i].module is PhysicsRaycaster && raycastResult[i].module != _physicsRaycaster)
                continue;
            //if raycastResult it not GraphicRaycaster or PhysicsRaycaster, ignore it.
            if (!(raycastResult[i].module is PhysicsRaycaster) && !(raycastResult[i].module is GraphicRaycaster))
                continue;
            //if raycastResult is not match raycastMask, ignore.
            if ((1 << raycastResult[i].gameObject.layer | _raycastMask) != _raycastMask)
                continue;

            HitResult = raycastResult[i];
            //worldPosition and Normal is not assigned by raycastResult under 2018.4, and unity fixed in 2019.1
            HitResult.worldPosition = _camera.transform.position + _camera.transform.forward * raycastResult[i].distance;
            HitResult.worldNormal = -raycastResult[i].gameObject.transform.forward;
            return HitResult;
        }
        return HitResult;
    }

    private int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
    {
        if (lhs.module != rhs.module)
        {
            var lhsEventCamera = lhs.module.eventCamera;
            var rhsEventCamera = rhs.module.eventCamera;
            if (lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
            {
                // need to reverse the standard compareTo
                if (lhsEventCamera.depth < rhsEventCamera.depth)
                    return 1;
                if (lhsEventCamera.depth == rhsEventCamera.depth)
                    return 0;

                return -1;
            }

            if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

            if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
        }

        if (lhs.sortingLayer != rhs.sortingLayer)
        {
            // Uses the layer value to properly compare the relative order of the layers.
            var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
            var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
            return rid.CompareTo(lid);
        }

        if (lhs.sortingOrder != rhs.sortingOrder)
            return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

        //This comparison has bug in 2018, depth comparison only make sense in the same module.
        if (lhs.depth != rhs.depth && lhs.module == rhs.module)
            return rhs.depth.CompareTo(lhs.depth);

        if (lhs.distance != rhs.distance)
            return lhs.distance.CompareTo(rhs.distance);

        return lhs.index.CompareTo(rhs.index);
    }

    //Prepare all canvas for GraphicRaycaster
    private void UpdateCanvas()
    {
        canvases.Clear();
        canvases.AddRange(FindObjectsOfType<Canvas>());
        //Every world space canvas set eventCamera for GraphicRaycaster
        foreach (var canvas in canvases)
            if (canvas.renderMode == RenderMode.WorldSpace /*&& canvas.GetComponent<GraphicRaycaster>() != null*/)
                canvas.worldCamera = _camera;
    }

    private void Update()
    {
        ProcessGaze();

        ProcessFocus();
    }

    private void ProcessFocus()
    {
        IsFocus = HitResult.isValid && (HitResult.distance <= FocusDistance);
    }

    private void ProcessGaze()
    {
        //gaze raycast out from a gameobj to null
        if (HitResult.gameObject == null && _hitObject != null)
        {
            _hitObject = null;
            _hitTime = 0;
            XRInputManager.Instance.FillRate = 0;
            XRInputManager.Instance.IsGaze = false;
            IsPress = false;
            return;
        }
        //gaze raycast null
        if (HitResult.gameObject == null)
        {
            IsPress = false;
            return;
        }
        //gaze raycast at a new gameobj
        if (_hitObject != HitResult.gameObject)
        {
            _hitObject = HitResult.gameObject;
            _hitTime = Time.time;
            IsPress = false;
            return;
        }
        //gaze count start
        if ((Time.time - _hitTime) > GazeInitialTime)
        {
            XRInputManager.Instance.IsGaze = true;
        }
        else
        {
            return;
        }
        //gaze counting
        float fillRate = 0;
        if ((Time.time - _hitTime) > GazeInitialTime + GazeTime)
        {
            XRInputManager.Instance.FillRate = 0;
            XRInputManager.Instance.IsGaze = false;
            _hitObject = null;
            _hitTime = 0;
            IsPress = true;
        }
        else
        {
            fillRate = (Time.time - _hitTime - GazeInitialTime) / GazeTime;
            XRInputManager.Instance.FillRate = fillRate;
            IsPress = false;
        }
    }
}
