using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GetDataService
{

    #region SINGLETON PATTERN
    public static GetDataService _instance;
    public static GetDataService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GetDataService();
            }

            return _instance;
        }
    }
    #endregion

    private const string GET_GROUP_API = "https://vrcam-prod-api.istaging.com/api/v2/properties/openlink/{0}";
    private const string GET_BUILDING_API = "https://vrcam-prod-api.istaging.com/api/v2/buildings/openLink/{0}";


    GetDataService()
    {

    }

    public Group GetGroupData()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(GET_GROUP_API, "7da2b3e9-55df-44e7-8116-35adc653ae57"));
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        GroupParsedObj info = JsonUtility.FromJson<GroupParsedObj>(jsonResponse);

        info.buildings = info.buildings?.OrderBy(b => b.index).ToList();

        Group group = new Group();
        group.objectId = info.objectId;

        group.buildings = new List<Building>();
        if (info.buildings != null && info.buildings.Count > 0)
        {
            foreach (BuildingParsedObj buildingParsedObj in info.buildings) {
                group.buildings.Add(GetBuildingData(buildingParsedObj.objectId));
            }
        }

        return group;
    }

    public Building GetBuildingData(string buildingId)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(GET_BUILDING_API, buildingId));
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        Building info = JsonUtility.FromJson<Building>(jsonResponse);
        info.panoramas = info.panoramas.OrderBy(o => o.index).ToList();

        return info;
    }
}
