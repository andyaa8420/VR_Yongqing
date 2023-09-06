using System.Collections;
using System.Collections.Generic;
using TWV;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateMaker : MonoBehaviour
{

    [SerializeField] GameObject MakerTemplate;

    [SerializeField] GameObject TagTemplate;

    [SerializeField] GameObject VideoController;

    [SerializeField] GameObject AudioController;

    [SerializeField] GameObject GalleryTemplate;

    [SerializeField] GameObject MultiTagTemplate;

    [SerializeField] GameObject WebView;

    [SerializeField] GameObject PreviewTemplate;

    private const float MakerDistanceScale = 0.1f;

    private const float MakerSizeScale = 20.5f;

    public void Init()
    {
        print("CreateMaker:Start");

        foreach (Maker maker in SceneManager.currentPanorama.markers)
        {
            if (maker.hidden || maker.disabled)
            {
                continue;
            }

            if ("sticker".Equals(maker.type)
                || "point".Equals(maker.type)
                || "teleport".Equals(maker.type)
                || "popup".Equals(maker.type)
                || "tag".Equals(maker.type)
                || "multitag".Equals(maker.type)
                || "videoplay".Equals(maker.type)
                || "narration".Equals(maker.type)
                || "gallery".Equals(maker.type))
            {
                // Instantiate dynamically
                GameObject gameObject = GameObject.Instantiate(MakerTemplate);

                // Set the parent of the new button 
                gameObject.transform.SetParent(transform.parent);

                SetContent(gameObject, maker);
                MakerScript makerScript =
                    gameObject.GetComponent<MakerScript>();

                makerScript.AddPointerEnterListener((eventData) => {
                    OnPointerEnter(gameObject, maker, eventData);
                });
                makerScript.AddPointerExitListener((eventData) => {
                    OnPointerExit(gameObject, maker, eventData);
                });
            }
        }
    }

    public void OnPointerEnter(GameObject gameObject, Maker maker, PointerEventData eventData)
    {
        if ("point".Equals(maker.type)
        || "teleport".Equals(maker.type))
        {
            // destroy popup
            GameObject[] makers = GameObject.FindGameObjectsWithTag("Preview");

            for (int i = 0; i < makers.Length; i++)
            {
                DestroyImmediate(makers[i]);
            }

            Panorama panorama = GetPanorama(maker);

            if (panorama != null)
            {
                GameObject tagObject = GameObject.Instantiate(PreviewTemplate);

                // Set the parent of the new button 
                tagObject.transform.SetParent(transform.parent);

                PreviewController previewController = tagObject.transform.Find("PreviewController").gameObject.GetComponent<PreviewController>();

                previewController.SetInfo(panorama);


                // set position
                Vector3 position = new Vector3();
                position.x = (float)maker.position.x * MakerDistanceScale * 0.8f;
                position.y = (float)maker.position.y * MakerDistanceScale * 0.8f;
                position.z = -(float)maker.position.z * MakerDistanceScale * 0.8f;
                tagObject.transform.position = position;

                SetRotation(tagObject);
            }
        }

        // set opacity
        if (gameObject.GetComponent<MeshRenderer>().material.HasProperty("_TintColor"))
        {
            Color newColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
        }
    }

    public void OnPointerExit(GameObject gameObject, Maker maker, PointerEventData eventData)
    {
        if ("point".Equals(maker.type)
        || "teleport".Equals(maker.type))
        {
            // destroy popup
            GameObject[] makers = GameObject.FindGameObjectsWithTag("Preview");

            for (int i = 0; i < makers.Length; i++)
            {
                DestroyImmediate(makers[i]);
            }
            Resources.UnloadUnusedAssets();
        }
        // set opacity
        if (gameObject.GetComponent<MeshRenderer>().material.HasProperty("_TintColor"))
        {
            Color newColor = new Color(0.5f, 0.5f, 0.5f, 0.5f * (float)maker.opacity / 100f);
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
        }
    }


    private static void SetRotation(GameObject gameObject)
    {
        gameObject.transform.LookAt(2 * gameObject.transform.position);
    }

    private void SetContent(GameObject gameObject, Maker maker)
    {
        Texture2D texture2d = null;
        UnityEngine.Video.VideoPlayer videoPlayer = null;
        if (maker.useCustomIcon)
        {
            print("CreateMaker:url" + maker.iconUrl);
            if ("videoplay".Equals(maker.type))
            {
                videoPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
                videoPlayer.playOnAwake = false;
                videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.MaterialOverride;
                MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                renderer.enabled = true;
                videoPlayer.targetMaterialRenderer = gameObject.GetComponent<Renderer>();
                videoPlayer.targetMaterialProperty = "_MainTex";
                videoPlayer.isLooping = true;
                videoPlayer.url = maker.actionLink;
                videoPlayer.SetDirectAudioMute(0, maker.videoplayAttribute.mute);
                gameObject.SetActive(true);
                if (maker.videoplayAttribute.autoplay)
                {
                    videoPlayer.Play();
                }

                videoPlayer.prepareCompleted += (source) =>
                {
                    SetVideoSize(gameObject, maker, videoPlayer.width, videoPlayer.height);
                };
            } else
            {
                Davinci data = Davinci.get();
                data.load(maker.iconUrl)
                    .setCached(true)
                    .withLoadedAction(() =>
                    {
                        print("withLoadedAction");
                        //Load Image
                        texture2d = data.rawTexture2D;
                        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                        renderer.material.mainTexture = texture2d;
                        renderer.material.shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
                        renderer.enabled = true;

                        SetImageSize(gameObject, maker, texture2d.width, texture2d.height);
                    })
                    .start();
            }
        }
        else
        {
            string path = string.Format("Image/{0}", maker.iconType);
            if ("teleport".Equals(maker.type))
            {
                path = string.Format("Image/{0}", "teleport");
            }

            texture2d =  Resources.Load<Texture2D>(path);

            if (texture2d != null)
            {
                MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                renderer.material.mainTexture = texture2d;
                renderer.material.shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
                renderer.enabled = true;

                SetImageSize(gameObject, maker, texture2d.width, texture2d.height);
            }
        }

        // set position
        Vector3 position = new Vector3();
        position.x = (float)maker.position.x * MakerDistanceScale;
        position.y = (float)maker.position.y * MakerDistanceScale;
        position.z = - (float)maker.position.z * MakerDistanceScale;
        gameObject.transform.position = position;
        
        //set rotation 
        if (maker.customCoordinate)
        {
            if (maker.coordinatePlane == 1)
            {
                gameObject.transform.eulerAngles = new Vector3(0f, - (float)maker.coordinateSpin - 90f, 0f);
            }
            else if (maker.coordinatePlane == 2)
            {
                gameObject.transform.eulerAngles = new Vector3(0f, - (float)maker.coordinateSpin, 0f);
            }
            else if (maker.coordinatePlane == 3)
            {
                gameObject.transform.Rotate(Vector3.down, (float)maker.rotation.y);
                gameObject.transform.Rotate(Vector3.right, 90f);
            }

        }
        else
        {
            SetRotation(gameObject);
        }

        // set opacity
        if (gameObject.GetComponent<MeshRenderer>().material.HasProperty("_TintColor"))
        {
            Color newColor = new Color(0.5f, 0.5f, 0.5f, 0.5f * (float)maker.opacity / 100f);
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
        }
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            OnClick(maker, gameObject);
        });
        if (maker.hidden)
        {
            gameObject.SetActive(false);
        } else
        {
            gameObject.SetActive(true);
        }
    }

    private void SetImageSize(GameObject gameObject, Maker maker, float width, float height)
    {
        float scaleX = (float)(maker.tagsizePercent / 100) / width * MakerSizeScale;
        float scaleY = (float)(maker.tagsizePercent / 100) / height * MakerSizeScale;
        float scale = scaleX > scaleY ? scaleY : scaleX;
        scale *= MakerDistanceScale;
        gameObject.transform.localScale = new Vector3((float)scale * width, (float)scale * height, 1);
    }

    private void SetVideoSize(GameObject gameObject, Maker maker, float width, float height)
    {
        float scaleX = (float)(maker.tagsizePercent / 100) / width * MakerSizeScale * 10f;
        float scaleY = (float)(maker.tagsizePercent / 100) / (width * (float)9.6 / (float)16.0) * MakerSizeScale * 10f;
        float scale = scaleX > scaleY ? scaleY : scaleX;
        scale *= MakerDistanceScale;
        gameObject.transform.localScale = new Vector3((float)scale * width, (float)scale * (width * (float)9.6 / (float)16.0), 1);
    }

    private void OnClick(Maker maker, GameObject gameObject)
    {
        // destroy popup
        GameObject[] makers = GameObject.FindGameObjectsWithTag("Popup");

        for (int i = 0; i < makers.Length; i++)
        {
            DestroyImmediate(makers[i]);
        }

        if (maker.disabled)
        {
            return;
        }
        print("CreateMaker: OnClick -> point " + maker.objectId);
        if ("point".Equals(maker.type))
        {
            OnPointClick(maker);
        }
        else if ("teleport".Equals(maker.type))
        {
            OnTeleportClick(maker);
        }
        else if ("popup".Equals(maker.type))
        {
            OnPopupClick(maker);
        }
        else if ("tag".Equals(maker.type))
        {
            OnTagClick(maker);
        }
        else if ("videoplay".Equals(maker.type))
        {
            OnVideoPlayClick(maker, gameObject);
        }
        else if ("narration".Equals(maker.type))
        {
            OnAudioPlayClick(maker, gameObject);
        }
        else if ("gallery".Equals(maker.type))
        {
            OnGalleryPlayClick(maker, gameObject);
        }
        else if ("multitag".Equals(maker.type))
        { 
            OnMultiTagClick(maker);
        }
    }

    private Panorama GetPanorama(Maker maker)
    {
        Panorama panorama = null;
        foreach (Panorama p in SceneManager.currentBuilding.panoramas)
        {
            if (p.objectId.Equals(maker.nextPanoramaId))
            {
                panorama = p;
                break;
            }
        }

        if (panorama == null)
        {
            Building building = GetDataService.Instance.GetBuildingData(maker.nextBuildingId);
            if (building == null)
            {
                return panorama;
            }
            foreach (Panorama p in building.panoramas)
            {
                if (p.objectId.Equals(maker.nextPanoramaId))
                {
                    panorama = p;
                    break;
                }
            }
        }

        return panorama;
    }

    private void OnPointClick(Maker maker)
    {
        Panorama panorama = GetPanorama(maker);
        print("OnPointClick: OnPointClick -> panorama " + panorama);

        if (panorama != null)
        {
            SceneManager.currentPanorama = panorama;
            LoadImageToSkybox lt = GameObject.Find("LiveTourManager").GetComponent<LoadImageToSkybox>();
            lt.StartCoroutine(lt.InitScreen());
        }
    }

    private void OnTeleportClick(Maker maker)
    {
        Panorama panorama = GetPanorama(maker);

        if (panorama != null)
        {
            SceneManager.currentPanorama = panorama;
            LoadImageToSkybox lt = GameObject.Find("LiveTourManager").GetComponent<LoadImageToSkybox>();
            lt.StartCoroutine(lt.InitScreen());
        }
    }

    private void OnGalleryPlayClick(Maker maker, GameObject gameObject)
    {
        GameObject galleryObject = GameObject.Instantiate(GalleryTemplate);
        galleryObject.transform.SetParent(transform.parent);

        Davinci data = Davinci.get();
        data.load(maker.photo)
        .setCached(true)
        .withLoadedAction(() =>
        {
            RectTransform rect = galleryObject.transform.Find("GameObject").GetComponent<RectTransform>();
            RawImage photoDownload = galleryObject.transform.Find("GameObject").Find("RawImage").GetComponent<RawImage>();
            photoDownload.texture = data.rawTexture2D;
            int width, height;
            if (data.rawTexture2D.width > data.rawTexture2D.height)
            {
                width = 600;
                height = data.rawTexture2D.height * 600 / data.rawTexture2D.width;
            } else
            {
                height = 400;
                width = data.rawTexture2D.width * 400 / data.rawTexture2D.height;
            }
            photoDownload.rectTransform.sizeDelta = new Vector2(width, height);
            rect.sizeDelta = new Vector2(width, height);
        }
        )
        .start();

        GameObject scaleUpBtn = galleryObject.transform.Find("ScaleUp").gameObject;
        scaleUpBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            RawImage photoDownload = galleryObject.transform.Find("GameObject").Find("RawImage").GetComponent<RawImage>();
            Vector3 currentScale = photoDownload.transform.localScale;
            if (currentScale.x < 9)
            {
                currentScale.x = currentScale.x + (float)2;
                currentScale.y = currentScale.y + (float)2;
                photoDownload.transform.localScale = currentScale;
            }
        });

        GameObject scaleDownBtn = galleryObject.transform.Find("ScaleDown").gameObject;
        scaleDownBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            RawImage photoDownload = galleryObject.transform.Find("GameObject").Find("RawImage").GetComponent<RawImage>();
            Vector3 currentScale = photoDownload.transform.localScale;
            if (currentScale.x > 1)
            {
                currentScale.x = currentScale.x - (float)2;
                currentScale.y = currentScale.y - (float)2;
                photoDownload.transform.localScale = currentScale;
            }
        });

        GameObject exitBtn = galleryObject.transform.Find("ExitBtn").gameObject;
        exitBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            DestroyImmediate(galleryObject);
            Resources.UnloadUnusedAssets();
        });

        Text nameTitle = galleryObject.transform.Find("Name").GetComponent<Text>();
        nameTitle.text = maker.name;

        GameObject categoryObj = galleryObject.transform.Find("Category").gameObject;

        foreach (string stringData in maker.category)
        {
            GameObject newGO = new GameObject("myTextGO");
            newGO.transform.SetParent(categoryObj.transform);
            newGO.transform.localScale = new Vector3(1, 1, 1);
            var newTextComp = newGO.AddComponent<Text>();
            newTextComp.text = stringData;
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            newTextComp.font = font;
            newTextComp.fontSize = 21;
        }
    }

    private void OnAudioPlayClick(Maker maker, GameObject gameObject)
    {
        GameObject audioControllerObject = GameObject.Instantiate(AudioController);
        audioControllerObject.transform.SetParent(transform.parent);

        bl_DownloadAudio audioDownload = audioControllerObject.transform.Find("Pro Audio Player").GetComponent<bl_DownloadAudio>();
        bl_APAudioWeb data = new bl_APAudioWeb();
        data.AudioTitle = maker.name;
        data.URL = maker.actionLink;
        data.m_AudioType = AudioType.MPEG;
        audioDownload.AudioURLs.Add(data);
        audioDownload.StartDownload();

        bl_AudioPlayer audioPlayer = audioControllerObject.transform.Find("Pro Audio Player").GetComponent<bl_AudioPlayer>();
        audioPlayer.Play();

        GameObject exitBtn = audioControllerObject.transform.Find("ExitBtn").gameObject;
        exitBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            DestroyImmediate(audioControllerObject);
            Resources.UnloadUnusedAssets();
        });
    }

    private void OnVideoPlayClick(Maker maker, GameObject gameObject)
    {
        GameObject videoControllerObject = GameObject.Instantiate(VideoController);

        UnityEngine.Video.VideoPlayer videoPlayer = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();
        VideoController videoController = videoControllerObject.transform.Find("VideoController").GetComponent<VideoController>();
        videoController.videoPlayer = videoPlayer;

        Vector3 position = new Vector3();
        position.x = (float)gameObject.transform.position.x;
        position.y = (float)gameObject.transform.position.y+2;
        if (gameObject.transform.position.z > 0)
        {
            position.z = (float)gameObject.transform.position.z-(float)0.5;
        }else
        {
            position.z = (float)gameObject.transform.position.z + (float)0.5;
        }
        videoControllerObject.transform.position = position;
        videoControllerObject.transform.rotation = gameObject.transform.rotation;
        videoControllerObject.transform.SetParent(transform.parent);
        GameObject exitBtn = videoControllerObject.transform.Find("ExitBtn").gameObject;
        exitBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            DestroyImmediate(videoControllerObject);
            Resources.UnloadUnusedAssets();
        });
    }

