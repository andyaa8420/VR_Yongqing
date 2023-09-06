using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TWV
{
    public delegate void JsResultDelegate(string value, string error);

    public class WebView : IWebView
    {
        private object _webViewObject;
        private IWebView _webView;

        #region Constructors
        /// <summary>
        ///  Create new instance of WebView object for current supported platform with device screen size by default
        /// </summary>
        /// <param name="monoObject">MonoBehaviour instanse</param>
        /// <param name="outputObject">WebView rendering object</param>
        public WebView(MonoBehaviour monoObject, GameObject outputObject) : this(monoObject, outputObject, Screen.width, Screen.height) { }

        /// <summary>
        ///  Create new instance of WebView object for current supported platform
        /// </summary>
        /// <param name="monoObject">MonoBehaviour instanse</param>
        /// <param name="outputObject">WebView rendering object</param>
        /// <param name="width">WebView width in pixels (width should be bigger than '0', instead will be used device screen width by default)</param>
        /// <param name="height">WebView height in pixels (height should be bigger than '0', instead will be used device screen height by default)</param>
        public WebView(MonoBehaviour monoObject, GameObject outputObject, int width, int height)
        {
            if (WebViewHelper.IsSupportedPlatform)
            {
                if (width <= 0 || height <= 0)
                {
                    Debug.LogWarning("Unsupported size (width <= 0, height <= 0): will be use device screen size by default");
                    width = Screen.width;
                    height = Screen.height;
                }

                if (Application.platform == RuntimePlatform.Android)
                    _webViewObject = new WebViewAndroid(monoObject, outputObject, width, height);
            }

            if (_webViewObject == null)
            {
                Debug.LogWarning("Runtime platform is unsupported with TWV asset, all functionality will be ignored");
                return;
            }

            if (_webViewObject is IWebView)
                _webView = (_webViewObject as IWebView);
        }
        #endregion

        /// <summary> 
        /// Get web view object for current running platform 
        /// for get more additional possibilities that exists only for this platform.
        /// Will be used in future updates.
        /// </summary> 
        public object PlatformWebView
        {
            get
            {
                return _webViewObject;
            }
        }

        /// <summary>
        /// Get/Set Unity 'GameObject' 
        /// that have 'Mesh Renderer' (with some material) 
        /// or 'Raw Image' component
        /// </summary>
        public GameObject OutputObject
        {
            get
            {
                if (_webView != null)
                    return _webView.OutputObject;

                return null;
            }
            set
            {
                if (_webView != null)
                    _webView.OutputObject = value;
            }
        }

        /// <summary>
        /// Get event manager for current web view
        /// to add possibility to attach/detach special playback listeners
        /// </summary>
        public WebViewManagerEvents EventManager
        {
            get
            {
                if (_webView != null)
                    return _webView.EventManager;

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
        /// Show default device keyboard that allows a user 
        /// to enter data into current active web page input field
        /// </summary>
        public bool DeviceKeyboard
        {
            set
            {
                if (_webView != null)
                    _webView.DeviceKeyboard = value;
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
                if (_webView != null)
                    return _webView.PageUrl;

                return null;
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
        /// Get current web view state
        /// </summary>
        public WebStates State
        {
            get
            {
                if (_webView != null)
                    return _webView.State;

                return WebStates.Empty;
            }
        }

        /// <summary>
        /// Get current web view state value (can be float, long or string type)
        /// </summary>
        public object StateValue
        {
            get
            {
                if (_webView != null)
                    return _webView.StateValue;

                return null;
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

        /// <summary>
        /// Add new main group of listeners to current web view instance 
        /// </summary>
        /// <param name="listener">Group of listeners</param>
        public void AddPageListener(IPageListener listener)
        {
            if (_webView != null)
                _webView.AddPageListener(listener);
        }

        /// <summary>
        /// Remove group of listeners from current web view instance 
        /// </summary>
        /// <param name="listener">Group of listeners</param>
        public void RemovePageListener(IPageListener listener)
        {
            if (_webView != null)
                _webView.RemovePageListener(listener);
        }

        /// <summary>
        /// Load new web page based on current url
        /// </summary>
        /// <param name="url">Page url</param>
        public void LoadUrl(string url)
        {
            if (_webView != null)
                _webView.LoadUrl(url);
        }

        /// <summary>
        /// Load new web page based on current data
        /// </summary>
        /// <param name="data">HTML data</param>
        public void LoadData(string data)
        {
            if (_webView != null)
                _webView.LoadData(data);
        }

        /// <summary>
        /// Unload current web page
        /// </summary>
        /// <param name="resetTexture">Clear the last frame</param>
        public void UnLoad(bool resetTexture)
        {
            if (_webView != null)
                _webView.UnLoad(resetTexture);
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
            if (_webView != null)
                _webView.Release();
        }

        /// <summary>
        /// Goes forward in the history of this WebView.
        /// </summary>
        /// <returns>True if this Webview has a forward history item</returns>
        public bool MoveForward()
        {
            if (_webView != null)
                return _webView.MoveForward();

            return false;
        }

        /// <summary>
        /// Goes back in the history of this WebView.
        /// </summary>
        /// <returns>True if this WebView has a back history item</returns>
        public bool MoveBack()
        {
            if (_webView != null)
                return _webView.MoveBack();

            return false;
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
                if (_webView.Width != width || _webView.Height != height)
                {
                    if (width <= 0)
                    {
                        Debug.LogWarning("Unsupported size (width <= 0): will be minimum possible size");
                        width = 1;
                    }

                    if (height <= 0)
                    {
                        Debug.LogWarning("Unsupported size (height <= 0): will be minimum possible size");
                        height = 1;
                    }

                    _webView.ChangeSize(width, height);
                }
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
        /// Send motion event with the specified coordinates.
        /// </summary>
        /// <param name="action">The motion action that will be sended into web view.</param>
        /// <param name="x">The coordinate of action, along the x-axis (horizontal).</param>
        /// <param name="y">The coordinate of action, along the y-axis (vertical).</param>
        public void SetMotionEvent(MotionActions action, float x, float y)
        {
            if (_webView != null)
                _webView.SetMotionEvent(action, x, y);
        }

        /// <summary>
        /// Asynchronously evaluates JavaScript in the context of the currently displayed page.
        /// </summary>
        /// <param name="script">The JavaScript to execute</param>
        /// <param name="resultCallback">A callback to be invoked when the script execution
        /// completes with the result of the execution(if any).
        /// May be empty if no notification of the result is required.</param>
        public void EvaluateJavascript(string script, JsResultDelegate resultCallback = null)
        {
            if (_webView != null)
                _webView.EvaluateJavascript(script, resultCallback);
        }

        /// <summary>
        /// Click in document to the specified coordinates.
        /// </summary>
        /// <param name="x">The coordinate to click to, along the x-axis (horizontal), in pixels.</param>
        /// <param name="y">The coordinate to click to, along the y-axis (vertical), in pixels.</param>
        public void ClickTo(int x, int y)
        {
            if (_webView != null)
                _webView.ClickTo(x, y);
        }

        /// <summary>
        /// Scrolls the document to the specified coordinates.
        /// </summary>
        /// <param name="x">The coordinate to scroll to, along the x-axis (horizontal), in pixels.</param>
        /// <param name="y">The coordinate to scroll to, along the y-axis (vertical), in pixels.</param>
        public void ScrollBy(int x, int y, float scrollTime = 1f)
        {
            if (_webView != null)
                _webView.ScrollBy(x, y, scrollTime);
        }
    }
}
