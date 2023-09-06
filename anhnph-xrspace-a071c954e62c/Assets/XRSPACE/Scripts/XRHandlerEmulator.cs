using UnityEngine;
using XRSpace.Platform.InputDevice;
using XRSpace.Platform.VRcore;

public class XRHandlerEmulator : MonoBehaviour
{
    public XRCTLRaycaster CTLRaycaster;

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && (XRInputManager.Instance.EditorMode == XREditorMode.Mouse))
        {
            ProcessMouseButton();
            ControlRaycastByMouse();
        }
    }

    private void ProcessMouseButton()
    {
        CTLRaycaster.IsPress = Input.GetMouseButton(0);
    }

    private void ControlRaycastByMouse()
    {
        if (Input.mousePosition.x < Screen.width / 2f)
        {
            //Left screen point to ray
            Ray ray = XRManager.Instance.leftCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 VectorOfController = transform.position - ray.origin;

            //IME keyboard add collider, this block will affect the ray direction
            //if (Physics.Raycast(ray, out hit, 10.0f, _raycaster.RaycastMask))
            //    transform.forward = ray.direction * hit.distance - VectorOfController;
            //else
            transform.forward = ray.direction * 2 - VectorOfController;
        }
        if (Input.mousePosition.x > Screen.width / 2f)
        {
            //Right screen point to ray
            Ray ray = XRManager.Instance.rightCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x - (Screen.width / 2), Input.mousePosition.y, 0));

            Vector3 VectorOfController = transform.position - ray.origin;

            //IME keyboard add collider, this block will affect the ray direction
            //if (Physics.Raycast(ray, out hit, 1.0f, _raycaster.RaycastMask))
            //    transform.forward = ray.direction * hit.distance - VectorOfController;
            //else
            transform.forward = ray.direction * 2 - VectorOfController;
        }
    }
}
