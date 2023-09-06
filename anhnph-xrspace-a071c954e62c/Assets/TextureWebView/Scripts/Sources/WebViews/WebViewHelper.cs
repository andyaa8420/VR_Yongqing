using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace TWV
{
    public class WebViewHelper
    {
        public const string LOCAL_FILE_ROOT = "file:///";
        private static Regex _androidStorageRoot = new Regex("(^\\/.*)Android");

        /// <summary>
        /// Generate correct texture for current runtime platform
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <returns></returns>
        internal static Texture2D GenWebViewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        /// <summary>
        /// Apply texture to Unity game object that has 'RawImage' or 'MeshRenderer' component
        /// </summary>
        /// <param name="texture">Texture to render video output</param>
        /// <param name="renderingObject">Game object where will be rendering video output</param>
        /// <returns></returns>
        public static void ApplyTextureToRenderingObject(Texture2D texture, GameObject renderingObject)
        {
            if (renderingObject == null)
                return;

            var rawImage = renderingObject.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = texture;
                return;
            }

            var meshRenderer = renderingObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null && meshRenderer.material != null)
                meshRenderer.material.mainTexture = texture;
            else
                Debug.LogError(renderingObject.name + ": don't have 'RawImage' or 'MeshRenderer' component - ignored");
        }

        public static bool HasMeshRenderer(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            return meshRenderer != null;
        }

        /// <summary>
        /// Get size in pixels of gameobject that has "MeshRenderer" or "RawImage" component
        /// </summary>
        /// <param name="meshRenderer">Gameobject that has "MeshRenderer" or "RawImage" component</param>
        /// <param name="camera">Main camera of current scene</param>
        /// <returns></returns>
        public static Vector2 GetPixelSizeOfObject(GameObject obj, Camera camera)
        {
            if (obj == null)
                return Vector2.zero;

            if (HasMeshRenderer(obj))
            {
                if (camera == null)
                    return Vector2.zero;
                else
                {
                    var meshRenderer = obj.GetComponent<MeshRenderer>();
                    return GetPixelSizeOfMeshRenderer(meshRenderer, camera);
                }
            }
            else
            {
                var imageRenderer = obj.GetComponent<RawImage>();

                if (imageRenderer != null)
                    return GetPixelSizeOfRawImage(imageRenderer);
                else
                    return Vector2.zero;
            }
        }

        /// <summary>
        /// Get size in pixels of gameobject that have "MeshRenderer" component
        /// </summary>
        /// <param name="meshRenderer">Gameobject that has "MeshRenderer" component</param>
        /// <param name="camera">Main camera of current scene</param>
        /// <returns></returns>
        public static Vector2 GetPixelSizeOfMeshRenderer(MeshRenderer meshRenderer, Camera camera)
        {
            if (meshRenderer == null)
                return Vector2.zero;

            Vector3 posStart = camera.WorldToScreenPoint(new Vector3(meshRenderer.bounds.min.x, meshRenderer.bounds.min.y, meshRenderer.bounds.min.z));
            Vector3 posEnd = camera.WorldToScreenPoint(new Vector3(meshRenderer.bounds.max.x, meshRenderer.bounds.max.y, meshRenderer.bounds.min.z));

            float widthX = Mathf.Abs((posEnd.x - posStart.x));
            float widthY = Mathf.Abs((posEnd.y - posStart.y));

            return new Vector2((int)widthX, (int)widthY);
        }

        /// <summary>
        /// Get size in pixels of gameobject that have "RawImage" component
        /// </summary>
        /// <param name="rawImage">Gameobject that has "RawImage" component</param>
        /// <returns></returns>
        public static Vector2 GetPixelSizeOfRawImage(RawImage rawImage)
        {
            if (rawImage == null)
                return Vector2.zero;

            return new Vector2(rawImage.rectTransform.rect.width, rawImage.rectTransform.rect.height);
        }

        /// <summary>
        /// Getting average color from frame buffer array
        /// </summary>
        /// <returns></returns>
        public static Color GetAverageColor(byte[] frameBuffer)
        {
            if (frameBuffer == null)
                return Color.black;

            long redBucket = 0;
            long greenBucket = 0;
            long blueBucket = 0;
            long alphaBucket = 0;
            int pixelCount = frameBuffer.Length / 4;

            if (pixelCount <= 0 || pixelCount % 4 != 0)
                return Color.black;

            for (int x = 0; x < frameBuffer.Length; x+=4)
            {
                redBucket += frameBuffer[x];
                greenBucket += frameBuffer[x + 1];
                blueBucket += frameBuffer[x + 2];
                alphaBucket += frameBuffer[x + 3];
            }

            return new Color(redBucket / pixelCount, 
                greenBucket / pixelCount, 
                blueBucket / pixelCount, 
                alphaBucket / pixelCount);
        }

        /// <summary>
        /// Getting colors from frame buffer array
        /// </summary>
        /// <returns></returns>
        public static Color32[] GetFrameColors(byte[] frameBuffer)
        {
            var colorsArray = new Color32[frameBuffer.Length / 4];
            for (var i = 0; i < frameBuffer.Length; i += 4)
            {
                var color = new Color32(frameBuffer[i + 0], frameBuffer[i + 1], frameBuffer[i + 2], frameBuffer[i + 3]);
                colorsArray[i / 4] = color;
            }
            return colorsArray;
        }

        /// <summary>
        /// Getting root storage menory for current platform
        /// Windows, Mac OS, Linux will return path to project folder
        /// Android, iOS will return root of internal/external memory root
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceRootPath()
        {
            Match match = _androidStorageRoot.Match(Application.persistentDataPath);

            if (match.Length > 1)
                return match.Groups[1].Value;

            return Application.persistentDataPath;
        }

        /// <summary>
        /// Check if file exists in 'StreamingAssets' folder
        /// </summary>
        /// <param name="filePath">File name or path in 'StreamingAssets' folder</param>
        /// <returns></returns>
        public static bool IsAssetsFile(string filePath)
        {
            if (filePath.StartsWith(LOCAL_FILE_ROOT))
                filePath = filePath.Substring(LOCAL_FILE_ROOT.Length);

            if (!filePath.Contains(Application.streamingAssetsPath))
                filePath = Path.Combine(Application.streamingAssetsPath, filePath);

            if (Application.platform == RuntimePlatform.Android)
            {
#if UNITY_2017_2_OR_NEWER
                var www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
                www.SendWebRequest();
                while (!www.isDone && www.downloadProgress <= 0) { }
#else
                var www = new WWW(filePath);
                while (!www.isDone && www.progress <= 0) { }
#endif
                bool result = string.IsNullOrEmpty(www.error);
                www.Dispose();

                return result;
            }
            else
                return File.Exists(filePath);
        }

        /// <summary>
        /// Get correct path for local web page file
        /// </summary>
        /// <param name="relativePath">Relative path to the data file</param>
        /// <returns></returns>
        public static string GetLocalSourcePath(string relativePath)
        {
            var isAssetsFile = false;

            if (relativePath.StartsWith(LOCAL_FILE_ROOT))
            {
                relativePath = relativePath.Substring(LOCAL_FILE_ROOT.Length);
                isAssetsFile = IsAssetsFile(relativePath);

                if (isAssetsFile)
                {
                    var root = Application.platform == RuntimePlatform.Android ? LOCAL_FILE_ROOT + "android_asset" : Application.streamingAssetsPath;
                    relativePath = Path.Combine(root, relativePath);
                }
            }

            if (!isAssetsFile)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    var pathDR = Path.Combine(GetDeviceRootPath(), relativePath);

                    if (File.Exists(pathDR))
                        relativePath = pathDR;
                }

                if (File.Exists(relativePath))
                    relativePath = LOCAL_FILE_ROOT + relativePath.TrimStart('/');
                else
                    relativePath = string.Empty;
            }

            return relativePath;
        }

        /// <summary>
        /// Check if the runtime platforms is supported by MWV asset
        /// </summary>
        /// <returns></returns>
        public static bool IsSupportedPlatform
        {
            get
            {
                return Application.platform == RuntimePlatform.Android;
            }
        }
    }
}
