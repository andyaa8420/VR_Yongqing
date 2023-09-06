using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TWV
{
    public class TextureWebView : MonoBehaviour, IPageListener, IErrorListener, IInputManagerListener
    {
        #region Editor Visible Properties
        [SerializeField]
        private GameObject _outputObject = null;

        [SerializeField]
        private Camera _inputCamera = null;

        [SerializeField]
        private EventSystem _eventSystem = null;

        [SerializeField]
        private bool _useHtmlContent = false;

        [SerializeField]
        private string _data = string.Empty;

#pragma warning disable 414
        [SerializeField]
        private bool _showMainOptions = false;
#pragma warning restore 414

        [SerializeField]
        private Vector2 _viewSize = Vector2.one;

        [SerializeField]
        private bool _useScreenSize = false;

        [SerializeField]
        private bool _useObjectSize = false;

        [SerializeField]
        private bool _autoLoad = true;

        [SerializeField]
        private bool _deviceKeyboard = true;

        [SerializeField]
        private InputSystem _inputSystem = InputSystem.Touch;

        [SerializeField]
        private bool _debugRay = false;

        [SerializeField]
        private bool _touchController = false;

#pragma warning disable 414
        [SerializeField]
        private bool _showLayers = false;
#pragma warning restore 414

        [SerializeField]
        private LayerMask _exclusionLayers = new LayerMask();

        [SerializeField]
        private bool _useGaze = false;

        [SerializeField]
        private AGaze _gaze = null;

        [SerializeField]
        private float _gazeSubmitTime = 1;

        [SerializeField]
        private float _gazeSensitivity = 0.1f;

#pragma warning disable 414
        [SerializeField]
        private bool _showEventsOptions = false;
#pragma warning restore 414

        [Serializable]
        private class StringType : UnityEvent<string> { }

        [SerializeField]
        private StringType _pageStartedEvent = new StringType();

        [Serializable]
        private class LoadingType : UnityEvent<int> { }

        [SerializeField]
        private LoadingType _pageLoadingEvent = new LoadingType();

        [SerializeField]
        private StringType _pageFinishedEvent = new StringType();

        [Serializable]
        private class ImageReadyType : UnityEvent<Texture2D> { }

        [SerializeField]
        private ImageReadyType _pageImageReadyEvent = new ImageReadyType();

        [SerializeField]
        private StringType _pageConsoleMessageEvent = new StringType();

        [SerializeField]
        private StringType _pageErrorEvent = new StringType();

        [SerializeField]
        private StringType _pageHttpErrorEvent = new StringType();

        [SerializeField]
        private StringType _errorEvent = new StringType();

        [Serializable]
        private class MotionType : UnityEvent<MotionActions, Vector2> { }

        [SerializeField]
        private MotionType _motionEvents = new MotionType();

#pragma warning disable 414
        [SerializeField]
        private bool _showAboutOption = false;
#pragma warning restore 414
        #endregion

        #region Properties
        /// <summary>
        /// Get/Set Unity 'GameObject' 
        /// that has 'Mesh Renderer' (with some material) 
        /// or 'Raw Image' component
        /// </summary>
        public GameObject OutputObject
        {
            set
            {
                if (_webView != null)
                    _webView.OutputObject = value;

                _outputObject = value;
            }
            get
            {
                return _outputObject;
            }
        }

        /// <summary>
        /// Main scene camera component that used for web view input system
        /// </summary>
        public Camera InputCamera
        {
            get
            {
                return _inputCamera;
            }
        }

        /// <summary>
        /// Auto load current web view component if it's enable
        /// </summary>
        public bool AutoLoad
        {
            set { _autoLoad = value; }
            get { return _autoLoad; }
        }

        /// <summary>
        /// Gets the URL for the current page. This is not always the same as the URL passed to Load, 
        /// because the current page can be changed.
        /// </summary>
        public string PageUrl
        {
            get
            {
                if (_webView != null)
                    return _webView.PageUrl;

                return null;
            }
        }

        /// <summary>
        /// Is web page ready to use and output texture is available
        /// </summary>
        public bool IsReady
        {
            get
            {
                if (_webView != null)
                    return _webView.IsReady;

                return false;
            }
        }

        /// <summary>
        /// Get pixels of current web page frame 
        /// Example of using:
        /// texture.LoadRawTextureData(_player.FramePixels);
        /// texture.Apply(); 
        /// </summary>
        public byte[] FramePixels
        {
            get
            {
                if (_webView != null)
                    return _webView.FramePixels;

                return null;
            }
        }

        /// <summary>
        /// Get The logical density of the display. This is a scaling factor for the 
        /// Density Independent Pixel unit, where one DIP is one pixel on an
        /// approximately 160 dpi screen(for example a 240x320, 1.5"x2" screen), 
        /// providing the baseline of the system's display. Thus on a 160dpi screen 
        /// this density value will be 1; on a 120 dpi screen it would be .75; etc.
        /// </summary>
        public float DisplayDensity
        {
            get
            {
                if (_webView != null)
                    return _webView.DisplayDensity;

                return 1;
            }
        }

        /// <summary>
        /// Get the width of your view, in pixels.
        /// </summary>
        public int Width
        {
            get
            {
                if (_webView != null)
                    return _webView.Width;

                return 0;
            }
        }

        /// <summary>
        /// Get the height of your view, in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                if (_webView != null)
                    return _webView.Height;

                return 0;
            }
        }
        #endregion

        private WebView _webView;
        private InputManager _inputManager;

        private void Awake()
        {
            if (_useScreenSize)
                _viewSize = new Vector2(Screen.width, Screen.height);
            else if (_useObjectSize)
                _viewSize = WebViewHelper.GetPixelSizeOfObject(_outputObject, _inputCamera);

            _webView = new WebView(this, _outputObject, (int)_viewSize.x, (int)_viewSize.y);
            _webView.DeviceKeyboard = _inputSystem != InputSystem.VR ? _deviceKeyboard : false;

            if (_inputSystem == InputSystem.VR && _webView.PlatformWebView is WebViewAndroid)
                (_webView.PlatformWebView as WebViewAndroid).UseLongClick = false;

            _inputManager = new InputManager(this, _inputSystem, _inputCamera, _eventSystem)
            {
                DebugRay = _debugRay,
                TouchController = _inputSystem == InputSystem.Touch ? false : _touchController,
                ExclusionLayers = _exclusionLayers,
                GazeObject = _useGaze ? _gaze : null,
                GazeSubmitTime = _gazeSubmitTime,
                GazeSensitivity = _gazeSensitivity
            };

            AddListeners();
        }

        private void OnEnable()
        {
            _inputManager.StartListener();

            if (!_autoLoad)
                return;

            Load();
        }

        private void OnDisable()
        {
            _inputManager.StopListener();
        }

        private void OnDestroy()
        {
            _inputManager.StopListener();
            _inputManager.RemoveAllEvents();

            if (_webView != null)
            {
                // Release WebView
                Release();
            }
        }

        private void AddListeners()
        {
            if (_webView.EventManager != null)
            {
                // Add to WebView the new main group of listeners
                _webView.AddPageListener(this);
                // Add to WebView the global error listener
                _webView.EventManager.ErrorListener += ((IErrorListener)this).OnError;
                // Add user input listener
                _inputManager.AddInputListener(this);
            }
        }

        private void RemoveListeners()
        {
            if (_webView != null && _webView.EventManager != null)
            {
                // Remove from WebView the main group of listeners
                _webView.RemovePageListener(this);
                // Remove from WebView the global error listener
                _webView.EventManager.ErrorListener -= ((IErrorListener)this).OnError;
                // Remove user input listener
                _inputManager.RemoveInputListener(this);
            }
        }

        /// <summary>
        /// Load new web page based on current page url property
        /// </summary>
        public void Load()
        {
            if (!_useHtmlContent)
                _webView.LoadUrl(_data);
            else
                _webView.LoadData(_data);
        }

        /// <summary>
        /// Load new web page based on current data
        /// </summary>
        /// <param name="data">Url or html content data</param>
        /// <param name="useHtmlContent">Use html data content</param>
        public void Load(string data, bool useHtmlContent = false)
        {
            _data = data;
            _useHtmlContent = useHtmlContent;

            if (!useHtmlContent)
                _webView.LoadUrl(_data);
            else
                _webView.LoadData(_data);
        }

        /// <summary>
        /// Unload current web page
        /// </summary>
        public void UnLoad()
        {
            UnLoad(true);
        }

        /// <summary>
        /// Unload current web page
        /// </summary>
        /// <param name="clearVideoTexture">Clear the last frame</param>
        public void UnLoad(bool clearVideoTexture)
        {
            if (_webView == null)
                return;

            _webView.UnLoad(clearVideoTexture);
        }

        /// <summary>
        /// Release a current web view instance 
        /// </summary>
        public void Release()
        {
            UnLoad();

            if (_webView != null)
            {
                RemoveListeners();

                // Release WebView
                _webView.Release();
                _webView = null;

                _pageStartedEvent.RemoveAllListeners();
                _pageLoadingEvent.RemoveAllListeners();
                _pageFinishedEvent.RemoveAllListeners();
                _pageImageReadyEvent.RemoveAllListeners();
                _pageConsoleMessageEvent.RemoveAllListeners();
                _pageErrorEvent.RemoveAllListeners();
                _pageHttpErrorEvent.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Goes forward in the history of this WebView.
        /// </summary>
        /// <returns>True if this Webview has a forward history item</returns>
        public bool MoveForward()
        {
            if (_webView == null)
                return false;

            return _webView.MoveForward();
        }

        /// <summary>
        /// Goes back in the history of this WebView.
        /// </summary>
        /// <returns>True if this WebView has a back history item</returns>
        public bool MoveBack()
        {
            if (_webView == null)
                return false;

            return _webView.MoveBack();
        }

        /// <summary>
        /// Change the size of view (should be > 0)
        /// </summary>
        /// <param name="width">The new width for current view</param>
        /// <param name="height">The new height for current view</param>
        public void ChangeSize(int width, int height)
        {
            if (_webView != null)
            {
                _webView.ChangeSize(width, height);
                _viewSize = new Vector2(width, height);
            }
        }

        /// <summary>
        /// Set text to current active input form
        /// </summary>
        /// <param name="text">Text that will be set to input form</param>
        public void SetInputText(string text)
        {
            if (_webView != null)
                _webView.SetInputText(text);
        }

        /// <summary>
        /// Asynchronously evaluates JavaScript in the context of the currently displayed page.
        /// </summary>
        /// <param name="script">The JavaScript to execute</param>
        /// <param name="resultCallback">A callback to be invoked when the script execution
        /// completes with the result of the execution(if any).
        /// May be empty if no notification of the result is required.</param>
        public void EvaluateJavascript(string script, JsResultDelegate resultCallback)
        {
            if (_webView != null)
                _webView.EvaluateJavascript(script, resultCallback);
        }

        public void ClickTo(Vector2 position)
        {
            if (_webView != null)
                _webView.ClickTo((int)position.x, (int)position.y);
        }

        public void ScrollBy(Vector2 direction)
        {
            if (_webView != null)
                _webView.ScrollBy((int)direction.x, (int)direction.y, 1f);
        }

        /// <summary>
        /// Notify the host application that a page has started loading
        /// </summary>
        /// <param name="url">The url of the page</param>
        void IPageStartedListener.OnPageStarted(string url)
        {
            if (_pageStartedEvent != null)
                _pageStartedEvent.Invoke(url);
        }

        /// <summary>
        /// Page has started loading listener
        /// </summary>
        public event UnityAction<string> PageStartedListener
        {
            add
            {
                _pageStartedEvent.AddListener(value);
            }
            remove
            {
                _pageStartedEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Tell the host application the current progress of loading a page
        /// </summary>
        /// <param name="percentage">Current progress percentage of loading a page</param>
        void IPageLoadingListener.OnPageLoading(int percentage)
        {
            if (_pageLoadingEvent != null)
                _pageLoadingEvent.Invoke(percentage);
        }

        /// <summary>
        /// The current progress of loading a page listener
        /// </summary>
        public event UnityAction<int> PageLoadingListener
        {
            add
            {
                _pageLoadingEvent.AddListener(value);
            }
            remove
            {
                _pageLoadingEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Notify the host application that a page has finished loading
        /// </summary>
        /// <param name="url">The URL corresponding to the page navigation that triggered this callback</param>
        void IPageFinishedListener.OnPageFinished(string url)
        {
            if (_pageFinishedEvent != null)
                _pageFinishedEvent.Invoke(url);
        }

        /// <summary>
        /// Page has finished loading listener
        /// </summary>
        public event UnityAction<string> PageFinishedListener
        {
            add
            {
                _pageFinishedEvent.AddListener(value);
            }
            remove
            {
                _pageFinishedEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Notify the host application that a page image has ready to use
        /// </summary>
        /// <param name="viewTexture">Page view texture</param>
        void IPageImageReadyListener.OnPageImageReady(Texture2D viewTexture)
        {
            if (_pageImageReadyEvent != null)
                _pageImageReadyEvent.Invoke(viewTexture);
        }

        /// <summary>
        /// Page image has ready to use listener
        /// </summary>
        public event UnityAction<Texture2D> PageImageReadyListener
        {
            add
            {
                _pageImageReadyEvent.AddListener(value);
            }
            remove
            {
                _pageImageReadyEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Report a JavaScript console message to the host application
        /// </summary>
        /// <param name="consoleMessage">Object containing details of the console message</param>
        void IPageConsoleMessageListener.OnPageConsoleMessage(string consoleMessage)
        {
            if (_pageConsoleMessageEvent != null)
                _pageConsoleMessageEvent.Invoke(consoleMessage);
        }

        /// <summary>
        /// A JavaScript console message listener
        /// </summary>
        public event UnityAction<string> PageConsoleMessageListener
        {
            add
            {
                _pageConsoleMessageEvent.AddListener(value);
            }
            remove
            {
                _pageConsoleMessageEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Report web resource loading error to the host application
        /// </summary>
        /// <param name="message">A String describing the error</param>
        void IPageErrorListener.OnPageError(string message)
        {
            if (_pageErrorEvent != null)
                _pageErrorEvent.Invoke(message);
        }

        /// <summary>
        /// Web resource loading error listener
        /// </summary>
        public event UnityAction<string> PageErrorListener
        {
            add
            {
                _pageErrorEvent.AddListener(value);
            }
            remove
            {
                _pageErrorEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Notify the host application that an HTTP error has been received from the server while loading a resource. 
        /// HTTP errors have status codes >= 400. 
        /// This callback will be called for any resource (iframe, image, etc.), not just for the main page
        /// </summary>
        /// <param name="message">Information about the error occurred</param>
        void IPageHttpErrorListener.OnPageHttpError(string message)
        {
            if (_pageHttpErrorEvent != null)
                _pageHttpErrorEvent.Invoke(message);
        }

        /// <summary>
        /// HTTP error has been received listener
        /// </summary>
        public event UnityAction<string> PageHttpErrorListener
        {
            add
            {
                _pageHttpErrorEvent.AddListener(value);
            }
            remove
            {
                _pageHttpErrorEvent.RemoveListener(value);
            }
        }

        /// <summary>
        /// Motion events callback for input system
        /// </summary>
        /// <param name="target">Target that handle the motion event</param>
        /// <param name="action">The motion action that generated on target.</param>
        /// <param name="coord">Motion event position in relative coord (0..1)</param>
        void IInputManagerListener.OnMotionEvents(GameObject target, MotionActions action, Vector2 coord)
        {
            if (_outputObject == target)
            {
                var motionCoord = new Vector2(_viewSize.x * coord.x, _viewSize.y * coord.y);
                _webView.SetMotionEvent(action, motionCoord.x, motionCoord.y);

                if (_motionEvents != null)
                    _motionEvents.Invoke(action, motionCoord);
            }
        }

        /// <summary>
        /// Motion events listener for current component
        /// </summary>
        public event UnityAction<MotionActions, Vector2> MotionEventsListener
        {
            add
            {
                _motionEvents.AddListener(value);
            }
            remove
            {
                _motionEvents.RemoveListener(value);
            }
        }

        /// <summary>
        /// Report web view loading error
        /// </summary>
        /// <param name="message">A String describing the error</param>
        void IErrorListener.OnError(string message)
        {
            if (_errorEvent != null)
                _errorEvent.Invoke(message);
        }

        /// <summary>
        /// Web view loading error listener
        /// </summary>
        public event UnityAction<string> ErrorListener
        {
            add
            {
                _errorEvent.AddListener(value);
            }
            remove
            {
                _errorEvent.RemoveListener(value);
            }
        }

        public void SetMotionEvent(MotionActions action, float x, float y)
        {
            if (_webView != null)
                _webView.SetMotionEvent(action, x, y);
        }
    }
}