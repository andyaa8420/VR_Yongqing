using UnityEngine;
using System.Collections;

public class QuitApp: MonoBehaviour
{
    public void Quit()
    {
        print("Application.Quit");
        Application.Quit();
    }

    public void ClearCache()
    {
        print("Clear cache");
        Davinci.ClearAllCachedFiles();
    }
}
