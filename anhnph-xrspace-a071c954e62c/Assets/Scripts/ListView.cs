using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
using UnityEngine.UI;
using System.Collections;
using System.Threading;

public class ListView : MonoBehaviour, IRecyclableScrollRectDataSource
{
    public Text groupLabel;

    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    //Dummy data List
    private List<Building> _dataList = new List<Building>();

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        _recyclableScrollRect.DataSource = this;
        Reload();
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

    public void Reload()
    {
        StartCoroutine(ReloadData());
    }

    private Group group = null;
    private IEnumerator ReloadData()
    {
        group = null;
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            group = GetDataService.Instance.GetGroupData();
        }).Start();
       
        while (group == null)
        {
            yield return null;
        }
        _dataList = group.buildings;
        _recyclableScrollRect.ReloadData();
    }

        #endregion
    }