using System;
using System.Collections.Generic;

[Serializable]
public class GroupParsedObj
{
    public string objectId;
    public List<BuildingParsedObj> buildings;

    public GroupParsedObj()
    {
    }
}
