using System;
using System.Collections.Generic;

[Serializable]
public class VideoplayAttribute
{
    public bool autoplay;
    public bool mute;
}

[Serializable]
public class Region
{
    public int x;
    public int y;
    public int width;
    public int height;
}

[Serializable]
public class MultiTagInfo
{
    public string objectId;
    public string action;
    public string actionLink;
    public string description;
    public bool hidden;
    public bool lastEdit;
    public bool mainPhoto;
    public string name;
    public string photo;
    public string price;
}

[Serializable]
public class Maker
{
    public string objectId;
    public long createdAt;
    public DateTime createdAtDateTime => (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(createdAt);
    public long updatedAt;
    public DateTime updatedAtDateTime => (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(updatedAt);
    public string name;
    public string action;
    public string actionLink;
    public string currentPanoramaId;
    public string description;
    public long height;
    public string iconType;
    public string iconUrl;
    public string photo;
    public string nextPanoramaId;
    public string nextBuildingId; 
    public string price;
    public string type;
    public bool useCustomIcon;
    public long width;
    public double tagsizePercent;
    public bool fitGround;
    public double fitGroundDepth;
    public double spin;
    public double opacity;
    public bool customCoordinate;
    public long coordinateSpin;
    public long coordinatePlane;
    public bool disabled;
    public bool hidden;
    public Coordinate nextRotation;
    public Coordinate position;
    public Coordinate coordinatePosition;
    public Coordinate rotation;
    public VideoplayAttribute videoplayAttribute;
    public Region region;
    public List<string> category;
    public List<MultiTagInfo> multitagInfo;

    public Maker()
    {
    }
}
