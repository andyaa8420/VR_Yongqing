using UnityEngine;
using System.Collections;

public class UIFollowCameraFix : MonoBehaviour
{
    public GameObject camera;

    public float dis = 9;

    private void Start()
    {
        camera = GameObject.Find("Head");
    }

    void Update()
    {

        float x = dis * Mathf.Sin(camera.transform.rotation.eulerAngles.y * Mathf.PI / 180);
        float z = dis * Mathf.Cos(camera.transform.rotation.eulerAngles.y * Mathf.PI / 180);
        float y = - Mathf.Sqrt(x * x + z * z) * Mathf.Tan(camera.transform.rotation.eulerAngles.x * Mathf.PI / 180);

        float r = Mathf.Sqrt(x * x + y * y + z * z);
        float scale = dis / r;

        //update the position
        transform.position = camera.transform.position + new Vector3(x * scale, y * scale, z * scale);
        transform.rotation = camera.transform.rotation;

    }

    public void DestryObjectWhenClick()
    {
        Debug.Log("DestryObjectWhenClick");
        Destroy(gameObject);
    }
}
