using System.Collections;
using UnityEngine;
using TWV.Wrappers;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TWV
{
    public class WebViewAndroid : IWebView
    {
        private static string JS_ERROR_PREFIX = "jsEvaluatorException:";

        private MonoBehaviour _monoObject;
        private WrapperAndroid _wrapper;

        private GameObject _outputObject;
        private Texture2D _viewTexture;
        private Texture2D _tempViewTexture;
        private int _widthPixels;
        private int _heightPixels;

        private bool _isStarted;
        private bool _isReady;
        private bool _isTextureExist;
        private bool _deviceKeyboard;

        private WebViewManagerEvents _eventManager;
        private WebViewBufferPage _pageBuffer;

        private Dictionary<string, JsResultDelegate> _jsCallbacks;

        private IEnumerator _loadPageProcessEnum;
        private IEnumerator _updatePageProcessEnum;
        private IEnumerator _changeSizeProcessEnum;
        private IEnumerator _pageScrollEnum;

        #region Constructors
        /// <summary>
        /// Create new instance of WebView object for Android platform
        /// </summary>
        /// <param name="monoObject">MonoBehaviour instanse</param>
        /// <param name="outputObject">WebView rendering object</param>
        /// <param name="width">WebView width in pixels</param>
        /// <param name="height">WebView height in pixels</param>
        internal WebViewAndroid(MonoBehaviour monoObject, GameObject outputObject, int width, int height)
        {
            _monoObject = monoObject;
            _outputObject = outputObject;
            _widthPixels = width;
            _heightPixels = height;

            _wrapper = new WrapperAndroid(_widthPixels, _heightPixels);
            _eventManager = new WebViewManagerEvents(_monoObject, this);
            _eventManager.JsDataReceivedListener += OnJsDataReceived;
            _jsCallbacks = new Dictionary<string, JsResultDelegate>();
        }
        #endregion

        private IEnumerator LoadPageProcess(string data, bool isUrl = true)
        {
            while (!_wrapper.IsViewReady)
                yield return null;

            if (!_isStarted)
            {
                if (_eventManager != null)
                    _eventManager.StartListener();
            }

            _wrapper.ShowKeyboard = _deviceKeyboard;

            if (isUrl)
            {
                var url = data;

                if (url.StartsWith(WebViewHelper.LOCAL_FILE_ROOT))
                    url = WebViewHelper.GetLocalSourcePath(data);

                if (!string.IsNullOrEmpty(url))
                    _wrapper.LoadUrl = url;
                else
                {
                    _eventManager.SetEvent(WebStates.Error, "Can't find local web page file");
                    UnLoad();
                    yield break;
                }
            }
            else
                _wrapper.LoadData = data;

            if (!_isTextureExist)
            {
                if (_viewTexture != null)
                {
                    UnityEngine.Object.Destroy(_viewTexture);
                    _viewTexture = null;
                }

                _viewTexture = WebViewHelper.GenWebViewTexture(_widthPixels, _heightPixels);
                _wrapper.SetTexture(_viewTexture.GetNativeTexturePtr());

                _isTextureExist = true;
                _isReady = false;
            }

            yield return _wrapper.SendAndWaitPluginEvent(WrapperAndroid.PluginEvents.Start);

            _isStarted = _wrapper.StartRender();

            if (_updatePageProcessEnum == null)
            {
                _updatePageProcessEnum = UpdatePageProcess();
                _monoObject.StartCoroutine(_updatePageProcessEnum);
            }

            if (!_isStarted)
                UnLoad();
        }

        private IEnumerator UpdatePageProcess()
        {
            while (true)
            {
                if (_wrapper.FramesCounter > 0)
                {
                    _wrapper.SendPluginEvent(WrapperAndroid.PluginEvents.Render);

                    if (!_isReady)
                    {
                        WebViewHelper.ApplyTextureToRenderingObject(_viewTexture, _outputObject);
                        _eventManager.SetEvent(WebStates.ImageReady, _viewTexture);
                        _isReady = true;

                        if (_tempViewTexture != null)
                        {
                            UnityEngine.Object.Destroy(_tempViewTexture);
                            _tempViewTexture = null;
                        }
                    }
                }

                yield return null;
            }
        }

        private IEnumerator ChangeSizeProcess(int width, int height)
        {
            if (_updatePageProcessEnum != null)
            {
                _monoObject.StopCoroutine(_updatePageProcessEnum);
                _updatePageProcessEnum = null;
            }

            yield return _wrapper.SendAndWaitPluginEvent(WrapperAndroid.PluginEvents.Stop);
            _wrapper.StopRender();

            _tempViewTexture = _viewTexture;
            _viewTexture = WebViewHelper.GenWebViewTexture(width, height);
            _wrapper.SetTexture(_viewTexture.GetNativeTexturePtr());

            _wrapper.ChangeSize(width, height);
            yield return _wrapper.SendAndWaitPluginEvent(WrapperAndroid.PluginEvents.Start);
            _wrapper.StartRender();

            _widthPixels = width;
            _heightPixels = height;
            _isReady = false;

            if (_updatePageProcessEnum == null)
            {
                _updatePageProcessEnum = UpdatePageProcess();
                _monoObject.StartCoroutine(_updatePageProcessEnum);
            }
        }

        private void OnJsDataReceived(string value)
        {
            int idx = value.LastIndexOf('@');

            try
            {
                var callbackPointer = value.Substring(idx + 1);

                if (!string.IsNullOrEmpty(callbackPointer))
                {
                    if (_jsCallbacks.ContainsKey(callbackPointer))
                    {
                        var resultCallback = _jsCallbacks[callbackPointer];
                        var error = string.Empty;
                        value = value.Substring(0, idx);

                        if (value.StartsWith(JS_ERROR_PREFIX))
                        {
                            error = value.Substring(JS_ERROR_PREFIX.Length);
                            value = string.Empty;
                        }

                        resultCallback(value, error);
                        _jsCallbacks.Remove(callbackPointer);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("OnJsDataReceived: Can't cast callback pointer - " + e);
            }
        }

        /// <summary>
        /// Get/Set Unity 'GameObject' 
        /// that have 'Mesh Renderer' (with some material) 
        /// or 'Raw Image' component
        /// </summary>
        public GameObject OutputObject
        {
            set
            {
                if (_outputObject != null)
                    WebViewHelper.ApplyTextureToRenderingObject(null, _outputObject);

                _outputObject = value;

                if (_viewTexture != null)
                    WebViewHelper.ApplyTextureToRenderingObject(_viewTexture, _outputObject);
            }

            get { return _outputObject; }
        }

        /// <summary>
        /// Get event manager for current web view
        /// to add possibility to attach/detach special playback listeners
        /// </summary>
        public WebViewManagerEvents EventManager
        {
            get { return _eventManager; }
        }


        /// <summary>
        /// Is web view is ready (first frame available)
        /// </summary>
        public bool IsReady
        {
            get { return _isReady; }
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
                return _wrapper.DisplayDensity;
            }
        }

        /// <summary>
        /// Show default device keyboard that allows a user 
        /// to enter data into current active web page input field
        /// </summary>
        public bool DeviceKeyboard
        {
            set
            {
                //WebView can be not available in view group, so we need to cache this value
                _deviceKeyboard = value;
                _wrapper.ShowKeyboard = _deviceKeyboard;
            }
        }

        /// <summary>
        /// Gets the URL for the current page. This is not always the same as the URL passed to Load, 
        /// because the current page can be changed.
        /// </summary>
        public string PageUrl
        {
            get
            {
                return _wrapper.PageUrl;
            }
        }

        /// <summary>
        /// Get pixels of current web page frame 
        /// Example of using:
        /// texture.LoadRawTextureData(_webView.FramePixels);
        /// texture.Apply(); 
        /// </summary>
        public byte[] FramePixels
        {
            get
            {
                if (_pageBuffer == null || (_pageBuffer != null && _pageBuffer.Width != _widthPixels && _pageBuffer.Height != _heightPixels))
                {
                    if (_pageBuffer != null)
                        _pageBuffer.ClearFramePixels();

                    _pageBuffer = new WebViewBufferPage(_widthPixels, _heightPixels);
                    _wrapper.SetTextureBuffer(_pageBuffer.FramePixelsAddr);
                }

                _wrapper.SendPluginEvent(WrapperAndroid.PluginEvents.Buffer);
                return _pageBuffer.FramePixels;
            }
        }

        /// <summary>
        /// Get current web view state
        /// </summary>
        public WebStates State
        {
            get
            {
                return _wrapper.State;
            }
        }

        /// <summary>
        /// Get current web view state value (can be float, long or string type)
        /// </summary>
        public object StateValue
        {
            get
            {
                return _wrapper.StateValue;
            }
        }

        /// <summary>
        /// Get the width of your view, in pixels.
        /// </summary>
        public int Width
        {
            get
            {
                return _widthPixels;
            }
        }

        /// <summary>
        /// Get the height of your view, in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                return _heightPixels;
            }
        }

        #region Platform dependent properties
        /// <summary>
        /// Enable/Disable long click logic, usable in VR to disable text selection.
        /// </summary>
        public bool UseLongClick
        {
            set
            {
                _wrapper.UseLongClick = value;
            }
        }
        #endregion

        /// <summary>
        /// Add new main group of listeners to current web view instance 
        /// </summary>
        /// <param name="listener">Group of listeners</param>
        public void AddPageListener(IPageListener listener)
        {
            if (_eventManager != null)
            {
                _eventManager.PageStartedListener += listener.OnPageStarted;
                _eventManager.PageLoadingListener += listener.OnPageLoading;
                _eventManager.PageFinishedListener += listener.OnPageFinished;
                _eventManager.PageImageReadyListener += listener.OnPageImageReady;
                _eventManager.PageConsoleMessageListener += listener.OnPageConsoleMessage;
                _eventManager.PageErrorListener += listener.OnPageError;
                _eventManager.PageHttpErrorListener += listener.OnPageHttpError;
            }
        }

        /// <summary>
        /// Remove group of listeners from current web view instance 
        /// </summary>
        /// <param name="listener">Group of listeners</param>
        public void RemovePageListener(IPageListener listener)
        {
            if (_eventManager != null)
            {
                _eventManager.PageStartedListener -= listener.OnPageStarted;
                _eventManager.PageLoadingListener -= listener.OnPageLoading;
                _eventManager.PageFinishedListener -= listener.OnPageFinished;
                _eventManager.PageImageReadyListener -= listener.OnPageImageReady;
                _eventManager.PageConsoleMessageListener -= listener.OnPageConsoleMessage;
                _eventManager.PageErrorListener -= listener.OnPageError;
                _eventManager.PageHttpErrorListener -= listener.OnPageHttpError;
            }
        }

        /// <summary>
        /// Load new web page based on current url
        /// </summary>
        /// <param name="url">Page url</param>
        public void LoadUrl(string url)
        {
            if (_loadPageProcessEnum != null)
                _monoObject.StopCoroutine(_loadPageProcessEnum);

            _loadPageProcessEnum = LoadPageProcess(url);
            _monoObject.StartCoroutine(_loadPageProcessEnum);
        }

        /// <summary>
        /// Load new web page based on current data
        /// </summary>
        /// <param name="data">HTML data</param>
        public void LoadData(string data)
        {
            if (_loadPageProcessEnum != null)
                _monoObject.StopCoroutine(_loadPageProcessEnum);

            _loadPageProcessEnum = LoadPageProcess(data, false);
            _monoObject.StartCoroutine(_loadPageProcessEnum);
        }

        /// <summary>
        /// Unload current web page
        /// </summary>
        /// <param name="resetTexture">Clear the last frame</param>
        public void UnLoad(bool resetTexture)
        {
            _wrapper.StopRender();
            _wrapper.SendPluginEvent(WrapperAndroid.PluginEvents.Stop);

            if (_loadPageProcessEnum != null)
            {
                _monoObject.StopCoroutine(_loadPageProcessEnum);
                _loadPageProcessEnum = null;
            }

            if (_updatePageProcessEnum != null)
            {
                _monoObject.StopCoroutine(_updatePageProcessEnum);
                _updatePageProcessEnum = null;
            }

            _isStarted = false;
            _isReady = false;
            _isTextureExist = !resetTexture;

            if (resetTexture)
            {
                if (_viewTexture != null)
                {
                    UnityEngine.Object.Destroy(_viewTexture);
                    _viewTexture = null;
                }
            }

            if (_tempViewTexture != null)
            {
                UnityEngine.Object.Destroy(_tempViewTexture);
                _tempViewTexture = null;
            }

            if (_eventManager != null)
                _eventManager.StopListener();

            _jsCallbacks.Clear();
        }

        /// <summary>
        /// Unload current web page and clear last frame
        /// </summary>
        public void UnLoad()
        {
            UnLoad(true);
        }

        /// <summary>
        /// Release a current web view instance 
        /// </summary>
        public void Release()
        {
            UnLoad();

            if (_updatePageProcessEnum != null)
            {
                _monoObject.StopCoroutine(_updatePageProcessEnum);
                _updatePageProcessEnum = null;
            }

            if (_eventManager != null)
            {
                _eventManager.RemoveAllEvents();
                _eventManager = null;
            }

            _wrapper.Release();
        }

        /// <summary>
        /// Goes forward in the history of this WebView.
        /// </summary>
        /// <returns>True if this Webview has a forward history item</returns>
        public bool MoveForward()
        {
            return _wrapper.MoveForward();
        }

        /// <summary>
        /// Goes back in the history of this WebView.
        /// </summary>
        /// <returns>True if this WebView has a back history item</returns>
        public bool MoveBack()
        {
            return _wrapper.MoveBack();
        }

        /// <summary>
        /// Change the size of view (should be > 0)
        /// </summary>
        /// <param name="width">The new width for current view</param>
        /// <param name="height">The new height for current view</param>
        public void ChangeSize(int width, int height)
        {
            if (_changeSizeProcessEnum != null)
                _monoObject.StopCoroutine(_changeSizeProcessEnum);

            _changeSizeProcessEnum = ChangeSizeProcess(width, height);
            _monoObject.StartCoroutine(_changeSizeProcessEnum);
        }

        /// <summary>
        /// Set text to current active input form
        /// </summary>
        /// <param name="text">Text that will be set to input form</param>
        public void SetInputText(string text)
        {
            _wrapper.SetInputText(text);
        }

        /// <summary>
        /// Send motion event with the specified coordinates.
        /// </summary>
        /// <param name="action">The motion action that will be sended into web view.</param>
        /// <param name="x">The coordinate of action, along the x-axis (horizontal).</param>
        /// <param name="y">The coordinate of action, along the y-axis (vertical).</param>
        public void SetMotionEvent(MotionActions action, float x, float y)
        {
            _wrapper.SetMotionEvent(action, x, y);
        }

        private Action<string> _resultCallback;

        /// <summary>
        /// Asynchronously evaluates JavaScript in the context of the currently displayed page.
        /// </summary>
        /// <param name="script">The JavaScript to execute</param>
        /// <param name="resultCallback">A callback to be invoked when the script execution
        /// completes with the result of the execution(if any).
        /// May be empty if no notification of the result is required.</param>
        public void EvaluateJavascript(string script, JsResultDelegate resultCallback)
        {
            var callbackPointer = IntPtr.Zero;

            if (resultCallback != null)
            {
                callbackPointer = Marshal.GetFunctionPointerForDelegate(resultCallback);
                _jsCallbacks.Add(callbackPointer.ToString(), resultCallback);
            }

            _wrapper.EvaluateJavascript(script, callbackPointer, JS_ERROR_PREFIX);
        }

        /// <summary>
        /// Click in document to the specified coordinates.
        /// </summary>
        /// <param name="x">The coordinate to click to, along the x-axis (horizontal), in pixels.</param>
        /// <param name="y">The coordinate to click to, along the y-axis (vertical), in pixels.</param>
        public void ClickTo(int x, int y)
        {
            SetMotionEvent(MotionActions.Began, x, y);
            SetMotionEvent(MotionActions.Ended, x, y);
        }

        /// <summary>
        /// Scrolls the document to the specified coordinates.
        /// </summary>
        /// <param name="x">The coordinate to scroll to, along the x-axis (horizontal), in pixels.</param>
        /// <param name="y">The coordinate to scroll to, along the y-axis (vertical), in pixels.</param>
        public void ScrollBy(int x, int y, float scrollTime = 1f)
        {
            if (_pageScrollEnum != null)
                _monoObject.StopCoroutine(_pageScrollEnum);

            _pageScrollEnum = PageScrollHandler(x, y, scrollTime);
            _monoObject.StartCoroutine(_pageScrollEnum);
        }

        private IEnumerator PageScrollHandler(int x, int y, float scrollTime)
        {
            var progress = 0f;
            var pointStart = new Vector2(_widthPixels * 0.5f, _heightPixels * 0.5f);
            var pointEnd = new Vector2(pointStart.x + (x * _wrapper.DisplayDensity), pointStart.y + (y * _wrapper.DisplayDensity));

            SetMotionEvent(MotionActions.Began, pointStart.x, pointStart.y);
            while (progress < 1)
            {
                progress += Mathf.Clamp01(Time.deltaTime / scrollTime);
                var pointScroll = Vector2.Lerp(pointStart, pointEnd, progress);
                
                SetMotionEvent(MotionActions.Moved, (int)pointScroll.x, (int)pointScroll.y);
                yield return null;
            }

            SetMotionEvent(MotionActions.Ended, pointEnd.x, pointEnd.y);
        }
    }
}
