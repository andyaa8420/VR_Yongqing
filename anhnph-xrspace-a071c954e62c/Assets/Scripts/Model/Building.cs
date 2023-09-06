using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Building
{
    public string objectId;
    public long createdAt;
    public DateTime createdAtDateTime => (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(createdAt);
    public long updatedAt;
    public DateTime updatedAtDateTime => (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(updatedAt);
    public string name;
    public int logoSize;
    public bool showComment;
    public bool showContactInfo;
    public bool showPoweredBy;
    public string themeColor;
    public bool unavailable;
    public bool hasPin;
    public bool requireVisitorData;
    public bool tourRingBtn;
    public bool compass;
    public int floorplanSize;
    public bool markerFinderEnable;
    public bool calendar;
    public Owner Owner;
    public string thumbnail;
    public List<Panorama> panoramas;

    public Building()
    {
    }
}
