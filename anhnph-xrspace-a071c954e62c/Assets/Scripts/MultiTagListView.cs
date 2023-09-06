using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
using UnityEngine.UI;
using System;

public class MultiTagListView : MonoBehaviour, IRecyclableScrollRectDataSource
{
    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    private Action<string> _actionLinkCallback;

    //Dummy data List
    private List<MultiTagInfo> _dataList = new List<MultiTagInfo>();

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        _recyclableScrollRect.DataSource = this;
    }

    //Initialising _contactList with dummy data 
    public void SetData(List<MultiTagInfo> dataList)
    {
        _dataList = dataList;
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
        var item = cell as MultiTagDataCell;
        item.ConfigureCell(_dataList[index]);
        item.AddActionLinkListener(OnActionClick);
    }
    
    private void OnActionClick(string actionLink)
    {
        if (_actionLinkCallback != null)
        {
            _actionLinkCallback(actionLink);
        }
    }

    public void AddActionLinkListener(Action<string> callback)
    {
        _actionLinkCallback = callback;
    }

    #endregion
}