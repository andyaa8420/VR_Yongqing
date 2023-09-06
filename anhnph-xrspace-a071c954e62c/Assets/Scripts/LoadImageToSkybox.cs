using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LoadImageToSkybox : MonoBehaviour
{
    private GameObject loadingBarObj;
    private GameObject galleryObj;
    private const int CubemapResolution = 1440;

    //Runtime
    private Material currentSkyboxMaterial;
    private Cubemap currentSkyboxCubemap;

    private bool canChangeScreen = true;

    private void Start()
    { // When the game starts, apply the skybox texture
        StartCoroutine(InitScreen());
    }

    public IEnumerator InitScreen()
    {
        if (!canChangeScreen)
        {
            yield break;
        }

        GameObject[] makers = GameObject.FindGameObjectsWithTag("Maker");

        for (int i = 0; i < makers.Length; i++)
        {
            DestroyImmediate(makers[i]);
        }

        GameObject[] previews = GameObject.FindGameObjectsWithTag("Preview");

        for (int i = 0; i < previews.Length; i++)
        {
            DestroyImmediate(previews[i]);
        }

        GameObject[] popups = GameObject.FindGameObjectsWithTag("Popup");

        for (int i = 0; i < popups.Length; i++)
        {
            DestroyImmediate(popups[i]);
        }

        if (loadingBarObj == null)
        {
            loadingBarObj = GameObject.Find("LoadingBar");
        }
        loadingBarObj.SetActive(true);

        if (galleryObj == null)
        {
            galleryObj = GameObject.Find("Gallery");
        } else
        {
            galleryObj.SetActive(false);
        }

        DestroyImmediate(currentSkyboxCubemap);
        DestroyImmediate(currentSkyboxMaterial);

        LoadingBar loadingBar = loadingBarObj.transform.Find("vica").gameObject.GetComponent<LoadingBar>();
        loadingBar.SetProcess(0.1f);
        while (loadingBar.CurrentProcess < 0.1f)
        {
            yield return null;
        }

        canChangeScreen = false;

        Davinci data = Davinci.get();
        data.load(SceneManager.currentPanorama.desktopUrl)
        .setCached(true)
        .withLoadedAction(() =>
        {
            Texture2D source = data.rawTexture2D;

            Cubemap cubemap = PanoramicToCubemapRuntimeConverter.ConvertPanoramaTextureToCubemap(source, CubemapResolution);
            cubemap.filterMode = FilterMode.Trilinear;
            currentSkyboxCubemap = cubemap;

            Destroy(source);
        })
        .start();
        loadingBar.SetProcess(0.9f);
        while (currentSkyboxCubemap == null)
        {
            yield return null;
        }
        loadingBar.SetProcess(1f);
        while (loadingBar.CurrentProcess < 0.99f)
        {
            yield return null;
        }
        loadingBar.SetProcess(0f);
        loadingBarObj.SetActive(false);
        // We change the Cubemap of the Skybox
        currentSkyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));
        currentSkyboxMaterial.SetTexture("_Tex", currentSkyboxCubemap);
        RenderSettings.skybox = currentSkyboxMaterial;

        CreateMaker createMaker = GameObject.Find("CreateMaker").gameObject.GetComponent<CreateMaker>();
        createMaker.Init();
        createMaker.ExitPopup();

        canChangeScreen = true;

        Resources.UnloadUnusedAssets();
        galleryObj.SetActive(true);
    }

    private Texture2D mosaicSource;

    private void Awake()
    {
        //DontDestroyOnLoad(this);
    }

    public void BackToHome()
    {
        //SceneManager.LoadScene(sceneName: "PortalScreen");
        SceneManager.LoadPreviousScene();
    }

    private IEnumerator ApplyMosaic(Texture2D image)
    {
        List<Region> listRegion = new List<Region>();
        foreach (Maker maker in SceneManager.currentPanorama.markers)
        {
            if (maker.hidden)
            {
                continue;
            }

            if ("mosaic".Equals(maker.type))
            {
                listRegion.Add(maker.region);
            }
        }
        return Blur(image, listRegion, 20);
    }

    private IEnumerator Blur(Texture2D image, List<Region> rectangles, int blurSize)
    {
        Texture2D blurred = image;
        foreach (Region rectangle in rectangles)
        {
            rectangle.y = image.height - rectangle.y;
            // look at every pixel in the blur rectangle
            for (int xx = rectangle.x; xx < rectangle.x + rectangle.width; xx++)
            {
                for (int yy = rectangle.y - rectangle.height; yy < rectangle.y; yy++)
                {
                    float avgR = 0, avgG = 0, avgB = 0, avgA = 0; ;
                    int blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.height); y++)
                        {
                            Color pixel = blurred.GetPixel(x, y);

                            avgR += pixel.r;
                            avgG += pixel.g;
                            avgB += pixel.b;
                            avgA += pixel.a;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;
                    avgA = avgA / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    //for (int x = xx; x < xx + blurSize && x < image.width ; x++)
                    //for (int y = yy; y < yy + blurSize && y < image.height ; y++)
                    blurred.SetPixel(xx, yy, new Color(avgR, avgG, avgB, avgA));
                }
                yield return null;
            }
        }
        blurred.Apply();

        mosaicSource = blurred;
    }
}