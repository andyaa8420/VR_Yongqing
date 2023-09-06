using UnityEngine;
using UnityEngine.UI;

namespace TWV.Scenes
{
    public class VrExample : MonoBehaviour
    {
        [SerializeField]
        private TextureWebView _webView = null;

        [Header("Global")]
        [SerializeField]
        private Text _loadStateText = null;

        [SerializeField]
        private Text _sizeText = null;

        [SerializeField]
        private Text _posText = null;

        [SerializeField]
        private Text _elemInfoText = null;

        [Header("Navigation Group")]
        [SerializeField]
        private Button _backButton = null;

        [SerializeField]
        private Button _forwardButton = null;

        [SerializeField]
        private Button _scrollUpButton = null;

        [SerializeField]
        private Button _scrollDownButton = null;

        private bool _vKVisible = false;

        private void Start()
        {
            _loadStateText.gameObject.SetActive(false);
            _sizeText.gameObject.SetActive(false);
            _posText.gameObject.SetActive(false);
            _elemInfoText.gameObject.SetActive(false);

            _webView.PageStartedListener += OnPageStarted;
            _webView.PageLoadingListener += OnPageLoading;
            _webView.PageFinishedListener += OnPageFinished;
            _webView.PageImageReadyListener += OnPageImageReady;

            _webView.MotionEventsListener += OnMotionEvents;

            _backButton.onClick.AddListener(OnBackClick);
            _forwardButton.onClick.AddListener(OnForwardClick);
            _scrollUpButton.onClick.AddListener(OnScrollUp);
            _scrollDownButton.onClick.AddListener(OnScrollDown);
        }

        #region WebView Callbacks
        private void OnPageStarted(string pageUrl)
        {
            _loadStateText.gameObject.SetActive(true);
            _sizeText.gameObject.SetActive(false);
            _posText.gameObject.SetActive(false);
            _elemInfoText.gameObject.SetActive(false);
        }

        private void OnPageLoading(int percentage)
        {
            _loadStateText.text = string.Format("Loading: {0}%", percentage);
        }

        private void OnPageFinished(string pageUrl)
        {
            _loadStateText.gameObject.SetActive(false);
            _sizeText.gameObject.SetActive(true);
            _posText.gameObject.SetActive(true);
        }

        private void OnPageImageReady(Texture2D pageImage)
        {
            _sizeText.text = string.Format("Size: {0}, {1}", _webView.Width, _webView.Height);
        }

        private void OnMotionEvents(MotionActions action, Vector2 coord)
        {
            _posText.text = string.Format("Pos: {0}, {1}", coord.x, coord.y);

            // Convert to web page coord
            var pageCoord = new Vector2(coord.x / _webView.DisplayDensity, coord.y / _webView.DisplayDensity);

            if (action == MotionActions.Ended)
            {
                // Check if clicked on input field component
                _webView.EvaluateJavascript(string.Format("return (function() {{" +
                        "elem = document.elementFromPoint({0},{1});" +
                        "return elem.tagName; }})()", pageCoord.x, pageCoord.y), (result, error) =>
                        {
                            if (string.IsNullOrEmpty(error))
                            {
                                if (!string.IsNullOrEmpty(result))
                                {
                                    if (result.ToLower().Contains("input"))
                                    {
                                        _vKVisible = true;
                                        ShowVirtualKeyboard();
                                    }
                                    else if (_vKVisible)
                                    {
                                        _vKVisible = false;
                                        HideVirtualKeyboard();
                                    }
                                }
                            }
                            else
                                Debug.LogError(string.Format("Can't get clicked component tag, because: {0}", error));
                        });
            }
        }
        #endregion

        /// <summary>
        /// Example of setting the text to current active input field on web page
        /// </summary>
        /// <param name="text">Text that will be setting to input form</param>
        private void SetInputFieldTextScript(string text)
        {
            _webView.EvaluateJavascript(string.Format("return (function() {{" +
                "elem = document.activeElement;" +
                "elem.value = '{0}'; }})()", text), (result, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError(string.Format("Can't set input field text, because: {0}", error));
                });
        }

        private void ShowVirtualKeyboard()
        {
            SetInputFieldTextScript("Example text" + Random.Range(0, 100));
            Debug.Log("ShowVirtualKeyboard");
        }

        private void HideVirtualKeyboard()
        {
            Debug.Log("HideVirtualKeyboard");
        }

        public void OnBackClick()
        {
            if (_webView != null && _webView.MoveBack())
                Debug.Log("WebView go on previous page.");
            else
                Debug.LogError("WebView does not have previous page.");
        }

        public void OnForwardClick()
        {
            if (_webView != null && _webView.MoveForward())
                Debug.Log("WebView go on forward page.");
            else
                Debug.LogError("WebView does not have forward page.");
        }

        public void OnScrollUp()
        {
            if (_webView != null)
                _webView.ScrollBy(Vector2.up * _webView.Height * 0.5f);
        }

        public void OnScrollDown()
        {
            if (_webView != null)
                _webView.ScrollBy(Vector2.down * _webView.Height * 0.5f);
        }
    }
}
