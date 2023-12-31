﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour
{
    private static bool destroyed = false;
    private static object lockObject = new object();
    private static SceneManager instance;
    private Stack<int> loadedLevels;
    public static Building currentBuilding;
    public static Panorama currentPanorama;

    public static SceneManager Instance
    {
        get
        {
            if (destroyed)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(SceneManager) +
                    "' already destroyed. Returning null.");
                return null;
            }

            lock (lockObject)
            {
                if (instance == null)
                {
                    // Search for existing instance.
                    instance = (SceneManager)FindObjectOfType(typeof(SceneManager));

                    // Create new instance if one doesn't already exist.
                    if (instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<SceneManager>();
                        singletonObject.name = typeof(SceneManager).ToString() + " (Singleton)";

                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return instance;
            }
        }
    }

    public static UnityEngine.SceneManagement.Scene GetActiveScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
    }

    public static void LoadScene(int buildIndex)
    {
        Instance.loadedLevels.Push(GetActiveScene().buildIndex);
        //UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex);
        Instance.StartCoroutine(Instance.LoadYourAsyncScene(buildIndex));
    }

    public static void LoadScene(string sceneName)
    {
        Instance.loadedLevels.Push(GetActiveScene().buildIndex);
        //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        Instance.StartCoroutine(Instance.LoadYourAsyncScene(sceneName));
    }

    public static void LoadPreviousScene()
    {
        if (Instance.loadedLevels.Count > 0)
        {
            //Instance.StartCoroutine(Instance.LoadYourAsyncScene(Instance.loadedLevels.Pop()));
            UnityEngine.SceneManagement.SceneManager.LoadScene(Instance.loadedLevels.Pop());
        }
        else
        {
            Debug.LogError("No previous scene loaded");
            // If you want, you can call `Application.Quit()` instead
        }
    }

    IEnumerator LoadYourAsyncScene(string scene)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator LoadYourAsyncScene(int scene)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }


    private void Awake()
    {
        loadedLevels = new Stack<int>();
    }

    private void OnApplicationQuit()
    {
        destroyed = true;
    }

    private void OnDestroy()
    {
        destroyed = true;
    }
}