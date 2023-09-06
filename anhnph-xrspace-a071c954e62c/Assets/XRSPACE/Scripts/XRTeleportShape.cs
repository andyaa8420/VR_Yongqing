using UnityEngine;


public class XRTeleportShape: MonoBehaviour
{
    [SerializeField] XRTeleportCircle teleportCircle = null;
    [SerializeField] XRTeleportCurve teleportCurve = null;

    public void DrawTeleport(Vector3 strPoint, Vector3 endPoint, Color color, float auraAlpha = 1f, bool autoZoom = true)
    {
        DrawTeleport(strPoint, endPoint, color, color, auraAlpha, autoZoom);
    }

    public void DrawTeleport(Vector3 strPoint, Vector3 endPoint, Color circleColor,Color curveColor, float auraAlpha = 1f, bool autoZoom = true)
    {
        if(teleportCircle)
            teleportCircle.ShowCircle(strPoint, endPoint, circleColor, auraAlpha, autoZoom);
        if(teleportCurve)
            teleportCurve.ShowCurve(strPoint, endPoint, curveColor);
    }
        

    public void HideTeleport()
    {
        HideCircle();
        HideCurve();
    }

    #region Teleport Circle
    public void ShowCircle(Vector3 strPoint, Vector3 endPoint, Color color, float auraAlpha = 1f, bool autoZoom = true)
    {
        if(teleportCircle)
            teleportCircle.ShowCircle(strPoint, endPoint, color, auraAlpha, autoZoom);
    }

    public void HideCircle()
    {
        if(teleportCircle)
            teleportCircle.HideCircle();
    }

    #endregion

    #region Teleport Curve

    public void ShowCurve(Vector3 strPoint, Vector3 endPoint, Color color)
    {
        if(teleportCurve)
            teleportCurve.ShowCurve(strPoint,endPoint,color);
    }

    public void HideCurve()
    {
        if(teleportCurve)
            teleportCurve.HideCurve();
    }
    #endregion
}
