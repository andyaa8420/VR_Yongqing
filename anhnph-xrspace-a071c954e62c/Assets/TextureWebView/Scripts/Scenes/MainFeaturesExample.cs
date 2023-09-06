using System;
using UnityEngine;
using UnityEngine.UI;

namespace TWV.Scenes
{
    public class MainFeaturesExample : MonoBehaviour
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

        [SerializeField]
        private Button _showPanelButton = null;

        [SerializeField]
        private GameObject _mainPanel = null;

        [Header("Load Group")]
        [SerializeField]
        private InputField _urlField = null;

        [SerializeField]
        private Button _loadButton = null;

        [Header("Navigation Group")]
        [SerializeField]
        private Button _backButton = null;

        [SerializeField]
        private Button _forwardButton = null;

        [SerializeField]
        private Button _scrollUpButton = null;

        [SerializeField]
        private Button _scrollDownButton = null;

        [Header("Change Size Group")]
        [SerializeField]
        private Button _changeSizeButton = null;

        [SerializeField]
        private InputField _sizeXField = null;

        [SerializeField]
        private InputField _sizeYField = null;

        [Header("Click Group")]
        [SerializeField]
        private Button _clickButton = null;

        [SerializeField]
        private InputField _xCoordField = null;

        [SerializeField]
        private InputField _yCoordField = null;

        [Header("Inject Script Group")]
        [SerializeField]
        private InputField _scriptField = null;

        [SerializeField]
        private Button _injectButton = null;

        [SerializeField]
        private Text _injectResultText = null;

        private void Start()
        {
            _loadStateText.gameObject.SetActive(false);
            _sizeText.gameObject.SetActive(false);
            _posText.gameObject.SetActive(false);
            _elemInfoText.gameObject.SetActive(false);
            _mainPanel.SetActive(false);
            _injectResultText.gameObject.SetActive(false);
            _showPanelButton.gameObject.SetActive(false);

            _webView.PageStartedListener += OnPageStarted;
            _webView.PageLoadingListener += OnPageLoading;
            _webView.PageFinishedListener += OnPageFinished;
            _webView.PageImageReadyListener += OnPageImageReady;

            _webView.MotionEventsListener += OnMotionEvents;

            _showPanelButton.onClick.AddListener(OnShowPanel);

            _loadButton.onClick.AddListener(OnLoadClick);

            _backButton.onClick.AddListener(OnBackClick);
            _forwardButton.onClick.AddListener(OnForwardClick);
            _scrollUpButton.onClick.AddListener(OnScrollUp);
            _scrollDownButton.onClick.AddListener(OnScrollDown);

            _changeSizeButton.onClick.AddListener(OnChangeSize);
            _clickButton.onClick.AddListener(OnClick);
            _injectButton.onClick.AddListener(OnInject);
        }

        #region WebView Callbacks
        private void OnPageStarted(string pageUrl)
        {
            _loadStateText.gameObject.SetActive(true);
            _sizeText.gameObject.SetActive(false);
            _posText.gameObject.SetActive(false);
            _elemInfoText.gameObject.SetActive(false);
            _showPanelButton.gameObject.SetActive(false);
            _mainPanel.SetActive(false);
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
            _showPanelButton.gameObject.SetActive(true);
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

            _webView.EvaluateJavascript(string.Format("return (function() {{" +
                    "elem = document.elementFromPoint({0},{1});" +
                    "return elem.name + '@' + elem.tagName; }})()", pageCoord.x, pageCoord.y), (result, error) =>
                    {
                        if (string.IsNullOrEmpty(error))
                        {
                            if (!string.IsNullOrEmpty(result))
                            {
                                _elemInfoText.text = string.Empty;

                                var resultInfos = result.Split('@');
                                _elemInfoText.text += "Elem Name: " + resultInfos[0] + "\n";
                                _elemInfoText.text += "Elem TagName: " + resultInfos[1] + "\n";

                                _elemInfoText.gameObject.SetActive(true);
                            }
                        }
                        else
                            Debug.LogError(string.Format("Can't get page element information, because: {0}", error));
                    });
        }
        #endregion

        public void OnShowPanel()
        {
            _mainPanel.SetActive(!_mainPanel.activeSelf);
        }

        public void OnLoadClick()
        {
            if (_webView != null)
            {
                if (!string.IsNullOrEmpty(_urlField.text))
                    _webView.Load(_urlField.text);
                else
                    Debug.LogError("Can't load page, because url is not correct");
            }
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

        public void OnChangeSize()
        {
            var newWidth = _webView.Width;
            var newHeight = _webView.Height;

            try
            {
                newWidth = int.Parse(_sizeXField.text);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Unable to convert width value: '{0}'.", e));
            }

            try
            {
                newHeight = int.Parse(_sizeYField.text);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Unable to convert height value: '{0}'.", e));
            }

            if (_webView != null)
                _webView.ChangeSize(newWidth, newHeight);
        }

        public void OnClick()
        {
            var xCoord = 0;
            var yCoord = 0;

            try
            {
                xCoord = int.Parse(_xCoordField.text);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Unable to convert x coord value: '{0}'.", e));
            }

            try
            {
                yCoord = int.Parse(_yCoordField.text);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Unable to convert y coord value: '{0}'.", e));
            }


            if (_webView != null)
                _webView.ClickTo(new Vector2(xCoord, yCoord));
        }

        public void OnInject()
        {
            if (!string.IsNullOrEmpty(_scriptField.text))
            {
                if (_webView != null)
                    _webView.EvaluateJavascript(_scriptField.text, (result, error) => {
                        if (string.IsNullOrEmpty(error))
                        {
                            if (!string.IsNullOrEmpty(result))
                            {
                                _injectResultText.gameObject.SetActive(true);
                                _injectResultText.text = string.Format("Result: {0}", result);
                            }
                            else
                            {
                                _injectResultText.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            _injectResultText.gameObject.SetActive(true);
                            _injectResultText.text = string.Format("Error: {0}", error);
                        }
                    });
            }
            else
                Debug.LogError("Can't inject current script, because it's incorrect.");
        }
    }
}