private void OnTagClick(Maker maker)
{
    GameObject tagObject = GameObject.Instantiate(TagTemplate);

    // Set the parent of the new button 
    tagObject.transform.SetParent(transform.parent);

    TagController tagController = tagObject.transform.Find("TagController").gameObject.GetComponent<TagController>();

    tagController.SetInfo(maker);
    tagController.AddActionLinkListener((actionLink) => {
        DestroyImmediate(tagObject);
        OnPopupClick(actionLink);
    });
    tagController.AddExitListener(() =>
    {
        DestroyImmediate(tagObject);
        Resources.UnloadUnusedAssets();
    });
}

private void OnMultiTagClick(Maker maker)
{
    GameObject tagObject = GameObject.Instantiate(MultiTagTemplate);

    // Set the parent of the new button 
    tagObject.transform.SetParent(transform.parent);

    MultiTagController tagController = tagObject.transform.Find("MultiTagController").gameObject.GetComponent<MultiTagController>();

    tagController.SetInfo(maker);
    tagController.AddActionLinkListener((actionLink) => {
        DestroyImmediate(tagObject);
        OnPopupClick(actionLink);
    });
    tagController.AddExitListener(() =>
    {
        DestroyImmediate(tagObject);
        Resources.UnloadUnusedAssets();
    });
}

public GameObject exitBtn;

    private void OnPopupClick(Maker maker)
    {
        //maker.actionLink
        TextureWebView uniWebView = GameObject.Find("TextureWebView").GetComponent<TextureWebView>();
        WebView.SetActive(true);
        if (uniWebView != null)
        {
            uniWebView.Load(maker.actionLink);
        }
        exitBtn.SetActive(true);
    }

    private void OnPopupClick(string actionLink)
    {
        //maker.actionLink
        TextureWebView uniWebView = GameObject.Find("TextureWebView").GetComponent<TextureWebView>();
        WebView.SetActive(true);
        uniWebView.Load(actionLink);
        exitBtn.SetActive(true);
    }


    public void ExitPopup()
    {
        TextureWebView uniWebView = GameObject.Find("TextureWebView").GetComponent<TextureWebView>();
        WebView.SetActive(false);
        if (uniWebView != null)
        {
            uniWebView.UnLoad();
        }
        exitBtn.SetActive(false);
        Resources.UnloadUnusedAssets();
    }
}
