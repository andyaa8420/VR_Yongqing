using UnityEngine;


//[ExecuteInEditMode]
public class XRTeleportCircle : MonoBehaviour
{
    #region Global Variable
    private static readonly float RADIUS_MAX = 0.50f;
    private static readonly float RADIUS_MIN = 0.05f;
    private static readonly float FAR_DISTANCE_BE_ZOOM = 5f;

    private static readonly string VISIBILITY_PROPERTY = "_Visibility";

    public float floatingDistance = 0.1f;


    public Transform teleportCircleEffect;
    public Vector3 targetOffset;
    private int mVertexCount = 50;
    public  int  VertexCount
    {
        get
        {
            return mVertexCount;
        }
        set
        {
            if (mVertexCount == value) { return; }
            mVertexCount = value;
        }
    }
    private float mRadius = 0.5f;
    public float Radius
    {
        get
        {
            return mRadius;
        }
        set
        {
            if (mRadius == value) { return; }
            mRadius = value;
        }
    }

    private float mVertexDeltaDegree = 0f; // auto calc

    private Material[] m_ColorMats;
    #endregion

    #region MonoBehaviour Flow
    private void Awake()
    {
        FetchReferences();
    }

    private void Start()
    {
        mVertexDeltaDegree = 2f * Mathf.PI / VertexCount;
    }
    #endregion

    #region Public Methods
    public void ShowCircle(Vector3 strPoint, Vector3 endPoint, Color color, float auraAlpha = 1f, bool autoZoom = true)
    {
        if(teleportCircleEffect && teleportCircleEffect.gameObject)
            teleportCircleEffect.gameObject.SetActive(true);

        Radius = autoZoom ? Mathf.Clamp(Vector3.Distance(strPoint,endPoint), RADIUS_MIN, RADIUS_MAX) : RADIUS_MAX;

        transform.up = Vector3.up; // Ensure teleport circle is always up right and not tilted
            
        ExecuteUpdate(endPoint);

        SetRenderColor(color, auraAlpha);
    }

    public void HideCircle()
    {
        if(teleportCircleEffect && teleportCircleEffect.gameObject)
            teleportCircleEffect.gameObject.SetActive(false);
    }

    public void ExecuteRender(bool enable, Color color, float auraAlpha = 1f)
    {
        SetRenderColor(color, auraAlpha);
        teleportCircleEffect.gameObject.SetActive(enable);
    }

    public void AutoZoomCircleByDistance(Vector3 strPoint, Vector3 endPoint)
    {
        AutoZoomCircleByDistance(Vector3.Distance(strPoint, endPoint));
    }

    void ExecuteUpdate(Vector3 centerPos)
    {
        if(teleportCircleEffect != null)
        {
            teleportCircleEffect.position = centerPos + targetOffset;
        }

        if (VertexCount < 1) { return; }


        Vector3 vertexPos = new Vector3(centerPos.x, centerPos.y, centerPos.z);

        float currentDegree = 0f;
        for (int i = 0; i < VertexCount; i++)
        {
            vertexPos.Set(Mathf.Cos(currentDegree) * Radius,
                            0,
                            Mathf.Sin(currentDegree) * Radius);
            vertexPos += centerPos + new Vector3(0, floatingDistance, 0);

            currentDegree += mVertexDeltaDegree;
        }
    }

    void AutoZoomCircleByDistance(float distance)
    {
        float radius = RADIUS_MAX * (distance / FAR_DISTANCE_BE_ZOOM);
        SetCircleRadius(radius);
    }

    void SetCircleRadius(float radius)
    {
        Radius = Mathf.Clamp(radius, RADIUS_MIN, RADIUS_MAX);
    }

    void SetRenderColor(Color color,float auraAlpha)
    {
        if (m_ColorMats[0] != null)
            m_ColorMats[0].color = color;
        if (m_ColorMats[1] != null)
            m_ColorMats[1].color = color;
        if (m_ColorMats[2] != null && m_ColorMats[2].HasProperty(VISIBILITY_PROPERTY))
        {
            var alpha = Mathf.Clamp01(auraAlpha);
            m_ColorMats[2].SetFloat(VISIBILITY_PROPERTY, alpha);
        }
    }
    #endregion

    #region Private Methods
    private void FetchReferences()
    {
        teleportCircleEffect = transform.GetChild(0);
        if(teleportCircleEffect && teleportCircleEffect.GetComponent<MeshRenderer>())
            m_ColorMats = teleportCircleEffect.GetComponent<MeshRenderer>().materials;
    }
    #endregion
} // END class