using System;
using System.Collections.Generic;

[Serializable]
public class Panorama
{

    public string objectId;
    public long createdAt;
    public DateTime createdAtDateTime => (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(createdAt);
    public long updatedAt;
    public DateTime updatedAtDateTime => (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(updatedAt);
    public string name;
    public double floorplanRotation;
    public string category;
    public bool cubemapReady;
    public string desktopUrl;
    public int index;
    public bool is720;
    public bool isStereo;
    public bool isTopLogo;
    public string mobileUrl;
    public string rawUrl;
    public string stereoUrl;
    public string thumbnail;
    public double geoLatitude;
    public double geoLongitude;
    public string cubemapFilePath;
    public string adjustedRawUrl;
    public bool lowResolutionCubemapReady;
    public double compassDegree;
    public string desktopRawUrl;
    public bool isAiFiltered;
    public Coordinate panoramaRotation;
    public Coordinate position;
    public List<Maker> markers;

    public Panorama()
    {
    }
}
