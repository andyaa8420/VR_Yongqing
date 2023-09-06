using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRSpace.Platform;
using XRSpace.Platform.InputDevice;

public class TeleportEffetControl : MonoBehaviour
{
    public XRHandlerDeviceType Device;
    public XRTeleportShape TeleportShape;
    public XRHandRaycaster HandRaycaster;
    public XRCTLRaycaster CTLRaycaster;
    public Transform Index;
    private bool _teleportDrawn;

    #region Colors

    private Color teleportWhite;
    private Color teleportRed;
    private Color teleportCyan;

    #endregion

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#FFFFFF", out teleportWhite);
        ColorUtility.TryParseHtmlString("#FF6C6C", out teleportRed);
        ColorUtility.TryParseHtmlString("#83dadb", out teleportCyan);
    }

    private void OnEnable()
    {
        HandRaycaster.TeleportEvent += DrawTeleportHand;
        CTLRaycaster.TeleportEvent += DrawTeleportCTL;
    }

    private void OnDisable()
    {
        HandRaycaster.TeleportEvent -= DrawTeleportHand;
        CTLRaycaster.TeleportEvent -= DrawTeleportCTL;
    }

    private void Start()
    {
        _teleportDrawn = false;
    }

    private void LateUpdate()
    {
        if (!_teleportDrawn)
            HideAll();
        _teleportDrawn = false;
    }

    public void HideAll()
    {
        HideTeleport();
    }

    #region Teleport
    // For hand raycaster
    private void DrawTeleportHand(Vector3 strPoint, Vector3 endPoint, XRRaycasterUtils.TeleportState state)
    {
        if (XRInputManager.Instance.HandlerType((XRDeviceType)Device) == XRHandlerType.Hand && XRInputManager.Instance.Gesture((XRDeviceType)Device)==XRHandlerGesture.Index_Outward)
        {
            switch (state)
            {
                case XRRaycasterUtils.TeleportState.CanTeleport:
                    Color circleColor = teleportCyan;
                    Color curveColor = teleportWhite;
                    float alpha = 1.0f;
                    if (TeleportShape)
                        TeleportShape.DrawTeleport(Index.position, endPoint, circleColor, curveColor, alpha);
                    _teleportDrawn = true;
                    break;
                case XRRaycasterUtils.TeleportState.InvalidTerrian:
                case XRRaycasterUtils.TeleportState.BlockByObstacle:
                    Color circleColors = teleportRed;
                    Color curveColors = teleportRed;
                    float alphas = 0f;
                    if (TeleportShape)
                        TeleportShape.DrawTeleport(Index.position, endPoint, circleColors, curveColors, alphas);
                    _teleportDrawn = true;
                    break;
                case XRRaycasterUtils.TeleportState.BlockByWall:
                    Color circleColorss = teleportRed;
                    Color curveColorss = teleportRed;
                    float alphass = 0f;
                    if (TeleportShape)
                        TeleportShape.DrawTeleport(Index.position, endPoint, circleColorss, curveColorss, alphass);
                    _teleportDrawn = true;
                    break;
                default:
                    break;
            }
        }
    }
    // For controller raycaster
    private void DrawTeleportCTL(Vector3 strPoint, Vector3 endPoint, XRRaycasterUtils.TeleportState state)
    {
        Color circleColor;
        Color curveColor;
        float alpha;
        switch (state)
        {
            case XRRaycasterUtils.TeleportState.CanTeleport:
                circleColor = teleportCyan;
                curveColor = teleportWhite;
                alpha = 1.0f;
                if (TeleportShape)
                    TeleportShape.DrawTeleport(strPoint, endPoint, circleColor, curveColor, alpha);
                _teleportDrawn = true;
                break;
            case XRRaycasterUtils.TeleportState.InvalidTerrian:
            case XRRaycasterUtils.TeleportState.BlockByObstacle:
                circleColor = teleportRed;
                curveColor = teleportRed;
                alpha = 0f;
                if (TeleportShape)
                    TeleportShape.DrawTeleport(strPoint, endPoint, circleColor, curveColor, alpha);
                _teleportDrawn = true;
                break;
            case XRRaycasterUtils.TeleportState.BlockByWall:
            case XRRaycasterUtils.TeleportState.NotProcess:
                break;
        }
    }
    public void HideTeleport()
    {
        if(TeleportShape)
            TeleportShape.HideTeleport();
    }
    #endregion
}

