using UnityEngine;
using XRSpace.Platform.InputDevice;

public class XRReceiver : MonoBehaviour
{
    private Vector2 mouseNDC = Vector2.zero;
    private Vector2 mouseRotPos = Vector2.zero;

    private void Update()
    {
        if (XRInputManager.Instance.EditorMode == XREditorMode.Mouse)
        {
            if (Application.isEditor)
            {
                if (Input.GetMouseButton(1))    // 1/Right mouse button
                {
                    mouseRotPos.x += Input.GetAxis("Mouse X") * 5f;
                    mouseRotPos.y += Input.GetAxis("Mouse Y") * 5f;

                    transform.localRotation = Quaternion.Euler(-mouseRotPos.y, mouseRotPos.x, 0);
                }

                if (Input.GetMouseButton(2))    // 2/Middle mouse button
                {
                    transform.position += -transform.right * Input.GetAxis("Mouse X") * 0.1f;
                    transform.position += -transform.up * Input.GetAxis("Mouse Y") * 0.1f;
                }

                Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
                Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;

                if (Input.GetKey(KeyCode.W))
                    transform.position += forward * 0.05f;

                if (Input.GetKey(KeyCode.S))
                    transform.position -= forward * 0.05f;

                if (Input.GetKey(KeyCode.A))
                    transform.position -= right * 0.05f;

                if (Input.GetKey(KeyCode.D))
                    transform.position += right * 0.05f;

                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel");
                }
            }
        }
    }
}