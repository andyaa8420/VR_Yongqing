using UnityEngine;
using UnityEngine.UI;
using PolyAndCode.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DataCell : MonoBehaviour, ICell
{
    //UI
    public Text nameLabel;

    public Image thumbnail;

    //Model
    private Building _dataInfo;
    private Panorama _panoramaInfo;
    private int _cellIndex;

    private void Start()
    {
        //Can also be done in the inspector
        GetComponent<Button>().onClick.AddListener(ButtonListener);
    }

    //This is called from the SetCell method in DataSource
    public void ConfigureCell(Building dataInfo,int cellIndex)
    {
        _cellIndex = cellIndex;
        _dataInfo = dataInfo;

        nameLabel.text = dataInfo.name;

        Davinci.get()
            .load(dataInfo.thumbnail)
            .into(thumbnail)
            .setFadeTime(1f)
            .setCached(true)
            .start();
    }

    public void ConfigureCell(Panorama dataInfo, int cellIndex)
    {
        _cellIndex = cellIndex;
        _panoramaInfo = dataInfo;

        nameLabel.text = dataInfo.category;

        Davinci.get()
            .load(dataInfo.thumbnail)
            .into(thumbnail)
            .setFadeTime(1f)
            .setCached(true)
            .start();
    }

    private void ButtonListener()
    {
        if (!object.ReferenceEquals(null, _dataInfo))
        {
            Debug.Log("Index : " + _cellIndex + ", Name : " + _dataInfo.name);
            SceneManager.currentBuilding = _dataInfo;
            SceneManager.currentPanorama = _dataInfo.panoramas[0];
            SceneManager.LoadScene(sceneName: "LiveTourScreen");
        } else
        {
            SceneManager.currentPanorama = _panoramaInfo;
            LoadImageToSkybox lt = GameObject.Find("LiveTourManager").GetComponent<LoadImageToSkybox>();
            lt.StartCoroutine(lt.InitScreen());
        }
    }
}
