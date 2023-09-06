using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TWV;

public class UIFollowCamera : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public new GameObject camera;

    public TextureWebView twv = null;

    private const float DIS = 9f;

    private const float MAX_HEAD_RANGE = 40f;
    //movement speed in units per second
    private const float MOVEMENT_TIME = 45f;

    private float lastRotation = float.MinValue;
    private static float targetRotation = float.MinValue;

    private void Start()
    {
        camera = GameObject.Find("Head");
    }

    private void UpdateMakerPosition(float rotation)
    {
        float x = DIS * Mathf.Sin(rotation * Mathf.PI / 180);
        float z = DIS * Mathf.Cos(rotation * Mathf.PI / 180);
        //float y = - Mathf.Sqrt(x * x + z * z) * Mathf.Tan(camera.transform.rotation.eulerAngles.x * Mathf.PI / 180);
        float y = 0f;

        float r = Mathf.Sqrt(x * x + y * y + z * z);
        float scale = DIS / r;

        //update the position
        transform.position = camera.transform.position + new Vector3(x * scale, y * scale, z * scale);
        //transform.rotation = camera.transform.rotation;
        SetRotation(gameObject);
    }

    void Update()
    {
        if (targetRotation == float.MinValue)
        {
            targetRotation = camera.transform.rotation.eulerAngles.y;
        } else
        {
            float delta = floatMod(targetRotation - camera.transform.rotation.eulerAngles.y, 360);
            if (delta > 180f)
            {
                delta -= 360f;
            }
            if (Mathf.Abs(delta) > MAX_HEAD_RANGE)
            {
                targetRotation = camera.transform.rotation.eulerAngles.y;
            }
        }

        if (lastRotation == float.MinValue)
        {
            // first update, update immediately
            targetRotation = camera.transform.rotation.eulerAngles.y;
            lastRotation = targetRotation;
            UpdateMakerPosition(lastRotation);
        }
        else
        {
            if (floatMod(lastRotation, targetRotation) != 0f)
            {
                float delta = floatMod(targetRotation - lastRotation, 360);
                if (delta > 180f)
                {
                    delta -= 360f;
                }
                if (Mathf.Abs(delta) > MAX_HEAD_RANGE / 10f)
                {
                    targetRotation = camera.transform.rotation.eulerAngles.y;
                }

                lastRotation = lastRotation + CalculateMove(lastRotation, targetRotation);
                UpdateMakerPosition(lastRotation);
            } else
            {
                lastRotation = targetRotation;
            }
        }

    }

    private float floatMod(float x, float y)
    {
        while (x < 0)
        {
            x += y;
        }

        while (x > y)
        {
            x -= y;
        }

        return x;
    }

    private float CalculateMove(float lastRotation, float targetRotation)
    {
        float delta = floatMod(targetRotation - lastRotation, 360);
        if (delta > 180f)
        {
            delta -= 360f;
        }
        float maxMove = MOVEMENT_TIME * Time.deltaTime;
        if (Mathf.Abs(delta) <= maxMove)
        {
            return delta;
        }
        return delta < 0 ? -maxMove : maxMove;
    }


    private static void SetRotation(GameObject gameObject)
    {
        gameObject.transform.LookAt(2 * gameObject.transform.position);
    }

    public void DestryObjectWhenClick()
    {
        Debug.Log("DestryObjectWhenClick");
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 localPoint = transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
        print("OnPointerDown" + transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition).ToString("F6"));
        if (twv != null)
        {
            Vector2 point;
            point.x = (localPoint.x + (float)0.5) * (float)1280;
            point.y = ((float)1-(localPoint.y + (float)0.5)) * (float)720;
            print("OnPointerDown2 " + point.ToString("F6"));
            //twv.ClickTo(point);
            MotionActions action;
            action = MotionActions.Began;
            twv.SetMotionEvent(action, point.x, point.y);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector3 localPoint = transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
        print("OnPointerUp" + transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition).ToString("F6"));
        if (twv != null)
        {
            Vector2 point;
            point.x = (localPoint.x + (float)0.5) * (float)1280;
            point.y = ((float)1 - (localPoint.y + (float)0.5)) * (float)720;
            print("OnPointerUp2 " + point.ToString("F6"));
            //twv.ClickTo(point);
            MotionActions action;
            action = MotionActions.Ended;
            twv.SetMotionEvent(action, point.x, point.y);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 localPoint = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
        print("OnPointerEnter" + transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition).ToString("F6"));
        if (twv != null)
        {
            Vector2 point;
            point.x = (localPoint.x + (float)0.5) * (float)1280;
            point.y = ((float)1 - (localPoint.y + (float)0.5)) * (float)720;
            print("OnPointerEnter2 " + point.ToString("F6"));
            //twv.ClickTo(point);
            MotionActions action;
            action = MotionActions.Moved;
            twv.SetMotionEvent(action, point.x, point.y);
        }
    }

}
