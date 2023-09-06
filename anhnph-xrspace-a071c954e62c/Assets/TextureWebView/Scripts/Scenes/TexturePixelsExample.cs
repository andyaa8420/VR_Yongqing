using UnityEngine;
using UnityEngine.UI;

namespace TWV.Scenes
{
    public class TexturePixelsExample : MonoBehaviour
    {
        [SerializeField]
        private RawImage _image = null;

        [SerializeField]
        private TextureWebView _webView = null;

        private Texture2D _pixelsTexture = null;

        private void Start()
        {
            _webView.PageImageReadyListener += OnImageReady;
        }

        private void Update()
        {
            if (_pixelsTexture != null)
            {
                _pixelsTexture.LoadRawTextureData(_webView.FramePixels);
                _pixelsTexture.Apply();
            }
        }

        private void OnImageReady(Texture2D viewTexture)
        {
            _pixelsTexture = new Texture2D(viewTexture.width, viewTexture.height, TextureFormat.RGBA32, false);
            _image.texture = _pixelsTexture;
        }
    }
}
