using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
using UnityEngine.UI;
using XRSpace.Platform.InputDevice;

public class Gallery : MonoBehaviour, IRecyclableScrollRectDataSource
{
    public Text groupLabel;

    public GameObject galleryObj;

    private bool _startTouch;

    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    //Dummy data List
    private List<Panorama> _dataList = new List<Panorama>();

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        InitData();
        _recyclableScrollRect.DataSource = this;
    }

    //Initialising _contactList with dummy data 
    public void InitData()
    {
        groupLabel.text = SceneManager.currentBuilding.name;
        _dataList = SceneManager.currentBuilding.panoramas;
    }

    #region DATA-SOURCE

    /// <summary>
    /// Data source method. return the list length.
    /// </summary>
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    /// <summary>
    /// Data source method. Called for a cell every time it is recycled.
    /// Implement this method to do the necessary cell configuration.
    /// </summary>
    public void SetCell(ICell cell, int index)
    {
        //Casting to the implemented Cell
        var item = cell as DataCell;
        item.ConfigureCell(_dataList[index], index);
    }

    public void openGallery()
    {
        Animator animator = galleryObj.GetComponent<Animator>();
        if (animator != null)
        {
            bool isOpen = animator.GetBool("isOpen");
            animator.SetBool("isOpen", !isOpen);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool startTouch = XRInputManager.Instance.Button(XRDeviceType.HANDLER_LEFT, XRControllerButton.Grab)
            || XRInputManager.Instance.Button(XRDeviceType.HANDLER_RIGHT, XRControllerButton.Grab);

        if (startTouch)
        {
            if (!_startTouch)
            {
                openGallery();
            }
        }

        _startTouch = startTouch;
    }

    #endregion
}
