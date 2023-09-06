using UnityEngine;
using UnityEngine.EventSystems;

public class CubeSelectionHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Material HintMat;
    private Material _originMat;
    private Vector3 _originPos;
    private Vector3 _originRot;

    void Start()
    {
        _originMat = gameObject.GetComponent<Renderer>().material;
        _originPos = transform.position;
        _originRot = transform.rotation.eulerAngles;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(HintMat)
            gameObject.GetComponent<Renderer>().material = HintMat;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.GetComponent<Renderer>().material = _originMat;
    }

    public void ResetTrans()
    {
        transform.position = _originPos;
        transform.rotation = Quaternion.Euler(_originRot);
    }
}
