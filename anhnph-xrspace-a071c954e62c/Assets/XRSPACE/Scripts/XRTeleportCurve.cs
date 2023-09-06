using UnityEngine;


public enum XREBezierCurvePointType
{
    Start, Middle, End
}

[RequireComponent(typeof(LineRenderer))]
//[ExecuteInEditMode]
public class XRTeleportCurve : MonoBehaviour
{
    #region Global Variable

    private static readonly float BEZIER_CURVE_RATIO  = 1f;
    private static readonly float BEZIER_CURVE_HEIGHT = 2f;

    private static readonly string COLOR_PROPERTY = "_Color";

    public float floatingDistance = 0.01f;

    public LineRenderer lineRenderer;

    private int mVertexCount = 40;
    public  int  VertexCount
    {
        get
        {
            return mVertexCount;
        }
        set
        {
            if (mVertexCount == value) { return; }
            lineRenderer.positionCount = VertexCount;
        }
    }

    private Material m_Mat;

    private Vector3 mStrPoint;
    private Vector3 mMidPoint;
    private Vector3 mEndPoint;
    #endregion

    #region MonoBehaviour Flow
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        m_Mat = lineRenderer.material;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = VertexCount;
        }
    }
    #endregion

    #region Method
    public void ShowCurve(Vector3 strPoint, Vector3 endPoint,Color color)
    {
        lineRenderer.enabled = true;
        Vector3 midPoint = CalcCurveMidPoint(strPoint, endPoint);
        ExecuteUpdate(strPoint, midPoint, endPoint);
        if (m_Mat != null)
        {
            m_Mat.SetColor(COLOR_PROPERTY, color);
        }
    }

    public void HideCurve()
    {
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Update LineRenderer curve
    /// </summary>
    void ExecuteUpdate(Vector3 strPoint, Vector3 endPoint)
    {
        Vector3 midPoint = CalcCurveMidPoint(strPoint, endPoint);
        ExecuteUpdate(strPoint, midPoint, endPoint);
    }

    void ExecuteUpdate(Vector3 strPoint, Vector3 midPoint, Vector3 endPoint)
    {
        if (VertexCount < 1) { return; }
        if (lineRenderer == null) { return; }

        mStrPoint = strPoint;
        mMidPoint = midPoint;
        mEndPoint = endPoint;

        float currentRatio = 0f;
        for (int i = 0; i < VertexCount; i++)
        {
            Vector3 vertex1 = Vector3.Lerp(strPoint, midPoint, currentRatio);
            Vector3 vertex2 = Vector3.Lerp(midPoint, endPoint, currentRatio);

            lineRenderer.SetPosition(i, Vector3.Lerp(vertex1, vertex2, currentRatio));
            currentRatio += 1.0f / VertexCount;
        }
    }


    Vector3 CalcCurveMidPoint(Vector3 strPoint, Vector3 endPoint)
    {
        float curveIncreaseHeight = Vector3.Distance(strPoint, endPoint) / BEZIER_CURVE_HEIGHT;
        Vector3 curvePoint = Vector3.Lerp(strPoint, endPoint, BEZIER_CURVE_RATIO);
        curvePoint.y += curveIncreaseHeight;

        return curvePoint;
    }

    public Vector3 GetPrevCurvePoint(XREBezierCurvePointType eCurvePointType)
    {
        switch (eCurvePointType)
        {
            case XREBezierCurvePointType.Start:
                return mStrPoint;
            case XREBezierCurvePointType.Middle:
                return mMidPoint;
            case XREBezierCurvePointType.End:
                return mEndPoint;
            default:
                return Vector3.zero;
        }
    }
    #endregion
} // END class
